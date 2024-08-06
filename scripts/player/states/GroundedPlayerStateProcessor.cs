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
        DASH,
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
                float healthLoss = (Mathf.Pow(1.01f, prevVelocity.Y - 200f)*4)+7;
                player.Health -= healthLoss;
            } else if (prevVelocity.Y > 150 && !Input.IsActionPressed("fly")) 
            {
                playerAnimation.UpdateAnimation(PlayerAnimationState.LandSoft);
                groundedPlayerState = GroundedPlayerStates.SOFT_LAND;
            } else 
            {
                playerAnimation.UpdateAnimation(PlayerAnimationState.Idle);
                groundedPlayerState = GroundedPlayerStates.IDLE;
            }

            if (player.playerDash.IsDashActive())
            {
                groundedPlayerState = GroundedPlayerStates.DASH;
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

        if (groundedPlayerState == GroundedPlayerStates.DASH)
        {
            if (!player.playerDash.IsDashActive()) 
            {
                groundedPlayerState = GroundedPlayerStates.IDLE;
            }
        }

        player.Rotation = 0;

        ApplyVerticalFlight(delta);

        PlayerState transitionState = PlayerState.None;
        DrillFromDirection drillFromDirection = DrillFromDirection.NONE;

        if (groundedPlayerState == GroundedPlayerStates.IDLE)
        {
            Vector2 velocity = player.Velocity;
            Vector2 direction = Input.GetVector("move_left", "move_right", "fly", "down");
            bool IsMouseLeftOfPlayer = player.GlobalPosition.X > player.GetGlobalMousePosition().X;
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
                    if (velocity.X < -player.WalkSpeed)
                    {
                        velocity.X = Mathf.MoveToward(velocity.X, direction.X * player.WalkSpeed, (float) delta * 500.0f);
                    } else if (velocity.X <= 0)
                    {
                        velocity.X = Mathf.MoveToward(velocity.X, direction.X * player.WalkSpeed, (float) delta * 1000.0f);
                    } else {
                        velocity.X = 0;
                    }
                    drillFromDirection = DrillFromDirection.RIGHT;
                    Node2D readyDrillable = playerDrillables.DirectionHeld(drillFromDirection, delta);
                    if (readyDrillable != null)
                    {
                        transitionState = PlayerState.Drilling;
                    } else 
                    {
                        if (Input.IsActionPressed("dash") && player.playerDash.CanDash())
                        {
                            StartDash(direction, ref velocity);
                        } else 
                        {
                            if (IsMouseLeftOfPlayer)
                            {
                                playerAnimation.UpdateAnimation(PlayerAnimationState.WalkLeft);
                            } else
                            {
                                playerAnimation.UpdateAnimation(PlayerAnimationState.WalkLeftBackwards);
                            }
                        }
                    }
                } else if (direction.X > 0) 
                {
                    if (velocity.X >= player.WalkSpeed)
                    {
                        velocity.X = Mathf.MoveToward(velocity.X, direction.X * player.WalkSpeed, (float) delta * 500.0f);
                    } else if (velocity.X >= 0)
                    {
                        velocity.X = Mathf.MoveToward(velocity.X, direction.X * player.WalkSpeed, (float) delta * 1000.0f);
                    } else {
                        velocity.X = 0;
                    }
                    drillFromDirection = DrillFromDirection.LEFT;
                    Node2D readyDrillable = playerDrillables.DirectionHeld(DrillFromDirection.LEFT, delta);
                    if (readyDrillable != null)
                    {
                        transitionState = PlayerState.Drilling;
                    } else 
                    {
                        if (Input.IsActionPressed("dash") && player.playerDash.CanDash())
                        {
                            StartDash(direction, ref velocity);
                        } else {
                            if (!IsMouseLeftOfPlayer)
                            {
                                playerAnimation.UpdateAnimation(PlayerAnimationState.WalkRight);
                            } else
                            {
                                playerAnimation.UpdateAnimation(PlayerAnimationState.WalkRightBackwards);
                            }
                        }                    
                    }
                }
            } else
            {
                playerDrillables.DirectionHeld(DrillFromDirection.NONE, delta);
                if (Mathf.Abs(velocity.X) > player.WalkSpeed)
                {
                    velocity.X = Mathf.MoveToward(velocity.X, 0, (float) delta * 500.0f);
                } else
                {
                    velocity.X = Mathf.MoveToward(velocity.X, 0, (float) delta * 1000.0f);
                }
                playerAnimation.UpdateAnimation(PlayerAnimationState.Idle);
            }

            player.Velocity = velocity;
        }
        return new StateTransition { ToState = transitionState, TransitionData = drillFromDirection };
    }

    public void StartDash(Vector2 direction, ref Vector2 velocity)
    {
        playerAnimation.UpdateAnimation(PlayerAnimationState.IdleSide);
        groundedPlayerState = GroundedPlayerStates.DASH;
        player.playerDash.Dash();
        velocity.X = direction.X * player.DashSpeed;
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