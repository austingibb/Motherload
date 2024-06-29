using System;
using Godot;

public partial class DrillableAnimation
{
    public DrillFromDirection drillFromDirection;
    private Drillable drillable;
    private ShaderMaterial shaderMaterial;
    private AnimationPlayer animationPlayer;
    private const float HorozontalAnimationOffset = -10.0f;
    private const float VerticalAnimationOffset = 1.0f;
    private const float ProgressScale = 1.0f;
    private const float FinishThreshold = 0.9f;

    public DrillableAnimation(Drillable drillable)
    {
        this.drillable = drillable;
        AnimationPlayer animationPlayer = drillable.GetNode<AnimationPlayer>("./AnimationPlayer");
		this.animationPlayer = animationPlayer;
        AnimatedSprite2D animatedSprite2D = drillable.GetNode<AnimatedSprite2D>("Dirt_AnimatedSprite2D");
        shaderMaterial = animatedSprite2D.Material as ShaderMaterial;
    }

    public void StartAnimation(DrillFromDirection drillFromDirection) 
    {
        this.drillFromDirection = drillFromDirection;
        DrillableShaderManager.SetupAnimation(drillable, drillFromDirection);
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
        } else {
            progress = 0.0f;
        }

        if (progress < 0.0f)
            progress = 0.0f;

        DrillableShaderManager.SetAnimationProgress(drillable, progress);
        
        return progress >= FinishThreshold;
    }
}
