using System;
using Godot;
using Godot.NativeInterop;

public class DeadPlayerStateProcessor : PlayerStateProcessor
{
    public DeadPlayerStateProcessor(PlayerCharacterBody2D playerCharacterBody2D) : base(playerCharacterBody2D) {}

    public override void SetupState(PlayerStateTransition transition)
    {
        player.Rotation = 0;
        playerAnimation.UpdateAnimation(PlayerAnimationState.PowerDown);
    }

    public override PlayerStateTransition ProcessState(double delta)
    {
        ApplyGravity(delta);
        ApplyDrag();

        return new PlayerStateTransition { ToState = PlayerState.None };
    }

    public override void AnimationFinished(string animationName) {}
}