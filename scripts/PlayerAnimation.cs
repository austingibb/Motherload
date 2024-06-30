using Godot;
using System;
using System.Collections.Generic;

public enum PlayerAnimationState
{
    Idle,
    SetupDrillLeft,
    SetupDrillRight,
    SetupDrillDown,
    DrillLeft,
    DrillRight,
    DrillDown,
    DrillStandupLeft,
    DrillStandupRight,
    DrillStandup,
    WalkLeft,
    WalkRight,
    Fall,
    Launch,
    LandHard,
    LandSoft,
    FlyLeft,
    FlyRight
}

public partial class PlayerAnimation : GodotObject
{
    Node2D flipper;
    public AnimatedSprite2D bodyAnimation;
    public AnimatedSprite2D frontHeadAnimation;
    public AnimatedSprite2D sideHeadAnimation;
    public AnimatedSprite2D frontArmAnimation;
    public AnimatedSprite2D backArmAnimation;
    public AnimatedSprite2D armsAnimation;
    public AnimatedSprite2D jetAnimation;
    public AnimationPlayer animationPlayer;
    public AnimationPlayer shaderAnimationPlayer;
    public PlayerAnimationState _currentState;

    public HashSet<PlayerAnimationState> forwardFacingStates = new HashSet<PlayerAnimationState>
    {
        PlayerAnimationState.Idle,
        PlayerAnimationState.Fall,
        PlayerAnimationState.Launch,
        PlayerAnimationState.LandHard,
        PlayerAnimationState.LandSoft,
        PlayerAnimationState.FlyLeft,
        PlayerAnimationState.FlyRight,
        PlayerAnimationState.SetupDrillDown,
        PlayerAnimationState.DrillDown,
        PlayerAnimationState.DrillStandup
    };

    public HashSet<PlayerAnimationState> sideFacingStates = new HashSet<PlayerAnimationState>
    {
        PlayerAnimationState.WalkLeft,
        PlayerAnimationState.WalkRight,
        PlayerAnimationState.SetupDrillLeft,
        PlayerAnimationState.SetupDrillRight,
        PlayerAnimationState.DrillLeft,
        PlayerAnimationState.DrillRight,
        PlayerAnimationState.DrillStandupLeft,
        PlayerAnimationState.DrillStandupRight
    };

    public PlayerAnimation(Node2D flipper, AnimatedSprite2D bodyAnimation, AnimatedSprite2D frontHeadAnimation, AnimatedSprite2D sideHeadAnimation, 
        AnimatedSprite2D frontArmAnimation, AnimatedSprite2D backArmAnimation, AnimatedSprite2D armsAnimation, AnimatedSprite2D jetAnimation, 
        AnimationPlayer animationPlayer, AnimationPlayer shaderAnimationPlayer)
    {
        this.flipper = flipper;
        this.bodyAnimation = bodyAnimation;
        this.frontHeadAnimation = frontHeadAnimation;
        this.sideHeadAnimation = sideHeadAnimation;
        this.frontArmAnimation = frontArmAnimation;
        this.backArmAnimation = backArmAnimation;
        this.armsAnimation = armsAnimation;
        this.jetAnimation = jetAnimation;
        this.animationPlayer = animationPlayer;
        this.shaderAnimationPlayer = shaderAnimationPlayer;
        this._currentState = PlayerAnimationState.Idle;
    }

    public PlayerAnimationState GetCurrentState()
    {
        return _currentState;
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

    public void UpdateAnimation(PlayerAnimationState state)
    {
        if (_currentState == state)
            return;
        
        if (state != PlayerAnimationState.DrillLeft && state != PlayerAnimationState.DrillRight && state != PlayerAnimationState.SetupDrillLeft && state != PlayerAnimationState.SetupDrillRight)
        {
            bodyAnimation.Rotation = 0;
            bodyAnimation.Position = new Vector2(0, 0);
        }
        
        _currentState = state;
        animationPlayer.SpeedScale = 4.0f;

        GD.Print("Switch to state:", state);

        if (forwardFacingStates.Contains(state))
        {
            frontHeadAnimation.Visible = true;
            armsAnimation.Visible = true;
            sideHeadAnimation.Visible = false;
            frontArmAnimation.Visible = false;
            backArmAnimation.Visible = false;
        }
        else if (sideFacingStates.Contains(state))
        {
            frontHeadAnimation.Visible = false;
            armsAnimation.Visible = false;
            sideHeadAnimation.Visible = true;
            frontArmAnimation.Visible = true;
            backArmAnimation.Visible = true;
        }

        switch (state)
        {
            case PlayerAnimationState.Idle:
                animationPlayer.Play("idle");
                break;
            case PlayerAnimationState.WalkLeft:
                animationPlayer.Play("walk_forward");
                flipper.Scale = new Vector2(1, 1);
                break;
            case PlayerAnimationState.WalkRight:
                animationPlayer.Play("walk_forward");
                flipper.Scale = new Vector2(-1, 1);
                break;
            case PlayerAnimationState.Fall:
                animationPlayer.Play("fall");
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
                animationPlayer.SpeedScale = 4.0f;
                animationPlayer.Play("mine_down_setup");
                break;
            case PlayerAnimationState.DrillDown:
                animationPlayer.SpeedScale = 4.0f;
                animationPlayer.Play("mine_down_drill");
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
            case PlayerAnimationState.DrillStandup:
                animationPlayer.Play("drill_down_standup");
                break;
            default:
                break;
        }
    }

    public bool IsFacingForward()
    {
        return forwardFacingStates.Contains(_currentState);
    }
}
