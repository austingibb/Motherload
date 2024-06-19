using Godot;
using System;

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
    public AnimatedSprite2D frontArmAnimation;
    public AnimatedSprite2D backArmAnimation;
    public AnimatedSprite2D armsAnimation;
    public AnimatedSprite2D jetAnimation;
    public AnimationPlayer animationPlayer;
    public AnimationPlayer shaderAnimationPlayer;
    public PlayerAnimationState _currentState;

    public PlayerAnimation(Node2D flipper, AnimatedSprite2D bodyAnimation, AnimatedSprite2D frontArmAnimation, AnimatedSprite2D backArmAnimation, 
        AnimatedSprite2D armsAnimation, AnimatedSprite2D jetAnimation, AnimationPlayer animationPlayer, AnimationPlayer shaderAnimationPlayer)
    {
        this.flipper = flipper;
        this.bodyAnimation = bodyAnimation;
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
            animationPlayer.Stop();
            bodyAnimation.Rotation = 0;
            bodyAnimation.Position = new Vector2(0, 0);
        }
        
        _currentState = state;
        animationPlayer.SpeedScale = 4.0f;

        GD.Print("Switch to state:", state);

        switch (state)
        {
            case PlayerAnimationState.Idle:
                bodyAnimation.Play("idle");
                frontArmAnimation.Visible = false;
                backArmAnimation.Visible = false;
                armsAnimation.Visible = false;
                jetAnimation.Visible = false;
                break;
            case PlayerAnimationState.WalkLeft:
                bodyAnimation.Stop();
                bodyAnimation.Play("walk");
                flipper.Scale = new Vector2(1, 1);
                frontArmAnimation.Visible = true;
                frontArmAnimation.Stop();
                frontArmAnimation.Play("walk");
                backArmAnimation.Visible = true;
                backArmAnimation.Stop();
                backArmAnimation.Play("walk");
                armsAnimation.Visible = false;
                jetAnimation.Visible = false;
                break;
            case PlayerAnimationState.WalkRight:
                bodyAnimation.Stop();
                bodyAnimation.Play("walk");
                flipper.Scale = new Vector2(-1, 1);
                frontArmAnimation.Visible = true;
                frontArmAnimation.Stop();
                frontArmAnimation.Play("walk");
                backArmAnimation.Visible = true;
                backArmAnimation.Stop();
                backArmAnimation.Play("walk");
                armsAnimation.Visible = false;
                jetAnimation.Visible = false;
                break;
            case PlayerAnimationState.Fall:
                bodyAnimation.Play("fall");
                frontArmAnimation.Visible = false;
                backArmAnimation.Visible = false;
                armsAnimation.Visible = false;
                jetAnimation.Visible = false;
                break;
            case PlayerAnimationState.Launch:
                frontArmAnimation.Visible = false;
                backArmAnimation.Visible = false;
                armsAnimation.Visible = false;
                animationPlayer.Play("launch");
                break;   
            case PlayerAnimationState.LandHard:
                frontArmAnimation.Visible = false;
                backArmAnimation.Visible = false;
                armsAnimation.Visible = false;
                animationPlayer.SpeedScale = 3.0f;
                animationPlayer.Play("land");
                shaderAnimationPlayer.Play("hurt");
                break;
            case PlayerAnimationState.LandSoft:
                frontArmAnimation.Visible = false;
                backArmAnimation.Visible = false;
                armsAnimation.Visible = false;
                animationPlayer.SpeedScale = 3.0f;
                animationPlayer.Play("land_soft");
                break;
            case PlayerAnimationState.FlyLeft:
                bodyAnimation.Stop();
                animationPlayer.Play("fly_left");
                frontArmAnimation.Visible = false;
                backArmAnimation.Visible = false;
                armsAnimation.Visible = false;
                break;
            case PlayerAnimationState.FlyRight:
                bodyAnimation.Stop();
                animationPlayer.Play("fly_right");
                frontArmAnimation.Visible = false;
                backArmAnimation.Visible = false;
                armsAnimation.Visible = false;
                _currentState = PlayerAnimationState.FlyRight;
                break;
            case PlayerAnimationState.SetupDrillLeft:
                frontArmAnimation.Visible = true;
                backArmAnimation.Visible = true;
                armsAnimation.Visible = false;
                flipper.Scale = new Vector2(1, 1);
                animationPlayer.SpeedScale = 5.0f;
                animationPlayer.Play("mine_forward_setup");
                break;
            case PlayerAnimationState.SetupDrillRight:
                frontArmAnimation.Visible = true;
                backArmAnimation.Visible = true;
                armsAnimation.Visible = false;
                flipper.Scale = new Vector2(-1, 1);
                animationPlayer.SpeedScale = 5.0f;
                animationPlayer.Play("mine_forward_setup");
                break;
            case PlayerAnimationState.SetupDrillDown:
                frontArmAnimation.Visible = false;
                backArmAnimation.Visible = false;
                armsAnimation.Visible = false;
                flipper.Scale = new Vector2(1, 1);
                animationPlayer.SpeedScale = 4.0f;
                animationPlayer.Play("mine_down_setup");
                break;
            case PlayerAnimationState.DrillDown:
                frontArmAnimation.Visible = false;
                backArmAnimation.Visible = false;
                armsAnimation.Visible = false;
                flipper.Scale = new Vector2(1, 1);
                animationPlayer.Play("mine_down_drill");
                break;
            case PlayerAnimationState.DrillLeft:
                frontArmAnimation.Visible = true;
                backArmAnimation.Visible = true;
                armsAnimation.Visible = false;
                flipper.Scale = new Vector2(1, 1);
                animationPlayer.Play("mine_forward_walk");
                break;
            case PlayerAnimationState.DrillRight:
                frontArmAnimation.Visible = true;
                backArmAnimation.Visible = true;
                armsAnimation.Visible = false;
                flipper.Scale = new Vector2(-1, 1);
                animationPlayer.Play("mine_forward_walk");
                break;
            case PlayerAnimationState.DrillStandupLeft:
                frontArmAnimation.Visible = true;
                backArmAnimation.Visible = true;
                armsAnimation.Visible = false;
                flipper.Scale = new Vector2(1, 1);
                animationPlayer.Play("drill_standup");
                break;
            case PlayerAnimationState.DrillStandupRight:
                frontArmAnimation.Visible = true;
                backArmAnimation.Visible = true;
                armsAnimation.Visible = false;
                flipper.Scale = new Vector2(-1, 1);
                animationPlayer.Play("drill_standup");
                break;
            case PlayerAnimationState.DrillStandup:
                frontArmAnimation.Visible = false;
                backArmAnimation.Visible = false;
                armsAnimation.Visible = false;
                flipper.Scale = new Vector2(1, 1);
                animationPlayer.Play("drill_down_standup");
                break;
            default:
                break;
        }
    }
}
