using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public delegate Vector2 GetUnitDistanceBetweenPoints(Vector2 point1, Vector2 point2);

public partial class PlayerCharacterBody2D : Godot.CharacterBody2D
{
	public float WalkSpeed = 120.0f;
	public float DrillSideSpeed = 80.0f;
	public float DrillVerticalSpeed = 80.0f;	
	public float VerticalFlightSpeed = -900.0f;
	public  float CatchVerticalFlightSpeed = -1100.0f;
	public float HorizontalFlightSpeed = 300.0f;
	public float MaxHorozontalSpeed = 300.0f;

	
	public float EnergyLossScale = PlayerConstants.BaseEnergyLossScale;
	public float NoEnergyHealthLoss = 10.0f;
	public PlayerDrillables playerDrillables = new();
	public Vector2 prevVelocity = new(0, 0);
	public AnimatedSprite2D bodyAnimation;
	public AnimatedSprite2D frontHeadAnimation;
	public AnimatedSprite2D sideHeadAnimation;
	public Vector2 sideHeadAnimationPosition;

	public RayCast2D leftRay;
	public RayCast2D rightRay;
	public RayCast2D upRay;
	public RayCast2D downRay;

	public Node2D flipper;
	public Node2D frontHeadProjectileParent;
	public Marker2D projectileSpawnPointFront;
	public Marker2D projectileSpawnPointSide;
	public Marker2D activeProjectileSpawnPoint;

	public Battery battery;

	public PlayerAnimation playerAnimation;
	public PlayerShaderManager playerShaderManager;
	public List<List<Drillable>> SurroundingDrillables;
	private PackedScene laserScene;
	private PlayerStateManager playerStateManager;

	public float Energy;
	public float MaxEnergy = PlayerConstants.BaseEnergy;
	public float Health;

	public GetUnitDistanceBetweenPoints getUnitDistanceBetweenPoints;

    public override void _Ready()
    {
		Node2D flipper = GetNode<Node2D>("%flipper");
		battery = GetNode<Node2D>("%body_AnimatedSprite2D/Battery/Battery0") as Battery;
		this.flipper = flipper;
		frontHeadProjectileParent = GetNode<Node2D>("%front_head_projectile_spawn");
		projectileSpawnPointFront = GetNode<Marker2D>("%player_front_head_projectile_spawn_point");
		projectileSpawnPointSide = GetNode<Marker2D>("%player_side_head_projectile_spawn_point");
		activeProjectileSpawnPoint = projectileSpawnPointFront;

		laserScene = GD.Load<PackedScene>("res://scenes/laser.tscn");
		AnimatedSprite2D bodyAnimation = GetNode<AnimatedSprite2D>("%body_AnimatedSprite2D");
		AnimatedSprite2D frontHeadAnimation = GetNode<AnimatedSprite2D>("%front_head_AnimatedSprite2D");
		AnimatedSprite2D sideHeadAnimation = GetNode<AnimatedSprite2D>("%side_head_AnimatedSprite2D");
		sideHeadAnimationPosition = sideHeadAnimation.Position;

		leftRay = GetNode<RayCast2D>("raycasts/left_RayCast2D");
		rightRay = GetNode<RayCast2D>("raycasts/right_RayCast2D");
		downRay = GetNode<RayCast2D>("raycasts/down_RayCast2D");
		upRay = GetNode<RayCast2D>("raycasts/up_RayCast2D");

		this.bodyAnimation = bodyAnimation;
		this.frontHeadAnimation = frontHeadAnimation;
		this.sideHeadAnimation = sideHeadAnimation;
		Node2D frontSprites = GetNode<Node2D>("%body_AnimatedSprite2D/front");
		Node2D sideSprites = GetNode<Node2D>("%body_AnimatedSprite2D/side");
		AnimatedSprite2D jetAnimation = GetNode<AnimatedSprite2D>("%jet_AnimatedSprite2D");
		AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("%player_AnimationPlayer");
		AnimationPlayer headAnimationPlayer = GetNode<AnimationPlayer>("%head_AnimationPlayer");
		AnimationPlayer shaderAnimationPlayer = GetNode<AnimationPlayer>("%shader_AnimationPlayer");
		playerShaderManager = GetNode<Node2D>("%shaderManager") as PlayerShaderManager;	
		playerShaderManager.UpdateDrills(DrillType.Base);

		playerAnimation = new PlayerAnimation(flipper, bodyAnimation, jetAnimation, frontSprites, sideSprites,
			animationPlayer, headAnimationPlayer, shaderAnimationPlayer, battery);

		SurroundingDrillables = new List<List<Drillable>>();
		for (int i = 0; i < 3; i++)
		{
			SurroundingDrillables.Add(new List<Drillable>());
			for (int j = 0; j < 3; j++)
			{
				SurroundingDrillables[i].Add(null);
			}
		}

		playerStateManager = new PlayerStateManager(this);

		Energy = 100.0f;
		Health = 100.0f;
    }

    public override void _PhysicsProcess(double delta)
	{
		HandleTestInput();
		HandleEnergy((float)delta);
		HandleRaycastChecks();
		battery.SetCharge(Energy/100.0f);
		HandleSideHead((float) delta);
		HandleFrontHead((float) delta);

		if (playerAnimation.IsFacingForward())
		{
			activeProjectileSpawnPoint = projectileSpawnPointFront;
		} else 
		{
			activeProjectileSpawnPoint = projectileSpawnPointSide;
		}

		if (Input.IsActionJustPressed("fire") && playerStateManager.CanShoot())
		{
			Shoot();
		}

		playerStateManager.Update((float)delta);

		prevVelocity = Velocity;
		MoveAndSlide();
	}

	private void HandleRaycastChecks()
	{
		HandleRaycastCheck(leftRay, DrillFromDirection.RIGHT);
		HandleRaycastCheck(rightRay, DrillFromDirection.LEFT);
		HandleRaycastCheck(downRay, DrillFromDirection.UP);
		HandleRaycastCheck(upRay, DrillFromDirection.DOWN);
	}

	private void HandleRaycastCheck(RayCast2D ray, DrillFromDirection direction)
	{
		if (ray.IsColliding())
		{
			Drillable drillable = (Drillable) ray.GetCollider();
			if (direction != DrillFromDirection.UP || Mathf.Abs(drillable.GlobalPosition.X - GlobalPosition.X) < 12.0f)
			{
				playerDrillables.RegisterDrillable(drillable, direction);
			} else 
			{
				playerDrillables.UnregisterDrillable(direction);
			}
		} else if (!ray.IsColliding())
		{
			playerDrillables.UnregisterDrillable(direction);
		}
	}

	private void HandleFrontHead(float delta) 
	{
		bool isPlayerFacingMouse = ((IsFacingLeft() && (GetGlobalMousePosition().X < this.GlobalPosition.X)) 
		|| (!IsFacingLeft() && (GetGlobalMousePosition().X > this.GlobalPosition.X)));
		if (!isPlayerFacingMouse && playerAnimation.IsFacingForward())
		{
			this.flipper.Scale = new Vector2(this.flipper.Scale.X * -1, this.flipper.Scale.Y);
		}

		bool isFlipped = this.flipper.Scale.X < 0;

		if (playerStateManager.CanShoot())
		{
			float angleToMouse = frontHeadAnimation.Position.AngleToPoint(GetLocalMousePosition());
			float angleToMouseLocal = angleToMouse -= Rotation;

			if (!isFlipped) 
			{
				angleToMouseLocal = Common.FlipAngleYAxis(angleToMouseLocal);
			}

			float targetRotation = 0;
			if (angleToMouseLocal > Mathf.DegToRad(-90) && angleToMouseLocal < Mathf.DegToRad(50))
			{
				playerAnimation.SetHeadAnimation(PlayerHeadState.LookSide);
				targetRotation = Common.FlipAngleYAxis(angleToMouseLocal + Mathf.Pi);
			} else if (angleToMouseLocal >= Mathf.DegToRad(50) && angleToMouseLocal <= Mathf.DegToRad(90))
			{
				playerAnimation.SetHeadAnimation(PlayerHeadState.LookDown);
				targetRotation = Common.FlipAngleYAxis(angleToMouseLocal + Mathf.Pi/2);
			}

			float result = Common.MoveToward(frontHeadAnimation.Rotation, targetRotation, delta * 10.0f);
			frontHeadAnimation.Rotation = result;
			frontHeadProjectileParent.Rotation = Common.FlipAngleYAxis(angleToMouseLocal + Mathf.Pi);
		} else 
		{
			frontHeadAnimation.Rotation = 0;
			frontHeadProjectileParent.Rotation = 0;
		}
	}

	private void HandleSideHead(float delta)
	{
		bool isPlayerFacingMouse = ((IsFacingLeft() && (GetGlobalMousePosition().X < this.GlobalPosition.X)) 
		|| (!IsFacingLeft() && (GetGlobalMousePosition().X > this.GlobalPosition.X)));

		bool isFlipped = this.flipper.Scale.X < 0;
		
		float angleToMouse = frontHeadAnimation.Position.AngleToPoint(GetLocalMousePosition());
		float angleToMouseLocal = angleToMouse -= Rotation;

		if (!isFlipped) 
		{
			angleToMouseLocal = Common.FlipAngleYAxis(angleToMouseLocal);
		}

		if (isPlayerFacingMouse && playerStateManager.CanShoot())
		{
			float xOffset = 0;
			float angleToMouseLocalLimited = angleToMouseLocal;
			if (angleToMouseLocal > Mathf.DegToRad(65))
			{
				angleToMouseLocalLimited = Mathf.DegToRad(65);
			}
			
			if (angleToMouseLocalLimited > Mathf.DegToRad(50)) 
			{
				xOffset = -3.0f*(angleToMouseLocalLimited - Mathf.DegToRad(50))/Mathf.DegToRad(40);
			}
			
			sideHeadAnimation.Rotation = Common.FlipAngleYAxis(angleToMouseLocalLimited + Mathf.Pi);
			sideHeadAnimation.Position = new Vector2(sideHeadAnimationPosition.X + xOffset, sideHeadAnimation.Position.Y);
			float projectileSpawnAngleToMouse = projectileSpawnPointSide.GlobalPosition.AngleToPoint(GetGlobalMousePosition());
			projectileSpawnPointSide.GlobalRotation = projectileSpawnAngleToMouse + Mathf.Pi;
		} else {
			sideHeadAnimation.Position = new Vector2(sideHeadAnimationPosition.X, sideHeadAnimation.Position.Y);
			sideHeadAnimation.Rotation = 0;
			projectileSpawnPointSide.Rotation = 0;
		}
	}

	private void HandleTestInput()
	{
		if (Input.IsActionJustPressed("test4"))
		{
			VerticalFlightSpeed = -900.0f;
			CatchVerticalFlightSpeed = -1100.0f;
		}
		if (Input.IsActionJustPressed("test5"))
		{
			VerticalFlightSpeed = -1100.0f;
			CatchVerticalFlightSpeed = -1300.0f;
		}
		if (Input.IsActionJustPressed("test6"))
		{
			VerticalFlightSpeed = -1300.0f;
			CatchVerticalFlightSpeed = -1500.0f;
		}
		if (Input.IsActionJustPressed("test7"))
		{
			VerticalFlightSpeed = -1500.0f;
			CatchVerticalFlightSpeed = -1700.0f;
		}
	}

	private void HandleEnergy(float delta)
	{
		if (Input.IsActionJustPressed("test0"))
		{
			if (EnergyLossScale == PlayerConstants.BaseEnergyLossScale)
			{
				EnergyLossScale = 0.0f;
			} else {
				EnergyLossScale = PlayerConstants.BaseEnergyLossScale;
			}
		}

		Energy -= delta / 4.0f * EnergyLossScale;
		if (playerStateManager.currentState == PlayerState.Airborne && Input.IsActionPressed("fly"))
		{
			Energy -= delta * EnergyLossScale;
		} else if (playerStateManager.currentState == PlayerState.Drilling)
		{
			Energy -= delta * EnergyLossScale;
		} else if (playerStateManager.currentState == PlayerState.Grounded)
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
		}
	}

	public void Shoot()
	{
		Laser laser = laserScene.Instantiate() as Laser;
		laser.transformSource = activeProjectileSpawnPoint;
		laser.flipDirection = true;
		laser.ZIndex = 2;
		Node2D projectiles = GetParent().GetNode<Node2D>("Projectiles");
		projectiles.AddChild(laser);
	}

	private Vector2 ApplyDrag(Vector2 velocity) 
	{
		Vector2 drag = new(Mathf.Pow(Mathf.Abs(velocity.X), 1.05f), Mathf.Pow(Mathf.Abs(velocity.Y), 1.05f));
		if (velocity.X > 0)
			drag.X *= -1;
		if (velocity.Y > 0)
			drag.Y *= -1;
		drag *= PlayerConstants.DragConstant;
		return new(velocity.X + drag.X, velocity.Y + drag.Y);
	}

	public void HandleDrillUpgrade(DrillUpgrade drillUpgrade)
    {
		DrillSideSpeed = PlayerConstants.BaseDrillSpeed * drillUpgrade.drillSpeedMultiplier;
		DrillVerticalSpeed = PlayerConstants.BaseDrillSpeed * drillUpgrade.drillSpeedMultiplier;
		playerShaderManager.UpdateDrills(drillUpgrade.drillType);
    }

	public void HandleBatteryUpgrade(BatteryUpgrade batteryUpgrade)
	{
		MaxEnergy = PlayerConstants.BaseEnergy * batteryUpgrade.capacity / 100.0f;
		Energy = MaxEnergy;
		battery.Visible = false;
		battery = GetNode<Node2D>("%body_AnimatedSprite2D/Battery/Battery" + batteryUpgrade.Rank()) as Battery;
		battery.Visible = true;
		playerAnimation.UpdateBattery(battery);
	}

	private void _on_player_animation_player_animation_finished(String anim_name) 
	{
		playerStateManager.AnimationFinished(anim_name);
	}

	private void _on_hit_box_body_entered(Node2D node)
	{
		if (node is Enemy enemy)
		{
			Health -= enemy.Damage;
			playerAnimation.PlayHurt();
		}
	}

	private bool IsFacingLeft() 
	{
		return this.flipper.Scale.X > 0;
	}

	public void SetUnitDistanceDelegate(GetUnitDistanceBetweenPoints getUnitDistanceBetweenPoints)
	{
		this.getUnitDistanceBetweenPoints = getUnitDistanceBetweenPoints;
	}
}
