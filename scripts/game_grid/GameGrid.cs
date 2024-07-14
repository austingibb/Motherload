using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;

public partial class GameGrid : TileMap
{
    [Export]
    public int ChunkWidth = 8;

    [Export]
    public int ChunkSize = 4;

    [Export]
    public int ChunkPadding = 2;

    public int Width;
    public int Seed;

    [Export]
    public NoiseTexture2D EmptyTileNoiseTexture;
    [Export]
    Godot.Collections.Array<DrillableType> drillableType = new Godot.Collections.Array<DrillableType>();
    [Export]
    Godot.Collections.Array<Curve> toDrillableProbabilityCurve = new Godot.Collections.Array<Curve>();
    [Export]
    Godot.Collections.Array<Vector2I> drillableTypeDepthRanges = new Godot.Collections.Array<Vector2I>();

    [Export]
    Godot.Collections.Array<ChestType> chestType = new Godot.Collections.Array<ChestType>();
    [Export]
    Godot.Collections.Array<Curve> toChestProbabilityCurve = new Godot.Collections.Array<Curve>();
    [Export]
    Godot.Collections.Array<Vector2I> chestTypedepthRanges = new Godot.Collections.Array<Vector2I>();

    [Signal]
    public delegate void drillableDugEventHandler(Drillable drillable);
    [Signal]
    public delegate void itemSpawnedEventHandler(ChunkItemType gameGridItemType, Node2D gameGridItem);

    private GameGridProbability<DrillableType> drillableGridProbability;
    private GameGridProbability<ChestType> chestGridProbability;
    private List<Node2D> buildings;

    private FastNoiseLite emptyTileNoise;
    private Dictionary<Vector2I, GameGridChunk> gameGridChunks = new Dictionary<Vector2I, GameGridChunk>(); 
    private Dictionary<Vector2I, GameGridChunk> activeGameGridChunks = new Dictionary<Vector2I, GameGridChunk>(); 
    private Dictionary<Vector2I, Object> chunksToSpawn = new Dictionary<Vector2I, object>();
    private Dictionary<Vector2I, int> positionToTileSetId = new Dictionary<Vector2I, int>();
    private Dictionary<Vector2I, int> positionToItemTileSetId = new Dictionary<Vector2I, int>();
    private Dictionary<Vector2I, Tile> positionToSolidTile = new Dictionary<Vector2I, Tile>();
    private Dictionary<Vector2I, GameGridItemTile> positionToGridItems = new Dictionary<Vector2I, GameGridItemTile>();
    private List<ChunkItem> chunkItems = new List<ChunkItem>();

    public override void _Ready()
    {
        Width = ChunkSize * ChunkWidth;
        RandomNumberGenerator rngSeedGen = new RandomNumberGenerator();
        rngSeedGen.Seed = 123234232;
        rngSeedGen.Randomize();
        Seed = (int) (rngSeedGen.Randi() % Mathf.Pow(2, 25));
        GD.Print("World seed: " + Seed);

        emptyTileNoise = EmptyTileNoiseTexture.Noise as FastNoiseLite;
        emptyTileNoise.Seed = Seed;

        drillableGridProbability = new GameGridProbability<DrillableType>(drillableType, DrillableType.DIRT, toDrillableProbabilityCurve, drillableTypeDepthRanges, Seed);
        chestGridProbability = new GameGridProbability<ChestType>(chestType, ChestType.None, toChestProbabilityCurve, chestTypedepthRanges, Seed);

        UpdateInternals();
        foreach (KeyValuePair<Vector2I, Tile> entry in positionToSolidTile)
        {
            Vector2I gridPosition = entry.Key;
            Tile tile = entry.Value;
            if (Common.ValidTile(tile))
            {
                EraseCell(1, GridPositionToTileMapPosition(gridPosition));
                positionToSolidTile.Remove(gridPosition);
            }
        }
    }

    public void Update(Godot.Vector2 playerPosition)
    {
        Vector2I playerChunkPos;
        Vector2I playerChunkOffset;
        TileMapPositionToChunkPos(LocalToMap(playerPosition), out playerChunkPos, out playerChunkOffset);

        foreach (KeyValuePair<Vector2I, GameGridChunk> entry in activeGameGridChunks)
        {
            Vector2I chunkPos = entry.Key;
            if (Math.Abs(chunkPos.X - playerChunkPos.X) > ChunkPadding || Math.Abs(chunkPos.Y - playerChunkPos.Y) > ChunkPadding)
            {
                DespawnChunk(chunkPos);
            }
        }

        for (int i = -ChunkPadding; i <= ChunkPadding; i++)
        {
            for (int j = -ChunkPadding; j <= ChunkPadding; j++)
            {

                Vector2I chunkPos = playerChunkPos + new Vector2I(i, j);
                if (chunkPos.X < 0 || chunkPos.X >= ChunkWidth || chunkPos.Y < 0)
                {
                    continue;
                }

                if (!activeGameGridChunks.ContainsKey(chunkPos))
                {
                    chunksToSpawn[chunkPos] = null;
                }
            }
        }

        if (chunksToSpawn.Count > 0)
        {
            var chunksToSpawnEnumerator = chunksToSpawn.GetEnumerator();
            chunksToSpawnEnumerator.MoveNext();
            Vector2I chunkToSpawn = chunksToSpawnEnumerator.Current.Key;
            SpawnChunk(chunkToSpawn);
            chunksToSpawn.Remove(chunkToSpawn);
        }
    }

    public void SetBuildings(List<Node2D> buildings = null)
    {
        this.buildings = buildings;   
    }

    public GameGridChunk GenerateChunk(Vector2I chunkPosition)
    {
        GameGridChunk gameGridChunk = new GameGridChunk(ChunkSize);
        for (int i = 0; i < ChunkSize; i++)
        {
            for (int j = 0; j < ChunkSize; j++)
            {
                int tileId;
                int itemTileSetId;
                Vector2I gridPos = ChunkPosToGridPosition(chunkPosition, new Vector2I(i, j));
                PopulateTile(gridPos, out tileId, out itemTileSetId);
                gameGridChunk.SetTile(new Vector2I(i, j), tileId, itemTileSetId);
                positionToTileSetId.Add(gridPos, tileId);
                positionToItemTileSetId.Add(gridPos, itemTileSetId);
            }
        }
        return gameGridChunk;
    }

    public void SpawnChunk(Vector2I chunkPosition)
    {
        if (chunkPosition.X < 0 || chunkPosition.X >= ChunkWidth || chunkPosition.Y < 0)
        {
            return;
        }

        GameGridChunk chunk;
        if (!gameGridChunks.ContainsKey(chunkPosition))
        {
            chunk = GenerateChunk(chunkPosition);
            gameGridChunks.Add(chunkPosition, chunk);
        } else {
            chunk = gameGridChunks[chunkPosition];
        }

        for (int i = 0; i < ChunkSize; i++)
        {
            for (int j = 0; j < ChunkSize; j++)
            {
                Vector2I tileMapPos = ChunkPosToTileMapPosition(chunkPosition, new Vector2I(i, j));
                SetCell(0, tileMapPos, 1, new Vector2I(0, 0), GameGridConstants.DIRT_BACKGROUND_TILE_SET_ID);

                int? tileSetId;
                int? itemTileSetId;
                chunk.GetTile(new Vector2I(i, j), out tileSetId, out itemTileSetId);
                if (tileSetId != null && tileSetId != -1)
                {
                    SetCell(1, tileMapPos, 1, new Vector2I(0, 0), (int) tileSetId);
                }

                if (itemTileSetId != null && itemTileSetId != -1)
                {
                    SetCell(2, tileMapPos, 1, new Vector2I(0, 0), (int) itemTileSetId);
                }
            }
        }

        UpdateInternals();
        UpdateSpawnedChunkDrillableEdges(chunkPosition);
        activeGameGridChunks.Add(chunkPosition, chunk);

        foreach (ChunkItem chunkItem in chunk.chunkItems)
        {
            chunkItem.Enable();
        }
        chunk.ClearChunkItems();
    }

    public void DespawnChunk(Vector2I chunkPosition)
    {
        if (!gameGridChunks.ContainsKey(chunkPosition))
        {
            return;
        }

        GameGridChunk chunk = gameGridChunks[chunkPosition];
        for (int i = 0; i < ChunkSize; i++)
        {
            for (int j = 0; j < ChunkSize; j++)
            {
                Vector2I tileMapPos = ChunkPosToTileMapPosition(chunkPosition, new Vector2I(i, j));
                Vector2I gridPos = ChunkPosToGridPosition(chunkPosition, new Vector2I(i, j));
                Vector2I chunkOffset = new Vector2I(i, j);

                if (positionToSolidTile.ContainsKey(gridPos))
                {
                    positionToSolidTile.Remove(gridPos);
                }

                if (positionToGridItems.ContainsKey(gridPos))
                {
                    positionToGridItems.Remove(gridPos);
                }
                
                EraseCell(0, tileMapPos);
                int? tileSetId;
                int? itemTileSetId;
                chunk.GetTile(chunkOffset, out tileSetId, out itemTileSetId);
                if (tileSetId != null && tileSetId != -1)
                {
                    EraseCell(1, tileMapPos);
                }
                if (itemTileSetId != null && itemTileSetId != -1)
                {
                    EraseCell(2, tileMapPos);
                }
            }
        }
        activeGameGridChunks.Remove(chunkPosition);


        foreach (ChunkItem chunkItem in chunkItems)
        {
            Vector2I chunkItemChunkPosition;
            Vector2I chunkItemChunkOffset;
            TileMapPositionToChunkPos(LocalToMap(chunkItem.GetPosition()), out chunkItemChunkPosition, out chunkItemChunkOffset);
            if (chunkItemChunkPosition == chunkPosition)
            {
                chunkItem.Disable();
                chunk.AddChunkItem(chunkItem);
            }
        }
    }

    public void UpdateSpawnedChunkDrillableEdges(Vector2I chunkPosition)
    {
        for (int i = -1; i < ChunkSize + 1; i++)
        {
            for (int j = -1; j < ChunkSize + 1; j++)
            {
                Vector2I gridPos = ChunkPosToGridPosition(chunkPosition, new Vector2I(i, j));
                if (positionToSolidTile.ContainsKey(gridPos))
                {
                    Tile drillable = positionToSolidTile[gridPos];
                    if (Common.ValidTile(drillable))
                    {
                        UpdateSolidTileCorner(drillable, gridPos, SolidTileCorner.TopLeft);
                        UpdateSolidTileCorner(drillable, gridPos, SolidTileCorner.TopRight);
                        UpdateSolidTileCorner(drillable, gridPos, SolidTileCorner.BottomLeft);
                        UpdateSolidTileCorner(drillable, gridPos, SolidTileCorner.BottomRight);
                    }
                }
            }
        }
    }

    public void PopulateTile(Vector2I position, out int tileSetId, out int itemTileSetId)
    {
        int posX = position.X;
        int posY = position.Y;

        if (posX == 0 || posX == ChunkWidth * ChunkSize - 1)
        {
            tileSetId = GameGridConstants.DIRT_NON_DRILLABLE_TILE_SET_ID;
            itemTileSetId = -1;
            return;
        }

        var emptyTileNoiseValue = emptyTileNoise.GetNoise2D(posX, posY);                

        if (emptyTileNoiseValue < -0.3) {
            tileSetId = -1;
            itemTileSetId = -1;
        } else {
            ChestType chestType = chestGridProbability.GetTypeForDepth((uint) posY);
            if (chestType == ChestType.None) 
            {
                DrillableType type = drillableGridProbability.GetTypeForDepth((uint) posY);
                tileSetId = GameGridConstants.MapDrillableTypeToTileSetId(type);
                itemTileSetId = -1;
            } else
            {
                tileSetId = GameGridConstants.DIRT_TILE_SET_ID;
                itemTileSetId = GameGridConstants.MapChestTypeToTileSetId(chestType);
            }
        }

        bool tileOverlapsBuilding = posY == 0 && buildings != null && buildings.Any((building) => {
            RectangleShape2D buildingShape = (RectangleShape2D) building.GetNode<CollisionShape2D>("CollisionShape2D").Shape;
            Godot.Vector2 buildingPosition = building.Position;
            float xPos = TileSet.TileSize.X * (posX - Width / 2);
            float overlap = Common.LineOverlap(buildingPosition.X - buildingShape.Size.X / 2, buildingPosition.X + buildingShape.Size.X / 2, 
                xPos, xPos + TileSet.TileSize.X);
            return overlap >= TileSet.TileSize.X / 3;
        });

        if (tileOverlapsBuilding)
        {
            tileSetId = GameGridConstants.DIRT_NON_DRILLABLE_STONE_TOP_TILE_SET_ID;
            itemTileSetId = -1;
        }
    }

    public void UpdateSurroundingSolidTileEdges(Vector2I gridPosition)
    {
        for(int i = gridPosition.X - 1; i <= gridPosition.X + 1; i++)
        {
            for(int j = gridPosition.Y - 1; j <= gridPosition.Y + 1; j++)
            {
                Vector2I surroundingGridPosition = new Vector2I(i, j);
                if (positionToSolidTile.ContainsKey(new Vector2I(i, j)))
                { 
                    Tile neighborSolidTile = positionToSolidTile[surroundingGridPosition];
                    if (!Common.ValidTile(neighborSolidTile))
                    {
                        continue;
                    }
                    // Animation for drillable finishes and reveals the corners prior to animation transparancy mask covering them
                    // as such, now that the animation is over set the corners to straigth to match the dug out area
                    if (i == gridPosition.X && j == gridPosition.Y)
                    {
                        SolidTileShaderManager.UpdateSolidTileCorner(neighborSolidTile, SolidTileCorner.TopLeft, SolidTileCornerShape.Straight);
                        SolidTileShaderManager.UpdateSolidTileCorner(neighborSolidTile, SolidTileCorner.TopRight, SolidTileCornerShape.Straight);
                        SolidTileShaderManager.UpdateSolidTileCorner(neighborSolidTile, SolidTileCorner.TopLeft, SolidTileCornerShape.Straight);
                        SolidTileShaderManager.UpdateSolidTileCorner(neighborSolidTile, SolidTileCorner.TopRight, SolidTileCornerShape.Straight);
                    } else 
                    {
                        UpdateSolidTileCorner(neighborSolidTile, new Vector2I(i, j), SolidTileCorner.TopLeft);
                        UpdateSolidTileCorner(neighborSolidTile, new Vector2I(i, j), SolidTileCorner.TopRight);
                        UpdateSolidTileCorner(neighborSolidTile, new Vector2I(i, j), SolidTileCorner.BottomLeft);
                        UpdateSolidTileCorner(neighborSolidTile, new Vector2I(i, j), SolidTileCorner.BottomRight);
                    }
                }
            }
        }   
    }

    private void UpdateSolidTileCorner(Tile solidTile, Vector2I gridPos, SolidTileCorner corner)
    {
        int yFlip = 1;
        if (corner == SolidTileCorner.BottomLeft || corner == SolidTileCorner.BottomRight)
        {
            yFlip = -1;
        }
        int xFlip = 1;
        if (corner == SolidTileCorner.TopRight || corner == SolidTileCorner.BottomRight)
        {
            xFlip = -1;
        }

        bool hasSolidTileAbove = HasNeighbor(new Vector2I(0, -1 * yFlip), gridPos);
        bool hasSolidTileDiagonal = HasNeighbor(new Vector2I(-1 * xFlip, -1 * yFlip), gridPos);
        bool hasSolidTileSide = HasNeighbor(new Vector2I(-1 * xFlip, 0), gridPos);
        
        SolidTileShaderManager.UpdateSolidTileSide(solidTile, (xFlip == 1) ? SolidTileSurface.Left : SolidTileSurface.Right, !hasSolidTileSide);
        SolidTileShaderManager.UpdateSolidTileSide(solidTile, (yFlip == 1) ? SolidTileSurface.Top : SolidTileSurface.Bottom, !hasSolidTileAbove);

        SolidTileCornerShape solidTileCornerShape = SolidTileCornerShape.None;
        int tileSetId = GetCellAlternativeTile(1, GridPositionToTileMapPosition(gridPos));
        if (hasSolidTileAbove || (tileSetId == GameGridConstants.DIRT_NON_DRILLABLE_STONE_TOP_TILE_SET_ID && (corner == SolidTileCorner.TopLeft || corner == SolidTileCorner.TopRight)))
        {
            solidTileCornerShape = SolidTileCornerShape.Straight;
        } else {
            if (hasSolidTileDiagonal) 
            {
                solidTileCornerShape = SolidTileCornerShape.Concave;
            } else
            {
                solidTileCornerShape = hasSolidTileSide ? SolidTileCornerShape.Straight : SolidTileCornerShape.Convex;
            }
        }

        SolidTileShaderManager.UpdateSolidTileCorner(solidTile, corner, solidTileCornerShape);
    }

    public void TileMapPositionToChunkPos(Vector2I tileMapPosition, out Vector2I chunkPos, out Vector2I chunkOffset)
    {
        Vector2I gridPosition = TileMapPositionToGridPosition(tileMapPosition);
        GridPositionToChunkPos(gridPosition, out chunkPos, out chunkOffset);
    }

    public void GridPositionToChunkPos(Vector2I gridPosition, out Vector2I chunkPos, out Vector2I chunkOffset)
    {
        chunkPos = new Vector2I(gridPosition.X / ChunkSize, gridPosition.Y / ChunkSize);
        chunkOffset = new Vector2I(gridPosition.X % ChunkSize, gridPosition.Y % ChunkSize);
    }

    public Vector2I ChunkPosToTileMapPosition(Vector2I chunkPos, Vector2I chunkOffset)
    {
        return GridPositionToTileMapPosition(ChunkPosToGridPosition(chunkPos, chunkOffset));
    }

    public Vector2I ChunkPosToGridPosition(Vector2I chunkPos, Vector2I chunkOffset)
    {
        return new Vector2I(chunkPos.X * ChunkSize + chunkOffset.X, chunkPos.Y * ChunkSize + chunkOffset.Y);
    }

    public Vector2I GridPositionToTileMapPosition(Vector2I gridPosition)
    {
        return new Vector2I(gridPosition.X - Width / 2, gridPosition.Y);
    }

    public Vector2I TileMapPositionToGridPosition(Vector2I tileMapPosition)
    {
        return new Vector2I(tileMapPosition.X + Width / 2, tileMapPosition.Y);
    }

    private bool HasNeighbor(Vector2I relativePosition, Vector2I gridPosition)
    {
        Vector2I neighborPosition = gridPosition + relativePosition;
        if (neighborPosition.X < 0 || neighborPosition.X >= Width)
        {
            return true;
        }

        bool containsKey = positionToSolidTile.ContainsKey(neighborPosition);

        return containsKey && Common.ValidTile(positionToSolidTile[neighborPosition]);
    }

    private Tile GetNeighbor(Vector2I relativePosition, Vector2I gridPosition)
    {
        Vector2I neighborPosition = gridPosition + relativePosition;
        if (neighborPosition.X < 0 || neighborPosition.X >= Width)
        {
            return null;
        }

        if (positionToSolidTile.ContainsKey(neighborPosition) && Common.ValidTile(positionToSolidTile[neighborPosition]))
        {
            return positionToSolidTile[neighborPosition];
        } else {
            return null;
        }
    }

    public void GetSurroundingDrillables(Godot.Vector2 position, List<List<Drillable>> surroundingDrillables)
    {
        Vector2I mapPos = LocalToMap(position);
        Vector2I gridPos = TileMapPositionToGridPosition(mapPos);
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Vector2I surroundingGridPos = gridPos + new Vector2I(i, j);
                if (positionToSolidTile.ContainsKey(surroundingGridPos) && positionToSolidTile[surroundingGridPos].tileType == TileType.Drillable
                    && Common.ValidTile(positionToSolidTile[surroundingGridPos]))
                {
                    surroundingDrillables[i+1][j+1] = (Drillable) positionToSolidTile[surroundingGridPos];
                    continue;
                }
                surroundingDrillables[i+1][j+1] = null;
            }
        }
    }

    public int GetDepth(Node2D node2D)
    {
        Vector2I tileMapPosition = LocalToMap(node2D.Position);
        int depth = TileMapPositionToGridPosition(tileMapPosition).Y + 1;
        if (depth < 0)
        {
            return 0;
        }
        return depth;
    }

    public void _on_drillable_pre_dug(Drillable drillable, DrillFromDirection direction)
    {
        Vector2I gridPos = TileMapPositionToGridPosition(LocalToMap(drillable.Position));

        Tile aboveTile = null;
        Tile belowTile = null;
        SolidTileCorner? bottomCorner = null;
        SolidTileCorner? topCorner = null;

        if (direction == DrillFromDirection.LEFT)
        {
            aboveTile = GetNeighbor(new Vector2I(-1, -1), gridPos);
            belowTile = GetNeighbor(new Vector2I(-1, 1), gridPos);
            bottomCorner = SolidTileCorner.BottomRight;
            topCorner = SolidTileCorner.TopRight;
        } else if (direction == DrillFromDirection.RIGHT)
        {
            aboveTile = GetNeighbor(new Vector2I(1, -1), gridPos);
            belowTile = GetNeighbor(new Vector2I(1, 1), gridPos);
            bottomCorner = SolidTileCorner.BottomLeft;
            topCorner = SolidTileCorner.TopLeft;
        }

        if (aboveTile != null && bottomCorner != null)
        {
            SolidTileShaderManager.UpdateSolidTileCorner(aboveTile, (SolidTileCorner) bottomCorner, SolidTileCornerShape.Straight);
        }
        if (belowTile != null && topCorner != null)
        {
            SolidTileShaderManager.UpdateSolidTileCorner(belowTile, (SolidTileCorner) topCorner, SolidTileCornerShape.Straight);
        }
    }

    public void ClearGridPosition(Vector2I gridPosition)
    {
        Vector2I chunkPos;
        Vector2I chunkOffset;
        GridPositionToChunkPos(gridPosition, out chunkPos, out chunkOffset);
        GameGridChunk chunk = gameGridChunks[chunkPos];
        chunk.SetTile(chunkOffset, -1, -1);

        positionToTileSetId.Remove(gridPosition);
        positionToItemTileSetId.Remove(gridPosition);
        positionToSolidTile.Remove(gridPosition);
        positionToGridItems.Remove(gridPosition);
    }

    public void _on_drillable_dug(Drillable drillable)
    {
        EmitSignal(SignalName.drillableDug, (Drillable) drillable);
        Vector2I tileMapPosition = LocalToMap(drillable.Position);
        Vector2I gridPosition = TileMapPositionToGridPosition(tileMapPosition);

        if (positionToGridItems.ContainsKey(gridPosition)) 
        {
            GameGridItemTile gameGridItem = positionToGridItems[gridPosition];
            if (Common.ValidTile(gameGridItem))
            {
                Node2D spawnedItem = gameGridItem.SpawnItem();
                if (spawnedItem != null)
                {
                    positionToGridItems[gridPosition] = null;
                    EmitSignal(SignalName.itemSpawned, (int) gameGridItem.gameGridItemType, spawnedItem);
                }

                if (spawnedItem is ChunkItem)
                {
                    chunkItems.Add(spawnedItem as ChunkItem);
                }
            }
        }

        ClearGridPosition(gridPosition);
        UpdateSurroundingSolidTileEdges(gridPosition);
    }

    public void _on_tile_ready(Tile tile)
    {
        Vector2I tileMapPosition = LocalToMap(tile.Position);
        Vector2I gridPosition = TileMapPositionToGridPosition(tileMapPosition);
        if (tile.tileType == TileType.Drillable)
        {
            Drillable drillable = tile as Drillable;
            positionToSolidTile[gridPosition] = drillable;
            drillable.dug += _on_drillable_dug;
            drillable.preDug += _on_drillable_pre_dug;
        } else if (tile.tileType == TileType.GameGridItem)
        {
            GameGridItemTile gameGridItemTile = tile as GameGridItemTile;
            positionToGridItems[gridPosition] = gameGridItemTile;
        } else if (tile.tileType == TileType.Background)
        {
            tile.ZIndex = -2;
            if (tileMapPosition.Y == 0)
            {
                SolidTileShaderManager.UpdateSolidTileCorner(tile, SolidTileCorner.TopLeft, SolidTileCornerShape.Straight);
                SolidTileShaderManager.UpdateSolidTileCorner(tile, SolidTileCorner.TopRight, SolidTileCornerShape.Straight);
                SolidTileShaderManager.UpdateSolidTileSide(tile, SolidTileSurface.Top, true);
            }
        } else if (tile.tileType == TileType.NonDrillable)
        {
            positionToSolidTile[gridPosition] = tile;
        }
    }
}
