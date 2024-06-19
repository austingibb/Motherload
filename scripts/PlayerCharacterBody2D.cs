using Godot;
using System;
using System.Collections.Generic;


public enum PlayerState {
	Grounded,
	HurtLanding,
	Landing,
	Flying,
	Falling,
	Drilling,
	None
}

public partial class PlayerCharacterBody2D : Godot.CharacterBody2D
{
	public const float WalkSpeed = 120.0f;
	public const float DrillSideSpeed = 80.0f;
	public const float DrillDownSpeed = 100.0f;
	private const float DrillingDuration = 0.3f;
	
	public const float VerticalFlightSpeed = -1000.0f;
	public const float HorizontalFlightSpeed = 300.0f;
	public const float MaxHorozontalSpeed = 300.0f;
	public const float TiltAmount = 0.174533f;
	public const float DragConstant = 0.011f;
	public float gravity = 600f;
	private PlayerState playerState = PlayerState.None;
	private PlayerDrillables playerDrillables = new();
	private ulong DrillingStartTime = 0;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public Vector2 prevVelocity = new(0, 0);
	public AnimatedSprite2D bodyAnimation;
	public PlayerAnimation playerAnimation;


    public override void _Ready()
    {
		Node2D flipper = GetNode<Node2D>("%flipper");
		AnimatedSprite2D bodyAnimation = GetNode<AnimatedSprite2D>("%body_AnimatedSprite2D");
		this.bodyAnimation = bodyAnimation;
		AnimatedSprite2D frontArmAnimation = GetNode<AnimatedSprite2D>("%front_arm_AnimatedSprite2D");
		AnimatedSprite2D backArmAnimation = GetNode<AnimatedSprite2D>("%back_arm_AnimatedSprite2D");
		AnimatedSprite2D armsAnimation = GetNode<AnimatedSprite2D>("%arms_AnimatedSprite2D");
		AnimatedSprite2D jetAnimation = GetNode<AnimatedSprite2D>("%jet_AnimatedSprite2D");
		AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("%player_AnimationPlayer");
		AnimationPlayer shaderAnimationPlayer = GetNode<AnimationPlayer>("%shader_AnimationPlayer");
		
		playerAnimation = new PlayerAnimation(flipper, bodyAnimation, frontArmAnimation, backArmAnimation, armsAnimation, jetAnimation, animationPlayer, shaderAnimationPlayer);
    }

    public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		
		// Add the gravity.
		if (!IsOnPlayerOnFloor())
		{
			velocity.Y += gravity * (float)delta;

			Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up", "ui_down");
			if (direction.X < 0)
			{
				velocity.X -= HorizontalFlightSpeed * (float)delta;
				this.Rotation = -TiltAmount;
			}
			else if (direction.X > 0)
			{
				velocity.X += HorizontalFlightSpeed * (float)delta;
				this.Rotation = TiltAmount;
			}
			else 
			{
				this.Rotation = 0;
			}

			if (Input.IsActionPressed("fly"))
			{
				playerState = PlayerState.Flying;
				playerAnimation.UpdateAnimation(PlayerAnimationState.Launch);
			}
			else if (velocity.Y > 0)
			{
				playerState = PlayerState.Falling;
				playerAnimation.UpdateAnimation(PlayerAnimationState.Fall);
			}
		} else 
		{
			this.Rotation = 0;

			if (playerState == PlayerState.Flying || playerState == PlayerState.Falling) 
			{
				if (prevVelocity.Y > 250) 
				{
					playerAnimation.UpdateAnimation(PlayerAnimationState.LandHard);
					playerState = PlayerState.HurtLanding;
					velocity.X /= 5;
				} else if (prevVelocity.Y > 150 && !Input.IsActionPressed("fly")) 
				{
					playerAnimation.UpdateAnimation(PlayerAnimationState.LandSoft);
					playerState = PlayerState.Landing;
					velocity.X /= 2;
				}
				else
				{
					playerState = PlayerState.Grounded;
				}
			} else if (playerState == PlayerState.Grounded || playerState == PlayerState.Drilling)
			{
				if (playerState == PlayerState.Grounded) 
				{
					// Get the input direction and handle the movement/deceleration.
					// As good practice, you should replace UI actions with custom gameplay actions.
					Vector2 direction = Input.GetVector("move_left", "move_right", "ui_up", "down");
					if (direction != Vector2.Zero)
					{
						if (direction.Y > 0)
						{
							velocity = new(0, 0);
							Node2D readyDrillable = playerDrillables.DirectionHeld(DrillFromDirection.UP, delta);
							if (readyDrillable != null)
							{
								playerState = PlayerState.Drilling;
								playerAnimation.UpdateAnimation(PlayerAnimationState.SetupDrillDown);
							} else 
							{
								playerAnimation.UpdateAnimation(PlayerAnimationState.Idle);
							}
						}
						else if (direction.X < 0) 
						{   
							velocity.X = direction.X * WalkSpeed;
							Node2D readyDrillable = playerDrillables.DirectionHeld(DrillFromDirection.RIGHT, delta);
							if (readyDrillable != null)
							{
								playerState = PlayerState.Drilling;
								playerAnimation.UpdateAnimation(PlayerAnimationState.SetupDrillLeft);
							} else 
							{
								playerAnimation.UpdateAnimation(PlayerAnimationState.WalkLeft);
							}
						} else if (direction.X > 0) 
						{
							velocity.X = direction.X * WalkSpeed;
							Node2D readyDrillable = playerDrillables.DirectionHeld(DrillFromDirection.LEFT, delta);
							if (readyDrillable != null)
							{
								playerState = PlayerState.Drilling;
								playerAnimation.UpdateAnimation(PlayerAnimationState.SetupDrillRight);
							} else 
							{
								playerAnimation.UpdateAnimation(PlayerAnimationState.WalkRight);
							}
						}
					} else
					{
						playerDrillables.DirectionHeld(DrillFromDirection.NONE, delta);
						velocity.X = Mathf.MoveToward(Velocity.X, 0, WalkSpeed);
						playerAnimation.UpdateAnimation(PlayerAnimationState.Idle);
					}

				} else if (playerState == PlayerState.Drilling) 
				{
					bool endDrilling = (Time.GetTicksMsec() - DrillingStartTime) / 1000.0f > DrillingDuration;
					if (playerAnimation.GetCurrentState() == PlayerAnimationState.DrillLeft ||
						playerAnimation.GetCurrentState() == PlayerAnimationState.DrillRight) 
					{
						if (playerAnimation.GetCurrentState() == PlayerAnimationState.DrillLeft)
						{
							if (endDrilling)
							{
								playerAnimation.UpdateAnimation(PlayerAnimationState.DrillStandupLeft);
							}
							velocity.X = -DrillSideSpeed;
						} else if (playerAnimation.GetCurrentState() == PlayerAnimationState.DrillRight)
						{
							velocity.X = DrillSideSpeed;
							if (endDrilling)
							{
								playerAnimation.UpdateAnimation(PlayerAnimationState.DrillStandupRight);
							}
						}
					}
					else if (playerAnimation.GetCurrentState() == PlayerAnimationState.DrillDown) 
					{
						velocity.Y = DrillDownSpeed;
						if (endDrilling) 
						{
							playerAnimation.UpdateAnimation(PlayerAnimationState.DrillStandup);
						}
					}
				}
			}
		}

		// Handle flight.
		if (Input.IsActionPressed("fly") && !(playerState == PlayerState.HurtLanding || playerState == PlayerState.Drilling))
		{
			playerAnimation.SetFlightState(true);
			velocity.Y += VerticalFlightSpeed * (float)delta;
		} else
		{
			playerAnimation.SetFlightState(false);
		}

		velocity = ApplyDrag(velocity);

		if (Mathf.Abs(velocity.X) > MaxHorozontalSpeed)
		{
			velocity.X = Mathf.Sign(velocity.X) * MaxHorozontalSpeed;
		}

		Velocity = velocity;
		MoveAndSlide();
		prevVelocity = velocity;
	}

	private bool IsOnPlayerOnFloor()
	{
		return (playerState == PlayerState.Drilling || IsOnFloor());
	}

	private Vector2 ApplyDrag(Vector2 velocity) 
	{
		Vector2 drag = new(Mathf.Pow(Mathf.Abs(velocity.X), 1.1f), Mathf.Pow(Mathf.Abs(velocity.Y), 1.1f));
		if (velocity.X > 0)
			drag.X *= -1;
		if (velocity.Y > 0)
			drag.Y *= -1;
		drag *= DragConstant;
		return new(velocity.X + drag.X, velocity.Y + drag.Y);
	}

	private void _on_player_animation_player_animation_finished(String anim_name) 
	{
		if (anim_name == "land") 
		{
			playerState = PlayerState.Grounded;
		} else if (anim_name == "land_soft") {
			playerState = PlayerState.Grounded;
		} else if (anim_name == "mine_forward_setup") {
			if (playerAnimation.GetCurrentState() == PlayerAnimationState.SetupDrillLeft)
			{
				playerAnimation.UpdateAnimation(PlayerAnimationState.DrillLeft);
				if (playerDrillables.ActiveDrillable != null)
				{
					(playerDrillables.ActiveDrillable as Drillable).StartDrillAnimation(DrillFromDirection.RIGHT);
				}
			} else if (playerAnimation.GetCurrentState() == PlayerAnimationState.SetupDrillRight)
			{
				playerAnimation.UpdateAnimation(PlayerAnimationState.DrillRight);
				if (playerDrillables.ActiveDrillable != null)
				{
					(playerDrillables.ActiveDrillable as Drillable).StartDrillAnimation(DrillFromDirection.LEFT);
				}
			}

			DrillingStartTime = Time.GetTicksMsec();
		} else if (anim_name == "mine_down_setup") {
			playerAnimation.UpdateAnimation(PlayerAnimationState.DrillDown);
			if (playerDrillables.ActiveDrillable != null)
			{
				(playerDrillables.ActiveDrillable as Drillable).StartDrillAnimation(DrillFromDirection.UP);
			}

			DrillingStartTime = Time.GetTicksMsec();
		} else if (anim_name == "drill_standup" || anim_name == "drill_down_standup")
		{
			playerState = PlayerState.Grounded;
		}
	}

	public void RegisterDrillable(Node2D drillable, DrillFromDirection direction) {
		playerDrillables.RegisterDrillable(drillable, direction);
	}

	public void UnregisterDrillable(Node2D drillable) {
		playerDrillables.UnregisterDrillable(drillable);
	}
}
