using System;
using Godot;
using Godot.NativeInterop;

public class AirbornePlayerStateProcessor : PlayerStateProcessor
{
    public enum AirbornePlayerState 
    {
        FLYING,
        FALLING,
        FALLING_STATIC,
        AWAITING_TRANSITION,
        NONE
    }

    public AirbornePlayerState airbornePlayerState = AirbornePlayerState.NONE;
    public Vector2 launchPoint;

    public AirbornePlayerStateProcessor(PlayerCharacterBody2D playerCharacterBody2D) : base(playerCharacterBody2D) {}

    public override void SetupState(StateTransition transition)
    {
        airbornePlayerState = AirbornePlayerState.FALLING;

        if (transition.FromState == PlayerState.Drilling)
        {
            DrillFromDirection direction = (DrillFromDirection) transition.TransitionData;

            if (direction == DrillFromDirection.DOWN)
            {
                player.playerAnimation.UpdateAnimation(PlayerAnimationState.DrillStandupUp);
                airbornePlayerState = AirbornePlayerState.AWAITING_TRANSITION;
            } else if (direction == DrillFromDirection.UP)
            {
                airbornePlayerState = AirbornePlayerState.FALLING_STATIC;
            }
        }

        launchPoint = player.GlobalPosition;
    }

    public override StateTransition ProcessState(double delta)
    {
        if (player.Health <= 0)
        {
            return new StateTransition { ToState = PlayerState.Dead };
        }

        ApplyGravity(delta);
        ApplyVerticalFlight(delta);
        ApplyHorizontalFlight(delta);
        ApplyDrag();

        if (player.IsOnFloor())
        {
            return new StateTransition { ToState = PlayerState.Grounded, TransitionData = player.prevVelocity };
        }

        if (airbornePlayerState != AirbornePlayerState.AWAITING_TRANSITION)
        {
            DrillFromDirection directionHeld = DrillFromDirection.NONE;

            if (Input.IsActionPressed("fly"))
            {
                playerAnimation.UpdateAnimation(PlayerAnimationState.Launch);
                if (Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right"))
                {
                    directionHeld = DrillFromDirection.NONE;
                }
                else
                {
                    directionHeld = DrillFromDirection.DOWN;
                }   
                airbornePlayerState = AirbornePlayerState.FLYING;
            }
            else if (player.Velocity.Y > 0)
            {
                if (airbornePlayerState != AirbornePlayerState.FALLING_STATIC)
                {
                    playerAnimation.UpdateAnimation(PlayerAnimationState.Fall);
                    airbornePlayerState = AirbornePlayerState.FALLING;
                }
            }

            float verticalUnitDistance = player.getUnitDistanceBetweenPoints(launchPoint, player.GlobalPosition).Y;
            float digUpResistance;
            if (verticalUnitDistance < 3)
            {
                digUpResistance = 0;
            }
            else
            {
                digUpResistance = (verticalUnitDistance - 3) / PlayerConstants.UnitHeightDigUpResistance;
            }

            Node2D readyDrillable = playerDrillables.DirectionHeld(directionHeld, delta, digUpResistance);
            if (readyDrillable != null)
            {
                return new StateTransition { ToState = PlayerState.Drilling, TransitionData = directionHeld };
            }
        }

        return new StateTransition { ToState = PlayerState.None };
    }

    public override void AnimationFinished(string animationName)
    {
        if (animationName == "mine_up_standup")
        {
            airbornePlayerState = AirbornePlayerState.FALLING_STATIC;
            playerAnimation.UpdateAnimation(PlayerAnimationState.FallStatic);
        }
    }
}