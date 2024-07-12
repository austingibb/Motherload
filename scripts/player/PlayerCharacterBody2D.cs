using Godot;
using System;
using System.Collections;
using System.Collections.Generic;


public enum PlayerState {
	Grounded,
	HurtLanding,
	Landing,
	Flying,
	Falling,
	Drilling,
	Dead,
	None
}

public partial class PlayerCharacterBody2D : Godot.CharacterBody2D
{
	public const float WalkSpeed = 120.0f;
	public const float BaseDrillSpeed = 70.0f;
	public float DrillSideSpeed = 70.0f;
	public float DrillDownSpeed = 70.0f;	
	public const float VerticalFlightSpeed = -900.0f;
	public const float CatchVerticalFlightSpeed = -1100.0f;
	public const float HorizontalFlightSpeed = 300.0f;
	public const float MaxHorozontalSpeed = 300.0f;
	public const float TiltAmount = 0.174533f;
	public const float DragConstant = 0.011f;
	public const float OriginalEnergyLossScale = 3.0f;
	public float EnergyLossScale = OriginalEnergyLossScale;
	public const float NoEnergyHealthLoss = 10.0f;
	private PlayerState playerState = PlayerState.None;
	private PlayerDrillables playerDrillables = new();
	public Vector2 prevVelocity = new(0, 0);
	public AnimatedSprite2D bodyAnimation;
	public AnimatedSprite2D frontHeadAnimation;
	public AnimatedSprite2D sideHeadAnimation;

	public RayCast2D leftRay;
	public RayCast2D rightRay;
	public RayCast2D downRay;

	public Node2D flipper;
	public Marker2D projectileSpawnPoint;

	public Battery battery;

	public PlayerAnimation playerAnimation;
	public PlayerShaderManager playerShaderManager;
	public List<List<Drillable>> SurroundingDrillables;
	private PackedScene laserScene;

	public float Energy;
	public float Health;

    public override void _Ready()
    {
		Node2D flipper = GetNode<Node2D>("%flipper");
		battery = GetNode<Node2D>("%body_AnimatedSprite2D/Battery/Battery1") as Battery;
		this.flipper = flipper;
		Marker2D projectileSpawnPoint = GetNode<Marker2D>("%player_projectile_spawn_point");
		this.projectileSpawnPoint = projectileSpawnPoint;
		laserScene = GD.Load<PackedScene>("res://scenes/laser.tscn");
		AnimatedSprite2D bodyAnimation = GetNode<AnimatedSprite2D>("%body_AnimatedSprite2D");
		AnimatedSprite2D frontHeadAnimation = GetNode<AnimatedSprite2D>("%front_head_AnimatedSprite2D");
		AnimatedSprite2D sideHeadAnimation = GetNode<AnimatedSprite2D>("%side_head_AnimatedSprite2D");

		leftRay = GetNode<RayCast2D>("raycasts/left_RayCast2D");
		rightRay = GetNode<RayCast2D>("raycasts/right_RayCast2D");
		downRay = GetNode<RayCast2D>("raycasts/down_RayCast2D");

		this.bodyAnimation = bodyAnimation;
		this.frontHeadAnimation = frontHeadAnimation;
		this.sideHeadAnimation = sideHeadAnimation;
		Node2D frontSprites = GetNode<Node2D>("%body_AnimatedSprite2D/front");
		Node2D sideSprites = GetNode<Node2D>("%body_AnimatedSprite2D/side");
		AnimatedSprite2D jetAnimation = GetNode<AnimatedSprite2D>("%jet_AnimatedSprite2D");
		AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("%player_AnimationPlayer");
		AnimationPlayer shaderAnimationPlayer = GetNode<AnimationPlayer>("%shader_AnimationPlayer");
		playerShaderManager = GetNode<Node2D>("%shaderManager") as PlayerShaderManager;	
		playerShaderManager.UpdateDrills(DrillType.Base);

		playerAnimation = new PlayerAnimation(flipper, bodyAnimation, jetAnimation, frontSprites, sideSprites,
			animationPlayer, shaderAnimationPlayer, battery);

		SurroundingDrillables = new List<List<Drillable>>();
		for (int i = 0; i < 3; i++)
		{
			SurroundingDrillables.Add(new List<Drillable>());
			for (int j = 0; j < 3; j++)
			{
				SurroundingDrillables[i].Add(null);
			}
		}

		Energy = 100.0f;
		Health = 100.0f;
    }

    public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		HandleEnergy((float)delta);
		HandleRaycastChecks();
		battery.SetCharge(Energy/100.0f);
		HandleHead((float)delta, frontHeadAnimation);
		HandleHead((float)delta, sideHeadAnimation);

		if (Input.IsActionJustPressed("fire"))
		{
			Shoot();
		}

		// Add the gravity.
		if (playerState == PlayerState.Dead)
		{
			if (!IsOnPlayerOnFloor()) 
			{
				velocity.Y += Game.GRAVITY * (float)delta;
			}
		}
		else if (!IsOnPlayerOnFloor())
		{
			velocity.Y += Game.GRAVITY * (float)delta;

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
			else if (velocity.Y >= 0)
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
					float healthLoss = (Mathf.Pow(1.01f, prevVelocity.Y - 200f)*4)+7;
					Health -= healthLoss;
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
					Vector2 direction = Input.GetVector("move_left", "move_right", "fly", "down");
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
						} else if (direction.Y < 0)
						{
							velocity.Y = -50;
							playerState = PlayerState.Flying;
						}
					} else
					{
						playerDrillables.DirectionHeld(DrillFromDirection.NONE, delta);
						velocity.X = Mathf.MoveToward(Velocity.X, 0, WalkSpeed);
						playerAnimation.UpdateAnimation(PlayerAnimationState.Idle);
					}
				} else if (playerState == PlayerState.Drilling) 
				{
					Drillable belowDrillable = SurroundingDrillables[1][2];
					if (playerAnimation.GetCurrentState() == PlayerAnimationState.DrillLeft ||
						playerAnimation.GetCurrentState() == PlayerAnimationState.DrillRight) 
					{
						if (playerAnimation.GetCurrentState() == PlayerAnimationState.DrillLeft)
						{
							playerDrillables.ActiveDrillable.UpdateDrillAnimationFromPosition(this);
							velocity.X = -DrillSideSpeed;
							
							if (playerDrillables.ActiveDrillable.IsDug)
							{
								Drillable leftDrillable = SurroundingDrillables[0][1];
								if (Health > 0 && Input.IsActionPressed("move_left") && Common.ValidTile(leftDrillable) && Common.ValidTile(belowDrillable))
								{
									playerDrillables.ActiveDrillable = leftDrillable;
									leftDrillable.StartDrillAnimation(DrillFromDirection.RIGHT);
								} else {
									playerAnimation.UpdateAnimation(PlayerAnimationState.DrillStandupLeft);
								}
							}
						} else if (playerAnimation.GetCurrentState() == PlayerAnimationState.DrillRight)
						{
							velocity.X = DrillSideSpeed;
							playerDrillables.ActiveDrillable.UpdateDrillAnimationFromPosition(this);
							if (playerDrillables.ActiveDrillable.IsDug)
							{
								Drillable rightDrillable = SurroundingDrillables[2][1];
								if (Health > 0 && Input.IsActionPressed("move_right") && Common.ValidTile(rightDrillable) && Common.ValidTile(belowDrillable))
								{
									playerDrillables.ActiveDrillable = rightDrillable;
									rightDrillable.StartDrillAnimation(DrillFromDirection.LEFT);
								} else {
									playerAnimation.UpdateAnimation(PlayerAnimationState.DrillStandupRight);
								}
							}
						}
					}
					else if (playerAnimation.GetCurrentState() == PlayerAnimationState.DrillDown) 
					{
						velocity.Y = DrillDownSpeed;
						playerDrillables.ActiveDrillable.UpdateDrillAnimationFromPosition(this);
						if (playerDrillables.ActiveDrillable.IsDug)
						{
							if (Health > 0 && Input.IsActionPressed("down") && belowDrillable != null && GodotObject.IsInstanceValid(belowDrillable))
							{
								playerDrillables.ActiveDrillable = belowDrillable;
								belowDrillable.StartDrillAnimation(DrillFromDirection.UP);
							} else 
							{
								playerAnimation.UpdateAnimation(PlayerAnimationState.DrillStandup);
							}
						}
					}
				}
			}
		}

		// Handle flight.
		if (Input.IsActionPressed("fly") && !(playerState == PlayerState.HurtLanding || playerState == PlayerState.Drilling || playerState == PlayerState.Dead))
		{
			playerAnimation.SetFlightState(true);
			if (velocity.Y < 0) 
			{
				velocity.Y += VerticalFlightSpeed * (float)delta;
			} else
			{
				velocity.Y += CatchVerticalFlightSpeed * (float)delta;
			}
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

	private void HandleRaycastChecks()
	{
		HandleRaycastCheck(leftRay, DrillFromDirection.RIGHT);
		HandleRaycastCheck(rightRay, DrillFromDirection.LEFT);
		HandleRaycastCheck(downRay, DrillFromDirection.UP);
	}

	private void HandleRaycastCheck(RayCast2D ray, DrillFromDirection direction)
	{
		if (ray.IsColliding())
		{
			Drillable drillable = (Drillable) ray.GetCollider();
			if (direction != DrillFromDirection.UP || Mathf.Abs(drillable.GlobalPosition.X - GlobalPosition.X) < 12.0f)
			{
				playerDrillables.RegisterDrillable(drillable, direction);
			} else {
				playerDrillables.UnregisterDrillable(direction);
			}
		} else if (!ray.IsColliding())
		{
			playerDrillables.UnregisterDrillable(direction);
		}
	}

	private void HandleHead(float delta, Node2D headAnimation) 
	{
		bool isPlayerFacingMouse = ((IsFacingLeft() && (GetGlobalMousePosition().X < this.GlobalPosition.X)) || (!IsFacingLeft() && (GetGlobalMousePosition().X > this.GlobalPosition.X)));
		if (!isPlayerFacingMouse && playerAnimation.IsFacingForward())
		{
			this.flipper.Scale = new Vector2(this.flipper.Scale.X * -1, this.flipper.Scale.Y);
		}

		bool allowRotation = isPlayerFacingMouse && (playerState != PlayerState.Drilling) && (playerState != PlayerState.Dead);

		if (allowRotation)
		{
			headAnimation.GlobalRotation = headAnimation.GlobalPosition.AngleToPoint(GetGlobalMousePosition()) + Mathf.DegToRad(180);
			if (headAnimation.Rotation > Mathf.DegToRad(180))
			{
				headAnimation.Rotation -= Mathf.DegToRad(360); 
			}

			if (headAnimation.Rotation < Mathf.DegToRad(-35))
			{
				headAnimation.Rotation = Mathf.DegToRad(-35);
			}
		} else {
			headAnimation.Rotation = 0;
		}
	}

	private void HandleEnergy(float delta)
	{
		if (Input.IsActionJustPressed("test6"))
		{
			if (EnergyLossScale == OriginalEnergyLossScale)
			{
				EnergyLossScale = 0.0f;
			} else {
				EnergyLossScale = OriginalEnergyLossScale;
			}
		}

		Energy -= delta / 4.0f * EnergyLossScale;
		if (playerState == PlayerState.Flying)
		{
			Energy -= delta * EnergyLossScale;
		} else if (playerState == PlayerState.Drilling)
		{
			Energy -= delta * EnergyLossScale;
		} else if (playerState == PlayerState.Grounded)
		{
			if (Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right"))
			{
				Energy -= delta / 2.0f * EnergyLossScale;
			}
		}

		if (Energy <= 0)
		{
			Energy = 0;
			Health -= delta * NoEnergyHealthLoss;
		}

		if (Health <= 0)
		{
			Health = 0;
			Die();
		}
	}

	public void Die()
	{
		if (playerState != PlayerState.Drilling && playerState != PlayerState.Dead)
		{
			this.Rotation = 0;
			playerState = PlayerState.Dead;
			playerAnimation.UpdateAnimation(PlayerAnimationState.PowerDown);
		}
	}

	public void Shoot()
	{
		Laser laser = laserScene.Instantiate() as Laser;
		laser.transformSource = projectileSpawnPoint;
		laser.flipDirection = true;
		laser.ZIndex = -1;
		Node2D projectiles = GetParent().GetNode<Node2D>("Projectiles");
		projectiles.AddChild(laser);
	}

	private bool IsOnPlayerOnFloor()
	{
		return (playerState == PlayerState.Drilling || IsOnFloor());
	}

	private Vector2 ApplyDrag(Vector2 velocity) 
	{
		Vector2 drag = new(Mathf.Pow(Mathf.Abs(velocity.X), 1.05f), Mathf.Pow(Mathf.Abs(velocity.Y), 1.05f));
		if (velocity.X > 0)
			drag.X *= -1;
		if (velocity.Y > 0)
			drag.Y *= -1;
		drag *= DragConstant;
		return new(velocity.X + drag.X, velocity.Y + drag.Y);
	}

	public void HandleDrillUpgrade(DrillUpgrade drillUpgrade)
    {
		DrillSideSpeed = BaseDrillSpeed * drillUpgrade.drillSpeedMultiplier;
		DrillDownSpeed = BaseDrillSpeed * drillUpgrade.drillSpeedMultiplier;
		playerShaderManager.UpdateDrills(drillUpgrade.drillType);
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
					playerDrillables.ActiveDrillable.StartDrillAnimation(DrillFromDirection.RIGHT);
				}
			} else if (playerAnimation.GetCurrentState() == PlayerAnimationState.SetupDrillRight)
			{
				playerAnimation.UpdateAnimation(PlayerAnimationState.DrillRight);
				if (playerDrillables.ActiveDrillable != null)
				{
					playerDrillables.ActiveDrillable.StartDrillAnimation(DrillFromDirection.LEFT);
				}
			}
		} else if (anim_name == "mine_down_setup") {
			playerAnimation.UpdateAnimation(PlayerAnimationState.DrillDown);
			if (playerDrillables.ActiveDrillable != null)
			{
				playerDrillables.ActiveDrillable.StartDrillAnimation(DrillFromDirection.UP);
			}

		} else if (anim_name == "drill_standup" || anim_name == "drill_down_standup")
		{
			playerState = PlayerState.Grounded;
		}
	}

	private bool IsFacingLeft() 
	{
		return this.flipper.Scale.X > 0;
	}
}
