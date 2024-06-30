using Godot;


public partial class DrillableGridProbability
{
    public Godot.Collections.Array<DrillableType> drillableTypes;
    public Godot.Collections.Array<Path2D> probabilityCurves;
    private RandomNumberGenerator rng = new RandomNumberGenerator();

    public DrillableGridProbability(Godot.Collections.Array<DrillableType> drillableTypes, Godot.Collections.Array<Path2D> probabilityCurves)
    {
        this.drillableTypes = drillableTypes;
        this.probabilityCurves = probabilityCurves;
    }    

    public DrillableType GetDrillableTypeForDepth(uint depth)
    {
        float tileRandomSelectionAmount = rng.Randf();
        float cumulativeProbability = 0;
        for (int i = 0; i < probabilityCurves.Count; i++)
        {
            Path2D path2D = probabilityCurves[i];
            float probability = GetProbabilityForDepth(path2D, depth)/1000.0f;
            cumulativeProbability += probability;
            if (cumulativeProbability >= tileRandomSelectionAmount)
            {
                return drillableTypes[i];
            }
        }

        return DrillableType.DIRT;
    }

    private float GetProbabilityForDepth(Path2D path2D, uint depth)
    {
        if (depth == 0)
        {
            return path2D.Curve.SampleBaked(0)[1];
        }

        float lineLength = 0.5f;
        float depthForLineLength = path2D.Curve.SampleBaked(lineLength)[0];
        while (depthForLineLength < depth)
        {
            lineLength *= 2;
            depthForLineLength = path2D.Curve.SampleBaked(lineLength)[0];
        }
        
        float leftLineLength = lineLength / 2;
        float rightLineLength = lineLength;
        float midPointLineLength = lineLength;

        while (Mathf.Abs(depthForLineLength - depth) > 0.1)
        {
            midPointLineLength = (leftLineLength + rightLineLength) / 2;
            depthForLineLength = path2D.Curve.SampleBaked(midPointLineLength)[0];

            if (path2D.Curve.SampleBaked(midPointLineLength)[0] < depth)
            {
                leftLineLength = midPointLineLength;
            }
            else
            {
                rightLineLength = midPointLineLength;
            }
        }

        return path2D.Curve.SampleBaked(midPointLineLength)[1];
    }
}
