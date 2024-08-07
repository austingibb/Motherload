using System;
using Godot;

public partial class DrillableAnimation
{
    public DrillFromDirection drillFromDirection;
    private Drillable drillable;
    private ShaderMaterial shaderMaterial;
    private const float HorozontalAnimationOffset = -10.0f;
    private const float VerticalAnimationOffset = 3.0f;
    private const float ProgressScale = 1.0f;
    private const float FinishThreshold = 0.94f;

    public DrillableAnimation(Drillable drillable)
    {
        this.drillable = drillable;
        AnimatedSprite2D animatedSprite2D = drillable.GetNode<AnimatedSprite2D>("Dirt_AnimatedSprite2D");
        shaderMaterial = animatedSprite2D.Material as ShaderMaterial;
    }

    public void StartAnimation(DrillFromDirection drillFromDirection) 
    {
        this.drillFromDirection = drillFromDirection;
        SolidTileShaderManager.SetupAnimation(drillable, drillFromDirection);
    }

    public bool UpdateAnimationFromPosition(Node2D drillingEntity)
    {
        float progress;
        if (drillFromDirection == DrillFromDirection.LEFT)
        {
            progress = 1.0f - (Mathf.Abs((drillingEntity.GlobalPosition.X + HorozontalAnimationOffset) - drillable.GlobalPosition.X) / 33 * ProgressScale);
        } else if (drillFromDirection == DrillFromDirection.RIGHT)
        {
            progress = 1.0f - (Mathf.Abs(((drillingEntity.GlobalPosition.X - HorozontalAnimationOffset) - drillable.GlobalPosition.X)) / 33 * ProgressScale);
        } else if (drillFromDirection == DrillFromDirection.UP)
        {
            float yDiff = Mathf.Abs(((drillingEntity.GlobalPosition.Y - VerticalAnimationOffset) - drillable.GlobalPosition.Y));
            float yDiffWithinDrillable = yDiff/33;
            progress = 1.0f - (yDiffWithinDrillable * ProgressScale);
        } else if (drillFromDirection == DrillFromDirection.DOWN)
        {
            float yDiff = Mathf.Abs(((drillingEntity.GlobalPosition.Y + VerticalAnimationOffset) - drillable.GlobalPosition.Y));
            float yDiffWithinDrillable = yDiff/33;
            progress = 1.0f - (yDiffWithinDrillable * ProgressScale);
        } else {
            progress = 0.0f;
        }

        if (progress < 0.0f)
            progress = 0.0f;
        
        SolidTileShaderManager.SetAnimationProgress(drillable, progress);
        
        return progress >= FinishThreshold;
    }
}
