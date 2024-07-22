using System;
using Godot;
using Godot.NativeInterop;

public class GroundedPlayerStateProcessor : PlayerStateProcessor
{
    public enum GroundedPlayerStates 
    {
        IDLE,
        SOFT_LAND,
        HARD_LAND,
        NONE
    }

    public GroundedPlayerStates groundedPlayerState = GroundedPlayerStates.NONE;
    public GroundedPlayerStateProcessor(PlayerCharacterBody2D playerCharacterBody2D) : base(playerCharacterBody2D) {}

    public override void SetupState(StateTransition transition)
    {
        Vector2 velocity = player.Velocity;
        
        if (transition.FromState == PlayerState.Airborne)
        {
            Vector2 prevVelocity = (Vector2) transition.TransitionData;
            if (prevVelocity.Y > 250) 
            {
                playerAnimation.UpdateAnimation(PlayerAnimationState.LandHard);
                groundedPlayerState = GroundedPlayerStates.HARD_LAND;
                velocity.X /= 5;
                float healthLoss = (Mathf.Pow(1.01f, prevVelocity.Y - 200f)*4)+7;
                player.Health -= healthLoss;
            } else if (prevVelocity.Y > 150 && !Input.IsActionPressed("fly")) 
            {
                playerAnimation.UpdateAnimation(PlayerAnimationState.LandSoft);
                groundedPlayerState = GroundedPlayerStates.SOFT_LAND;
                velocity.X /= 2;
            } else 
            {
                playerAnimation.UpdateAnimation(PlayerAnimationState.Idle);
                groundedPlayerState = GroundedPlayerStates.IDLE;
            }
        } else 
        {
            playerAnimation.UpdateAnimation(PlayerAnimationState.Idle);
            groundedPlayerState = GroundedPlayerStates.IDLE;
        }

        player.Velocity = velocity;
    }

    public override StateTransition ProcessState(double delta)
    {
        if (player.Health <= 0)
        {
            return new StateTransition { ToState = PlayerState.Dead };
        }

        if (!player.IsOnFloor())
        {
            return new StateTransition { ToState = PlayerState.Airborne, TransitionData = null };
        }

        if (groundedPlayerState == GroundedPlayerStates.HARD_LAND)
        {
            // waiting for animation to finish before player can take action
            return new StateTransition { ToState = PlayerState.None };
        }

        player.Rotation = 0;

        ApplyVerticalFlight(delta);

        PlayerState transitionState = PlayerState.None;
        DrillFromDirection drillFromDirection = DrillFromDirection.NONE;

        if (groundedPlayerState == GroundedPlayerStates.IDLE)
        {
            Vector2 velocity = player.Velocity;
            Vector2 direction = Input.GetVector("move_left", "move_right", "fly", "down");
            if (direction != Vector2.Zero)
            {
                if (direction.Y > 0)
                {
                    velocity = new(0, 0);
                    drillFromDirection = DrillFromDirection.UP;
                    Node2D readyDrillable = playerDrillables.DirectionHeld(drillFromDirection, delta);
                    if (readyDrillable != null)
                    {
                        transitionState = PlayerState.Drilling;
                    } else 
                    {
                        playerAnimation.UpdateAnimation(PlayerAnimationState.Idle);
                    }
                }
                else if (direction.X < 0) 
                {   
                    velocity.X = direction.X * player.WalkSpeed;
                    drillFromDirection = DrillFromDirection.RIGHT;
                    Node2D readyDrillable = playerDrillables.DirectionHeld(drillFromDirection, delta);
                    if (readyDrillable != null)
                    {
                        transitionState = PlayerState.Drilling;
                    } else 
                    {
                        playerAnimation.UpdateAnimation(PlayerAnimationState.WalkLeft);
                    }
                } else if (direction.X > 0) 
                {
                    velocity.X = direction.X * player.WalkSpeed;
                    drillFromDirection = DrillFromDirection.LEFT;
                    Node2D readyDrillable = playerDrillables.DirectionHeld(DrillFromDirection.LEFT, delta);
                    if (readyDrillable != null)
                    {
                        transitionState = PlayerState.Drilling;
                    } else 
                    {
                        playerAnimation.UpdateAnimation(PlayerAnimationState.WalkRight);
                    }
                }
            } else
            {
                playerDrillables.DirectionHeld(DrillFromDirection.NONE, delta);
                velocity.X = 0;
                playerAnimation.UpdateAnimation(PlayerAnimationState.Idle);
            }

            player.Velocity = velocity;
        }
        return new StateTransition { ToState = transitionState, TransitionData = drillFromDirection };
    }

    public override void AnimationFinished(string animationName)
    {
        if (animationName == "land" || animationName == "land_soft")
        {
            // GD.Print("Grounded animation finished: " + animationName);
            groundedPlayerState = GroundedPlayerStates.IDLE;
        }
    }
}