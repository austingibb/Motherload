using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public partial class DrillableGrid : TileMap
{
    [Export]
    public int Width = 80;
    [Export]
    public int StartingRows = 250;
    [Export]
    public NoiseTexture2D EmptyTileNoiseTexture;
    [Export]
    Godot.Collections.Array<DrillableType> drillableType = new Godot.Collections.Array<DrillableType>();
    [Export]
    Godot.Collections.Array<Curve> toCurve = new Godot.Collections.Array<Curve>();
    [Export]
    Godot.Collections.Array<Vector2I> depthRanges = new Godot.Collections.Array<Vector2I>();

    [Signal]
    public delegate void drillableDugEventHandler(Drillable drillable);
    
    private DrillableGridProbability drillableGridProbability;
    private FastNoiseLite emptyTileNoise;
    private List<List<Node2D>> positionToNode2D = new List<List<Node2D>>();

    public override void _Ready()
    {
        emptyTileNoise = EmptyTileNoiseTexture.Noise as FastNoiseLite;
        emptyTileNoise.Seed = (int) (new RandomNumberGenerator().Randi() % Mathf.Pow(2, 25));

        drillableGridProbability = new DrillableGridProbability(drillableType, toCurve, depthRanges);
    }

    public void Init(List<Node2D> buildings = null)
    {
        GenerateWorld(buildings);
        UpdateInternals();
        foreach (var child in GetChildren())
        {
            Tile tile = (Tile) child;
            Vector2I childCoord = LocalToMap(((Node2D) child).Position);
            Vector2I childGridCoord = TileMapPositionToGridPosition(childCoord);

            // Only register drillable tiles
            if (tile.tileType == TileType.Drillable || tile.tileType == TileType.NonDrillable)
            {
                positionToNode2D[childGridCoord.X][childGridCoord.Y] = (Node2D) child;
                if (tile.tileType == TileType.Drillable)
                {
                    ((Drillable) child).dug += _on_drillable_dug;
                    ((Drillable) child).preDug += _on_drillable_pre_dug;
                }
            } else if (tile.tileType == TileType.Background)
            {
                if (childGridCoord.Y == 0)
                {
                    DrillableShaderManager.UpdateDrillableSide((Node2D) child, DrillableSurface.Top, true);
                } else {
                    DrillableShaderManager.UpdateDrillableSide((Node2D) child, DrillableSurface.Top, false);
                }
                tile.ZIndex = -2;
            }
        }
        UpdateAllDrillableEdges();
    }

    public void GenerateWorld(List<Node2D> buildings = null)
    {
        List<List<int>> grid = new List<List<int>>();
        for (int i = 0; i < Width; i++)
        {
            positionToNode2D.Add(new List<Node2D>());
            for (int j = 0; j < StartingRows; j++)
            {
                SetCell(0, GridPositionToTileMapPosition(new Vector2I(i, j)), 1, new Vector2I(0, 0), DrillableConstants.DIRT_BACKGROUND_TILE_SET_ID);
                // clear any tiles left from editor
                EraseCell(1, GridPositionToTileMapPosition(new Vector2I(i, j)));

                if (i == 0 || i == Width - 1)
                {
                    SetCell(1, GridPositionToTileMapPosition(new Vector2I(i, j)), 1, new Vector2I(0, 0), DrillableConstants.DIRT_NON_DRILLABLE_TILE_SET_ID);
                    positionToNode2D[i].Add(null);
                    continue;
                }

                int tileSetId;
                var emptyTileNoiseValue = emptyTileNoise.GetNoise2D(i, j);
                if (emptyTileNoiseValue < -0.3) {
                    tileSetId = -1;
                } else {
                    tileSetId = DrillableConstants.MapDrillableTypeToTileSetId(drillableGridProbability.GetDrillableTypeForDepth((uint) j));
                }

                bool tileOverlapsBuilding = j == 0 && buildings != null && buildings.Any((building) => {
                    RectangleShape2D buildingShape = (RectangleShape2D) building.GetNode<CollisionShape2D>("CollisionShape2D").Shape;
                    Godot.Vector2 buildingPosition = building.Position;
                    float xPos = TileSet.TileSize.X * (i - Width / 2);
                    float overlap = Common.LineOverlap(buildingPosition.X - buildingShape.Size.X / 2, buildingPosition.X + buildingShape.Size.X / 2, 
                        xPos, xPos + TileSet.TileSize.X);
                    return overlap >= TileSet.TileSize.X / 3;
                });
                if (tileOverlapsBuilding)
                {
                    tileSetId = DrillableConstants.DIRT_NON_DRILLABLE_STONE_TOP_TILE_SET_ID;
                }

                if (tileSetId != -1) {
                    SetCell(1, GridPositionToTileMapPosition(new Vector2I(i, j)), 1, new Vector2I(0, 0), tileSetId);
                }

                positionToNode2D[i].Add(null);
            }
        }
    }

    public void UpdateAllDrillableEdges()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < StartingRows; j++)
            {
                Node2D node2D = positionToNode2D[i][j];
                if (node2D != null && GodotObject.IsInstanceValid(node2D))
                {
                    UpdateDrillableCorner(node2D, new Vector2I(i, j), DrillableCorner.TopLeft);
                    UpdateDrillableCorner(node2D, new Vector2I(i, j), DrillableCorner.TopRight);
                    UpdateDrillableCorner(node2D, new Vector2I(i, j), DrillableCorner.BottomLeft);
                    UpdateDrillableCorner(node2D, new Vector2I(i, j), DrillableCorner.BottomRight);
                }
            }
        }    
    }

    public void UpdateSurroundingDrillableEdges(Vector2I gridPosition, Node2D drillable)
    {
        for(int i = gridPosition.X - 1; i <= gridPosition.X + 1; i++)
        {
            for(int j = gridPosition.Y - 1; j <= gridPosition.Y + 1; j++)
            {
                if (i < 0 || i >= Width || j < 0 || j >= StartingRows)
                {
                    continue;
                }

                Node2D node2D = positionToNode2D[i][j];
                // Animation for drillable finishes and reveals the corners prior to animation transparancy mask covering them
                // as such, now that the animation is over set the corners to straigth to match the dug out area
                if (i == gridPosition.X && j == gridPosition.Y)
                {
                    DrillableShaderManager.UpdateDrillableCorner(drillable, DrillableCorner.TopLeft, DrillableCornerShape.Straight);
                    DrillableShaderManager.UpdateDrillableCorner(drillable, DrillableCorner.TopRight, DrillableCornerShape.Straight);
                    DrillableShaderManager.UpdateDrillableCorner(drillable, DrillableCorner.TopLeft, DrillableCornerShape.Straight);
                    DrillableShaderManager.UpdateDrillableCorner(drillable, DrillableCorner.TopRight, DrillableCornerShape.Straight);
                } else if (node2D != null && GodotObject.IsInstanceValid(node2D))
                {
                    UpdateDrillableCorner(node2D, new Vector2I(i, j), DrillableCorner.TopLeft);
                    UpdateDrillableCorner(node2D, new Vector2I(i, j), DrillableCorner.TopRight);
                    UpdateDrillableCorner(node2D, new Vector2I(i, j), DrillableCorner.BottomLeft);
                    UpdateDrillableCorner(node2D, new Vector2I(i, j), DrillableCorner.BottomRight);
                }
            }
        }   
    }

    private void UpdateDrillableCorner(Node2D drillable, Vector2I gridPos, DrillableCorner corner)
    {
        int yFlip = 1;
        if (corner == DrillableCorner.BottomLeft || corner == DrillableCorner.BottomRight)
        {
            yFlip = -1;
        }
        int xFlip = 1;
        if (corner == DrillableCorner.TopRight || corner == DrillableCorner.BottomRight)
        {
            xFlip = -1;
        }

        bool hasDrillableAbove = HasNeighbor(new Vector2I(0, -1 * yFlip), gridPos);
        bool hasDrillableDiagonal = HasNeighbor(new Vector2I(-1 * xFlip, -1 * yFlip), gridPos);
        bool hasDrillableSide = HasNeighbor(new Vector2I(-1 * xFlip, 0), gridPos);
        
        DrillableShaderManager.UpdateDrillableSide(drillable, (xFlip == 1) ? DrillableSurface.Left : DrillableSurface.Right, !hasDrillableSide);
        DrillableShaderManager.UpdateDrillableSide(drillable, (yFlip == 1) ? DrillableSurface.Top : DrillableSurface.Bottom, !hasDrillableAbove);

        DrillableCornerShape drillableCornerShape = DrillableCornerShape.None;
        int tileSetId = GetCellAlternativeTile(1, GridPositionToTileMapPosition(gridPos));
        if (hasDrillableAbove || (tileSetId == DrillableConstants.DIRT_NON_DRILLABLE_STONE_TOP_TILE_SET_ID && (corner == DrillableCorner.TopLeft || corner == DrillableCorner.TopRight)))
        {
            drillableCornerShape = DrillableCornerShape.Straight;
        } else {
            if (hasDrillableDiagonal) 
            {
                drillableCornerShape = DrillableCornerShape.Concave;
            } else
            {
                drillableCornerShape = hasDrillableSide ? DrillableCornerShape.Straight : DrillableCornerShape.Convex;
            }
        }

        DrillableShaderManager.UpdateDrillableCorner(drillable, corner, drillableCornerShape);
    }

    public void AddRows(int numRows)
    {

    }

    private Vector2I GridPositionToTileMapPosition(Vector2I gridPosition)
    {
        return new Vector2I(gridPosition.X - Width / 2, gridPosition.Y);
    }

    private Vector2I TileMapPositionToGridPosition(Vector2I tileMapPosition)
    {
        return new Vector2I(tileMapPosition.X + Width / 2, tileMapPosition.Y);
    }

    private bool HasNeighbor(Vector2I relativePosition, Vector2I gridPosition)
    {
        Vector2I neighborPosition = gridPosition + relativePosition;
        if (neighborPosition.X < 0 || neighborPosition.X >= Width)
        {
            return true;
        } else  if(neighborPosition.Y < 0 || neighborPosition.Y >= StartingRows)
        {
            return false;
        }

        return positionToNode2D[neighborPosition.X][neighborPosition.Y] != null && GodotObject.IsInstanceValid(positionToNode2D[neighborPosition.X][neighborPosition.Y]);
    }
    
    private bool IsValidGridPosition(Vector2I gridPos)
    {
        return gridPos.X >= 0 && gridPos.X < Width && gridPos.Y >= 0 && gridPos.Y < StartingRows;
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
                if (IsValidGridPosition(surroundingGridPos) && !(i == 0 && j == 0) && positionToNode2D[surroundingGridPos.X][surroundingGridPos.Y] != null
                    && GodotObject.IsInstanceValid(positionToNode2D[surroundingGridPos.X][surroundingGridPos.Y]))
                {
                    Tile tile = (Tile) positionToNode2D[surroundingGridPos.X][surroundingGridPos.Y];
                    if (tile.tileType == TileType.Drillable)
                    {
                        surroundingDrillables[i+1][j+1] = (Drillable) tile;
                        continue;
                    }
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

    public void _on_drillable_pre_dug(Node2D drillable, DrillFromDirection direction)
    {
        Vector2I gridPos = TileMapPositionToGridPosition(LocalToMap(drillable.Position));

        if (direction == DrillFromDirection.LEFT)
        {
            Node2D aboveNode = gridPos.Y - 1 >= 0 && gridPos.X - 1 >= 0 ? positionToNode2D[gridPos.X - 1][gridPos.Y - 1] : null;
            Node2D belowNode = gridPos.Y + 1 < StartingRows && gridPos.X - 1 >= 0 ? positionToNode2D[gridPos.X - 1][gridPos.Y + 1] : null;

            if (aboveNode != null && GodotObject.IsInstanceValid(aboveNode))
            {
                DrillableShaderManager.UpdateDrillableCorner(aboveNode, DrillableCorner.BottomRight, DrillableCornerShape.Straight);
            }
            if (belowNode != null && GodotObject.IsInstanceValid(belowNode))
            {
                DrillableShaderManager.UpdateDrillableCorner(belowNode, DrillableCorner.TopRight, DrillableCornerShape.Straight);
            }
        } else if (direction == DrillFromDirection.RIGHT)
        {
            Node2D aboveNode = gridPos.Y - 1 >= 0 && gridPos.X + 1 < StartingRows ? positionToNode2D[gridPos.X + 1][gridPos.Y - 1] : null;
            Node2D belowNode = gridPos.Y + 1 < StartingRows && gridPos.X + 1 < StartingRows ? positionToNode2D[gridPos.X + 1][gridPos.Y + 1] : null;

            if (aboveNode != null && GodotObject.IsInstanceValid(aboveNode))
            {
                DrillableShaderManager.UpdateDrillableCorner(aboveNode, DrillableCorner.BottomLeft, DrillableCornerShape.Straight);
            }
            if (belowNode != null && GodotObject.IsInstanceValid(belowNode))
            {
                DrillableShaderManager.UpdateDrillableCorner(belowNode, DrillableCorner.TopLeft, DrillableCornerShape.Straight);
            }
        }
    }

    public void _on_drillable_dug(Node2D drillable)
    {
        EmitSignal(SignalName.drillableDug, (Drillable) drillable);
        Vector2I tileMapPosition = LocalToMap(drillable.Position);
        Vector2I gridPosition = TileMapPositionToGridPosition(tileMapPosition);
        positionToNode2D[gridPosition.X][gridPosition.Y] = null;
        UpdateSurroundingDrillableEdges(gridPosition, drillable);
    }
}
