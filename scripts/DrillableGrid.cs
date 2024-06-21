using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public partial class DrillableGrid : TileMap
{
    [Export]
    public int Width = 20;
    [Export]
    public int StartingRows = 60;
    [Export]
    public NoiseTexture2D EmptyTileNoiseTexture;
    [Export]
    public NoiseTexture2D DrillableTypeNoiseTexture;

    public const int DIRT_TILE_SET_ID = 1;
    public const int GOLD_TILE_SET_ID = 2;
    public const int IRON_TILE_SET_ID = 3;
    public const int SILVER_TILE_SET_ID = 4;

    private FastNoiseLite emptyTileNoise;
    private FastNoiseLite drillableTypeNoise;

    private List<List<Node2D>> positionToNode2D = new List<List<Node2D>>();

    private float updateTime = 0.0f;

    public override void _Ready()
    {
        emptyTileNoise = EmptyTileNoiseTexture.Noise as FastNoiseLite;
        drillableTypeNoise = DrillableTypeNoiseTexture.Noise as FastNoiseLite;
        emptyTileNoise.Seed = (int) (new RandomNumberGenerator().Randi() % Mathf.Pow(2, 25));
        drillableTypeNoise.Seed = (int) (new RandomNumberGenerator().Randi() % Mathf.Pow(2, 25));
        GenerateWorld();
        UpdateInternals();
        foreach (var child in GetChildren())
        {
            Vector2I childCoord = LocalToMap(((Node2D) child).Position);
            Vector2I childGridCoord = TileMapPositionToGridPosition(childCoord);
            positionToNode2D[childGridCoord.X][childGridCoord.Y] = (Node2D) child;
            ((Drillable) child).dug += _on_drillable_dug;
        }
        UpdateAllDrillableEdges();
    }

    public void _on_drillable_dug(Node2D drillable)
    {
        GD.Print("Drillable dug");
        Vector2I gridPosition = TileMapPositionToGridPosition(LocalToMap(drillable.Position));
        positionToNode2D[gridPosition.X][gridPosition.Y] = null;
        UpdateSurroundingDrillableEdges(gridPosition);
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
                    UpdateDrillableSurface(node2D, new Vector2I(i, j), true, true);
                    UpdateDrillableSurface(node2D, new Vector2I(i, j), true, false);
                    UpdateDrillableSurface(node2D, new Vector2I(i, j), false, true);
                    UpdateDrillableSurface(node2D, new Vector2I(i, j), false, false);
                }
            }
        }    
    }

    public void UpdateSurroundingDrillableEdges(Vector2I gridPosition)
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
                if (node2D != null && GodotObject.IsInstanceValid(node2D))
                {
                    UpdateDrillableSurface(node2D, new Vector2I(i, j), true, true);
                    UpdateDrillableSurface(node2D, new Vector2I(i, j), true, false);
                    UpdateDrillableSurface(node2D, new Vector2I(i, j), false, true);
                    UpdateDrillableSurface(node2D, new Vector2I(i, j), false, false);
                }
            }
        }   
    }

    private void UpdateDrillableSurface(Node2D drillable, Vector2I gridPos, bool isAbove, bool isLeft)
    {
        int yFlip = isAbove ? 1 : -1;
        int xFlip = isLeft ? 1 : -1;
        bool hasDrillableAbove = HasNeighbor(new Vector2I(0, -1 * yFlip), gridPos);
        bool hasDrillableDiagonal = HasNeighbor(new Vector2I(-1 * xFlip, -1 * yFlip), gridPos);
        bool hasDrillableSide = HasNeighbor(new Vector2I(-1 * xFlip, 0), gridPos);

        DrillableSurface surface = isAbove ? DrillableSurface.Top : DrillableSurface.Bottom;
        DrillableSide side = isLeft ? DrillableSide.Left : DrillableSide.Right;
        

        if (hasDrillableAbove)
        {
            DrillableShaderManager.UpdateDrillableCorner(drillable, surface, false);
        } else 
        {
            DrillableCornerShape drillableCornerShape = DrillableCornerShape.None;
            if (hasDrillableDiagonal) 
            {
                drillableCornerShape = DrillableCornerShape.Concave;
            
            } else
            {
                drillableCornerShape = hasDrillableSide ? DrillableCornerShape.Straight : DrillableCornerShape.Convex;
            }

            DrillableShaderManager.UpdateDrillableCorner(drillable, surface, true, side, drillableCornerShape);
        }

    }

    public void GenerateWorld()
    {
        List<List<int>> grid = new List<List<int>>();
        for (int i = 0; i < Width; i++)
        {
            positionToNode2D.Add(new List<Node2D>());
            for (int j = 0; j < StartingRows; j++)
            {
                var emptyTileNoiseValue = emptyTileNoise.GetNoise2D(i, j);
                var drillableTypeNoiseValue = drillableTypeNoise.GetNoise2D(i, j);
                int tileSetId = -1;
                if (emptyTileNoiseValue < -0.3) {
                    tileSetId = -1;
                } else if (drillableTypeNoiseValue < 0.42) {
                    tileSetId = DIRT_TILE_SET_ID;
                } 
                // else if (drillableTypeNoiseValue < 0.37) {
                //     tileSetId = IRON_TILE_SET_ID;
                // } else if (drillableTypeNoiseValue < 0.4) {
                //     tileSetId = SILVER_TILE_SET_ID;
                // } else if (drillableTypeNoiseValue < 0.42) {
                //     tileSetId = GOLD_TILE_SET_ID;
                // }
                if (tileSetId != -1) {
                    SetCell(0, GridPositionToTileMapPosition(new Vector2I(i, j)), 1, new Vector2I(0, 0), tileSetId);
                }
                positionToNode2D[i].Add(null);
            }
        }
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
}
