using System;
using System.Collections.Generic;
using Godot;


public partial class DrillableGridProbability
{
    public Godot.Collections.Array<DrillableType> drillableTypes;
    public Godot.Collections.Array<Curve> probabilityCurves;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    Godot.Collections.Array<Vector2I> depthRanges;

    public DrillableGridProbability(Godot.Collections.Array<DrillableType> drillableTypes, Godot.Collections.Array<Curve> probabilityCurves, Godot.Collections.Array<Vector2I> depthRanges)
    {
        this.drillableTypes = drillableTypes;
        this.probabilityCurves = probabilityCurves;
        this.depthRanges = depthRanges;
    }    

    public DrillableType GetDrillableTypeForDepth(uint depth)
    {
        float tileRandomSelectionAmount = rng.Randf();
        float cumulativeProbability = 0;
        for (int i = 0; i < probabilityCurves.Count; i++)
        {
            float probability = GetProbabilityForDepth(i, depth);
            cumulativeProbability += probability;
            if (cumulativeProbability >= tileRandomSelectionAmount)
            {
                return drillableTypes[i];
            }
        }

        return DrillableType.DIRT;
    }

    public float GetProbabilityForDepth(int curveIdx, uint depth)
    {
        if (depth == 20)
        {
            GD.Print("Depth: " + depth);
        }
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

        return curve.Sample(depthNormalized) / 100f;
    }
}
