using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class DiggableMapGenerator : TileMap
{
    [Export]
    public int Width = 20;
    [Export]
    public int StartingRows = 60;
    [Export]
    public NoiseTexture2D NoiseHeight;

    private Noise noise;

    public override void _Ready()
    {
        noise = NoiseHeight.Noise;
    }

    public void GenerateWorld()
    {
        List<float> heights = new List<float>();
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < StartingRows; j++)
            {
                var noiseValue = noise.GetNoise2D(i, j);
                heights.Add(noiseValue);
            }
        }

        GD.Print("Min noise val:", heights.Min());
        GD.Print("Max noise val:", heights.Max());
    }

    public void AddRows(int numRows)
    {

    }
}
