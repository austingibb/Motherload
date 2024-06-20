using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class DrillableMapGenerator : TileMap
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

    public override void _Ready()
    {
        emptyTileNoise = EmptyTileNoiseTexture.Noise as FastNoiseLite;
        drillableTypeNoise = DrillableTypeNoiseTexture.Noise as FastNoiseLite;
        emptyTileNoise.Seed = (int) (new RandomNumberGenerator().Randi() % Mathf.Pow(2, 25));
        drillableTypeNoise.Seed = (int) (new RandomNumberGenerator().Randi() % Mathf.Pow(2, 25));
        GenerateWorld();
    }

    public void GenerateWorld()
    {
        List<float> heights = new List<float>();
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < StartingRows; j++)
            {
                var emptyTileNoiseValue = emptyTileNoise.GetNoise2D(i, j);
                var drillableTypeNoiseValue = drillableTypeNoise.GetNoise2D(i, j);
                int tileSetId = -1;
                if (emptyTileNoiseValue < -0.3) {
                    tileSetId = -1;
                } else if (drillableTypeNoiseValue < 0.3) {
                    tileSetId = DIRT_TILE_SET_ID;
                } else if (drillableTypeNoiseValue < 0.37) {
                    tileSetId = IRON_TILE_SET_ID;
                } else if (drillableTypeNoiseValue < 0.4) {
                    tileSetId = SILVER_TILE_SET_ID;
                } else if (drillableTypeNoiseValue < 0.42) {
                    tileSetId = GOLD_TILE_SET_ID;
                }
                if (tileSetId != -1) {
                    SetCell(0, new Vector2I(i-Width/2, j), 0, new Vector2I(0, 0), tileSetId);
                }
            }
        }
    }

    public void AddRows(int numRows)
    {

    }
}
