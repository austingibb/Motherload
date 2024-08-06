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
    public bool dashApplied = false;
    public float horozontalSpeedReduction = 1;
    public ulong launchStartTime = 0;

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
        launchStartTime = Time.GetTicksUsec();
    }

    public override StateTransition ProcessState(double delta)
    {
        if (player.Health <= 0)
        {
            return new StateTransition { ToState = PlayerState.Dead };
        }
        
        HandleDash();
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

    public void HandleDash()
    {
        if (Input.IsActionPressed("dash") && player.playerDash.CanDash() && !dashApplied)
        {
            player.playerDash.Dash();
            dashApplied = true;
            float launchTimeProgress = Mathf.Clamp((Time.GetTicksUsec() - launchStartTime) / (0.5f * Mathf.Pow(10, 6)), 0, 1);
            horozontalSpeedReduction =  0.5f + 0.5f * launchTimeProgress;
            GD.Print("Launch time progress: " + launchTimeProgress + " horozontal speed reduction: " + horozontalSpeedReduction);

            Vector2 direction = Input.GetVector("move_left", "move_right", "fly", "ui_down");
            if (direction.X < 0)
            {
                if (player.Velocity.X >= -player.HorizontalFlightSpeed)
                {
                    player.Velocity = new Vector2(-player.MaxHorozontalSpeed, player.Velocity.Y);
                }
            } else if (direction.X > 0)
            {
                if (player.Velocity.X <= player.HorizontalFlightSpeed)
                {
                    player.Velocity = new Vector2(player.MaxHorozontalSpeed, player.Velocity.Y);
                }  
            } else if (direction.Y < 0)
            {
                float velocityY = player.Velocity.Y * 1.1f - 1.5f*player.DashFlightSpeed;
                velocityY = Mathf.Clamp(velocityY, float.NegativeInfinity, -1.5f * player.DashFlightSpeed);
                player.Velocity = new Vector2(player.Velocity.X, velocityY);
            }
        }

        if (dashApplied && !player.playerDash.IsDashActive())
        {
            player.Velocity = new Vector2(player.Velocity.X * horozontalSpeedReduction, player.Velocity.Y);
            dashApplied = false;
        }
    }


    public virtual void ApplyHorizontalFlight(double delta) 
    {
        Vector2 velocity = player.Velocity;
        Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up", "ui_down");
        if (direction.X < 0)
        {
            velocity.X -= player.HorizontalFlightSpeed * (float)delta;
            player.Rotation = -PlayerConstants.TiltAmount;
        }
        else if (direction.X > 0)
        {
            velocity.X += player.HorizontalFlightSpeed * (float)delta;
            player.Rotation = PlayerConstants.TiltAmount;
        }
        else 
        {
            player.Rotation = 0;
        }

        float effectiveMaxSpeed = player.MaxHorozontalSpeed;
        if (player.playerDash.IsDashActive())
        {
            effectiveMaxSpeed = player.MaxHorozontalSpeed * 1.2f;
        }
        if (Mathf.Abs(velocity.X) > effectiveMaxSpeed)
		{
			velocity.X = Mathf.Sign(velocity.X) * effectiveMaxSpeed;
		}
        player.Velocity = velocity;
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