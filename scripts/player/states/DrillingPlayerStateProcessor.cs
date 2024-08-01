using System;
using System.Collections.Generic;
using Godot;
using Godot.NativeInterop;

public class DrillingPlayerStateProcessor : PlayerStateProcessor
{
    public enum DrillingPlayerStates 
    {
        DRILL_SETUP,
        DRILLING,
        DRILL_END,
        NONE
    }

    public DrillFromDirection drillFromDirection = DrillFromDirection.NONE;
    public DrillingPlayerStates drillingPlayerState = DrillingPlayerStates.NONE;
    public bool FinishedDrilling = false;
    public DrillingPlayerStateProcessor(PlayerCharacterBody2D playerCharacterBody2D) : base(playerCharacterBody2D) {}

    public override void SetupState(StateTransition transition)
    {
        drillFromDirection = (DrillFromDirection) transition.TransitionData;
        FinishedDrilling = false;
        drillingPlayerState = DrillingPlayerStates.DRILL_SETUP;
        Vector2 velocity = player.Velocity;
        velocity /= 5;
        player.Velocity = velocity;

        if ((playerAnimation._currentState == PlayerAnimationState.WalkLeftBackwards && drillFromDirection == DrillFromDirection.RIGHT) ||
            (playerAnimation._currentState == PlayerAnimationState.WalkRightBackwards && drillFromDirection == DrillFromDirection.LEFT))
        {
            playerAnimation.UpdateAnimation(PlayerAnimationState.IdleTurn);
        } else if (drillFromDirection == DrillFromDirection.RIGHT)
        {
            playerAnimation.UpdateAnimation(PlayerAnimationState.SetupDrillLeft);
        } else if (drillFromDirection == DrillFromDirection.LEFT)
        {
            playerAnimation.UpdateAnimation(PlayerAnimationState.SetupDrillRight);
        } else if (drillFromDirection == DrillFromDirection.UP)
        {
            playerAnimation.UpdateAnimation(PlayerAnimationState.SetupDrillDown);
        } else if (drillFromDirection == DrillFromDirection.DOWN)
        {
            playerAnimation.UpdateAnimation(PlayerAnimationState.SetupDrillUp);
        }
    }

    public override StateTransition ProcessState(double delta)
    {
        player.Rotation = 0;

        if (drillFromDirection == DrillFromDirection.DOWN)
        {
            playerAnimation.SetFlightState(true);
        }

        if (FinishedDrilling) 
        {
            return new StateTransition { ToState = PlayerState.Airborne, TransitionData = drillFromDirection};
        }

        // wait for drill setup animation to finish
        if (drillingPlayerState == DrillingPlayerStates.DRILL_SETUP || drillingPlayerState == DrillingPlayerStates.DRILL_END)
        {
            return new StateTransition { ToState = PlayerState.None, TransitionData = null };
        }

        Vector2 velocity = player.Velocity;

        List<List<Drillable>> surroundingDrillables = player.SurroundingDrillables;

        Drillable belowDrillable = surroundingDrillables[1][2];
        if (drillFromDirection == DrillFromDirection.RIGHT || drillFromDirection == DrillFromDirection.LEFT) 
        {
            if (drillFromDirection == DrillFromDirection.RIGHT)
            {
                playerDrillables.ActiveDrillable.UpdateDrillAnimationFromPosition(player);
                velocity.X = -player.DrillSideSpeed;
                
                if (playerDrillables.ActiveDrillable.IsDug)
                {
                    Drillable leftDrillable = surroundingDrillables[0][1];
                    if (Input.IsActionPressed("move_left") && Common.ValidTile(leftDrillable) && Common.ValidTile(belowDrillable))
                    {
                        playerDrillables.ActiveDrillable = leftDrillable;
                        leftDrillable.StartDrillAnimation(DrillFromDirection.RIGHT);
                    } else {
                        playerAnimation.UpdateAnimation(PlayerAnimationState.DrillStandupLeft);
                        drillingPlayerState = DrillingPlayerStates.DRILL_END;
                    }
                }
            } else if (drillFromDirection == DrillFromDirection.LEFT)
            {
                velocity.X = player.DrillSideSpeed;
                playerDrillables.ActiveDrillable.UpdateDrillAnimationFromPosition(player);
                if (playerDrillables.ActiveDrillable.IsDug)
                {
                    Drillable rightDrillable = surroundingDrillables[2][1];
                    if (Input.IsActionPressed("move_right") && Common.ValidTile(rightDrillable) && Common.ValidTile(belowDrillable))
                    {
                        playerDrillables.ActiveDrillable = rightDrillable;
                        rightDrillable.StartDrillAnimation(DrillFromDirection.LEFT);
                    } else {
                        playerAnimation.UpdateAnimation(PlayerAnimationState.DrillStandupRight);
                        drillingPlayerState = DrillingPlayerStates.DRILL_END;
                    }
                }
            }
        }
        else if (drillFromDirection == DrillFromDirection.UP) 
        {
            velocity.Y = player.DrillVerticalSpeed;
            playerDrillables.ActiveDrillable.UpdateDrillAnimationFromPosition(player);
            if (playerDrillables.ActiveDrillable.IsDug)
            {
                if (Input.IsActionPressed("down") && Common.ValidTile(belowDrillable))
                {
                    playerDrillables.ActiveDrillable = belowDrillable;
                    belowDrillable.StartDrillAnimation(DrillFromDirection.UP);
                } else 
                {
                    playerAnimation.UpdateAnimation(PlayerAnimationState.DrillStandupDown);
                    drillingPlayerState = DrillingPlayerStates.DRILL_END;
                }
            }
        }
        else if (drillFromDirection == DrillFromDirection.DOWN)
        {
            Drillable aboveDrillable = surroundingDrillables[1][0];
            velocity.Y = -player.DrillVerticalSpeed;
            playerDrillables.ActiveDrillable.UpdateDrillAnimationFromPosition(player);
            if (playerDrillables.ActiveDrillable.IsDug)
            {
                if (Input.IsActionPressed("fly") && aboveDrillable != null && GodotObject.IsInstanceValid(aboveDrillable))
                {
                    playerDrillables.ActiveDrillable = aboveDrillable;
                    aboveDrillable.StartDrillAnimation(DrillFromDirection.DOWN);
                } else 
                {
                    return new StateTransition { ToState = PlayerState.Airborne, TransitionData = drillFromDirection };
                }
            }
        }

        player.Velocity = velocity;
        return new StateTransition { ToState = PlayerState.None, TransitionData = null };
    }

    public override void AnimationFinished(string animationName)
    {
        if (animationName == "mine_forward_setup" || animationName == "mine_down_setup" || animationName == "mine_up_setup")
        {
            drillingPlayerState = DrillingPlayerStates.DRILLING;
            if (playerDrillables.ActiveDrillable != null)
            {
                playerDrillables.ActiveDrillable.StartDrillAnimation(drillFromDirection);
            }

            if (animationName == "mine_forward_setup")
            {
                if (drillFromDirection == DrillFromDirection.RIGHT)
                {
                    playerAnimation.UpdateAnimation(PlayerAnimationState.DrillLeft);
                } else if (playerAnimation.GetCurrentState() == PlayerAnimationState.SetupDrillRight)
                {
                    playerAnimation.UpdateAnimation(PlayerAnimationState.DrillRight);
                }
            } else if (animationName == "mine_down_setup") {
                playerAnimation.UpdateAnimation(PlayerAnimationState.DrillDown);
            } else if (animationName == "mine_up_setup")
            {
                playerAnimation.UpdateAnimation(PlayerAnimationState.DrillUp);
            } 
        }

        if (animationName == "idle_turn")
        {
            if (drillFromDirection == DrillFromDirection.RIGHT)
            {
                playerAnimation.UpdateAnimation(PlayerAnimationState.SetupDrillLeft);
            } else if (drillFromDirection == DrillFromDirection.LEFT)
            {
                playerAnimation.UpdateAnimation(PlayerAnimationState.SetupDrillRight);
            }
        }

        if (animationName == "drill_standup" || animationName == "drill_down_standup")
		{
			FinishedDrilling = true;
		}
    }
}