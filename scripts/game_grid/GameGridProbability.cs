using System;
using System.Collections.Generic;
using Godot;


public partial class GameGridProbability<[MustBeVariant] T>
{
    public Godot.Collections.Array<T> types;
    public T defaultType;
    public Godot.Collections.Array<Curve> probabilityCurves;
    private RandomNumberGenerator rng;
    Godot.Collections.Array<Vector2I> depthRanges;
    public float probabilityConstant;

    public GameGridProbability(Godot.Collections.Array<T> types, T defaultType, Godot.Collections.Array<Curve> probabilityCurves, Godot.Collections.Array<Vector2I> depthRanges, 
        float probabilityConstant = 0.01f, int seed = -1)
    {
        this.probabilityConstant = probabilityConstant;
        this.types = types;
        this.probabilityCurves = probabilityCurves;
        this.depthRanges = depthRanges;
        this.defaultType = defaultType;

        rng = new RandomNumberGenerator();
        if (seed == -1)
        {
            rng.Randomize();
        }
        else
        {
            rng.Seed = (ulong) seed;
        }
    }    

    public T GetTypeForDepth(uint depth)
    {
        float randomSelectionAmount = rng.Randf();
        float cumulativeProbability = 0;
        for (int i = 0; i < probabilityCurves.Count; i++)
        {
            float probability = GetProbabilityForDepth(i, depth);
            cumulativeProbability += probability;
            if (cumulativeProbability >= randomSelectionAmount)
            {
                return types[i];
            }
        }

        return defaultType;
    }

    public float GetProbabilityForDepth(int curveIdx, uint depth)
    {
        int lowerDepthRange = depthRanges[curveIdx].X;
        int upperDepthRange = depthRanges[curveIdx].Y;
        int depthWidth = upperDepthRange - lowerDepthRange;
        Curve curve = probabilityCurves[curveIdx];

        if (depth < lowerDepthRange)
        {
            return curve.Sample(0) / 100f;
        }
        if (depth > upperDepthRange)
        {
            return curve.Sample(1) / 100f;
        }

        float depthNormalized = ((float) depth - lowerDepthRange) / (depthWidth);

        return curve.Sample(depthNormalized) * this.probabilityConstant;
    }
}
