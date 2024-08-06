using Godot;
using System;
using System.Collections.Generic;

public enum PlayerAnimationState
{
    Idle,
    IdleTurn,
    IdleSide,
    SetupDrillLeft,
    SetupDrillRight,
    SetupDrillDown,
    SetupDrillUp,
    DrillLeft,
    DrillRight,
    DrillDown,
    DrillUp,
    DrillStandupLeft,
    DrillStandupRight,
    DrillStandupDown,
    DrillStandupUp,
    WalkLeft,
    WalkLeftBackwards,
    WalkRight,
    WalkRightBackwards,
    Fall,
    FallStatic,
    Launch,
    LandHard,
    LandSoft,
    FlyLeft,
    FlyRight,
    DeploySolarPanel,
    RetractSolarPanel,
    PowerDown
}

public enum PlayerHeadState
{
    LookSide,
    LookDown
}

public partial class PlayerAnimation : Node2D
{
    Node2D flipper;
    public AnimatedSprite2D bodyAnimation;
    public Node2D frontSprites;
    public Node2D sideSprites;
    public AnimatedSprite2D jetAnimation;
    public AnimationPlayer animationPlayer;
    public AnimationPlayer headAnimationPlayer;
    public AnimationPlayer shaderAnimationPlayer;
    public PlayerAnimationState _currentState;
    public PlayerHeadState _currentHeadState;
    public PlayerAnimationState _previousState;
    public Battery battery;
    public bool isInit = false;

    public HashSet<PlayerAnimationState> forwardFacingStates = new HashSet<PlayerAnimationState>
    {
        PlayerAnimationState.Idle,
        PlayerAnimationState.IdleTurn,
        PlayerAnimationState.Fall,
        PlayerAnimationState.FallStatic,
        PlayerAnimationState.Launch,
        PlayerAnimationState.LandHard,
        PlayerAnimationState.LandSoft,
        PlayerAnimationState.FlyLeft,
        PlayerAnimationState.FlyRight,
        PlayerAnimationState.SetupDrillDown,
        PlayerAnimationState.DrillDown,
        PlayerAnimationState.DrillStandupDown,
        PlayerAnimationState.PowerDown,
        PlayerAnimationState.SetupDrillUp,
        PlayerAnimationState.DrillUp,
        PlayerAnimationState.DrillStandupUp,
        PlayerAnimationState.DeploySolarPanel,
        PlayerAnimationState.RetractSolarPanel
    };

    public HashSet<PlayerAnimationState> sideFacingStates = new HashSet<PlayerAnimationState>
    {
        PlayerAnimationState.WalkLeft,
        PlayerAnimationState.WalkRight,
        PlayerAnimationState.WalkLeftBackwards,
        PlayerAnimationState.WalkRightBackwards,
        PlayerAnimationState.SetupDrillLeft,
        PlayerAnimationState.SetupDrillRight,
        PlayerAnimationState.DrillLeft,
        PlayerAnimationState.DrillRight,
        PlayerAnimationState.DrillStandupLeft,
        PlayerAnimationState.DrillStandupRight,
        PlayerAnimationState.IdleSide
    };

    public void Init(Node2D flipper, AnimatedSprite2D bodyAnimation, AnimatedSprite2D jetAnimation, Node2D frontSprites, Node2D sideSprites,
        AnimationPlayer animationPlayer, AnimationPlayer headAnimationPlayer, AnimationPlayer shaderAnimationPlayer, Battery battery)
    {
        this.flipper = flipper;
        this.frontSprites = frontSprites;
        this.sideSprites = sideSprites;
        this.bodyAnimation = bodyAnimation;
        this.jetAnimation = jetAnimation;
        this.animationPlayer = animationPlayer;
        this.headAnimationPlayer = headAnimationPlayer;
        this.shaderAnimationPlayer = shaderAnimationPlayer;
        this.battery = battery;
        this._currentState = PlayerAnimationState.Idle;
        this._previousState = PlayerAnimationState.Idle;
        this.isInit = true;
    }

    public PlayerAnimationState GetPreviousState()
    {
        return _previousState;
    }

    public PlayerAnimationState GetCurrentState()
    {
        return _currentState;
    }

    public void PlayHurt()
    {
        shaderAnimationPlayer.Play("hurt");
    }

    public void UpdateBattery(Battery battery)
    {
        this.battery = battery;
    }

    public void SetFlightState(bool isFlying)
    {
        if (isFlying)
        {
            jetAnimation.Visible = true;
        }
        else
        {
            jetAnimation.Visible = false;
        }
    }

    public void SetHeadAnimation(PlayerHeadState playerHeadState)
    {
        if (_currentHeadState == playerHeadState)
            return;

        headAnimationPlayer.SpeedScale = 2.0f;

        switch (playerHeadState)
        {
            case PlayerHeadState.LookSide:
                headAnimationPlayer.Play("down_to_side");
                break;
            case PlayerHeadState.LookDown:
                headAnimationPlayer.Play("side_to_down");
                break;
            
        }
        _currentHeadState = playerHeadState;
    }

    public void UpdateAnimation(PlayerAnimationState state)
    {
        if (!isInit)
        {
            return;
        }

        if (_currentState == state)
            return;
        
        // GD.Print("Switching to state: ", state);

        if (state != PlayerAnimationState.DrillLeft && state != PlayerAnimationState.DrillRight && state != PlayerAnimationState.SetupDrillLeft && state != PlayerAnimationState.SetupDrillRight)
        {
            bodyAnimation.Rotation = 0;
            bodyAnimation.Position = new Vector2(0, 0);
        }
        
        _previousState = _currentState;
        _currentState = state;
        animationPlayer.SpeedScale = 4.0f;

        if (forwardFacingStates.Contains(state))
        {
            frontSprites.Visible = true;
            sideSprites.Visible = false;
            battery.SetOrientation(false);
        }
        else if (sideFacingStates.Contains(state))
        {
            sideSprites.Visible = true;
            frontSprites.Visible = false;
            battery.SetOrientation(true);
        }

        switch (state)
        {
            case PlayerAnimationState.Idle:
                animationPlayer.Play("idle");
                break;
            case PlayerAnimationState.IdleSide:
                animationPlayer.Play("idle_side");
                break;
            case PlayerAnimationState.IdleTurn:
                animationPlayer.Play("idle_turn");
                break;
            case PlayerAnimationState.WalkLeft:
                animationPlayer.Play("walk_forward");
                flipper.Scale = new Vector2(1, 1);
                break;
            case PlayerAnimationState.WalkRight:
                animationPlayer.Play("walk_forward");
                flipper.Scale = new Vector2(-1, 1);
                break;
            case PlayerAnimationState.WalkLeftBackwards:
                animationPlayer.PlayBackwards("walk_forward");
                flipper.Scale = new Vector2(-1, 1);
                break;
            case PlayerAnimationState.WalkRightBackwards:
                animationPlayer.PlayBackwards("walk_forward");
                flipper.Scale = new Vector2(1, 1);
                break;
            case PlayerAnimationState.Fall:
                animationPlayer.Play("fall");
                break;
            case PlayerAnimationState.FallStatic:
                animationPlayer.Play("fall_static");
                break;
            case PlayerAnimationState.Launch:
                animationPlayer.Play("launch");
                break;   
            case PlayerAnimationState.LandHard:
                animationPlayer.SpeedScale = 3.0f;
                animationPlayer.Play("land");
                shaderAnimationPlayer.Play("hurt");
                break;
            case PlayerAnimationState.LandSoft:
                animationPlayer.SpeedScale = 3.0f;
                animationPlayer.Play("land_soft");
                break;
            case PlayerAnimationState.FlyLeft:
                animationPlayer.Play("fly_left");
                break;
            case PlayerAnimationState.FlyRight:
                animationPlayer.Play("fly_right");
                break;
            case PlayerAnimationState.SetupDrillLeft:
                flipper.Scale = new Vector2(1, 1);
                animationPlayer.SpeedScale = 5.0f;
                animationPlayer.Play("mine_forward_setup");
                break;
            case PlayerAnimationState.SetupDrillRight:
                flipper.Scale = new Vector2(-1, 1);
                animationPlayer.SpeedScale = 5.0f;
                animationPlayer.Play("mine_forward_setup");
                break;
            case PlayerAnimationState.SetupDrillDown:
                animationPlayer.SpeedScale = 3.0f;
                animationPlayer.Play("mine_down_setup");
                break;
            case PlayerAnimationState.DrillDown:
                animationPlayer.SpeedScale = 4.0f;
                animationPlayer.Play("mine_down_drill");
                break;
            case PlayerAnimationState.SetupDrillUp:
                animationPlayer.SpeedScale = 5.0f;
                animationPlayer.Play("mine_up_setup");
                break;
            case PlayerAnimationState.DrillUp:
                animationPlayer.SpeedScale = 5.0f;
                animationPlayer.Play("mine_up_drill");
                break;
            case PlayerAnimationState.DrillLeft:
                flipper.Scale = new Vector2(1, 1);
                animationPlayer.Play("mine_forward_walk");
                break;
            case PlayerAnimationState.DrillRight:
                flipper.Scale = new Vector2(-1, 1);
                animationPlayer.Play("mine_forward_walk");
                break;
            case PlayerAnimationState.DrillStandupLeft:
                flipper.Scale = new Vector2(1, 1);
                animationPlayer.Play("drill_standup");
                break;
            case PlayerAnimationState.DrillStandupRight:
                flipper.Scale = new Vector2(-1, 1);
                animationPlayer.Play("drill_standup");
                break;
            case PlayerAnimationState.DrillStandupDown:
                animationPlayer.SpeedScale = 3.0f;
                animationPlayer.Play("drill_down_standup");
                break;
            case PlayerAnimationState.DrillStandupUp:
                animationPlayer.SpeedScale = 4.0f;
                animationPlayer.Play("mine_up_standup");
                break;
            case PlayerAnimationState.DeploySolarPanel:
                animationPlayer.Play("deploy_solar_panel");
                break;
            case PlayerAnimationState.RetractSolarPanel:
                animationPlayer.Play("deploy_solar_panel");
                break;
            case PlayerAnimationState.PowerDown:
                animationPlayer.SpeedScale = 1.0f;
                animationPlayer.Play("power_down");
                break;
            default:
                break;
        }
    }

    public bool IsFacingForward()
    {
        return forwardFacingStates.Contains(_currentState);
    }

    public void FlipX()
    {
        flipper.Scale = new Vector2(-flipper.Scale.X, flipper.Scale.Y);
    }
}
