using Godot;
using System;

public enum DrillFromDirection 
{
	LEFT,
	UP,
	RIGHT,
	NONE
}

public partial class Drillable : Tile
{
	public CollisionShape2D collisionShape2D;
	public DrillableAnimation drillableAnimation;
	public Boolean IsDug = false;
	
	[Export]
	public DrillableType DrillableType;

	[Signal]
	public delegate void dugEventHandler(Node2D drillable);

	[Signal]
	public delegate void preDugEventHandler(Node2D drillable, DrillFromDirection direction);

	public override void _Ready()
	{
		this.tileType = TileType.Drillable;
		this.collisionShape2D = GetNode<CollisionShape2D>("./CollisionShape2D");
		this.drillableAnimation = new DrillableAnimation(this);
	}

	public void StartDrillAnimation(DrillFromDirection direction) 
	{
		this.drillableAnimation.StartAnimation(direction);
		this.collisionShape2D.Disabled = true;
		EmitSignal(SignalName.preDug, this, (int) drillableAnimation.drillFromDirection);
	}

	public void UpdateDrillAnimationFromPosition(Node2D drillingEntity)
	{
		bool finished = this.drillableAnimation.UpdateAnimationFromPosition(drillingEntity);
		if (finished)
		{
			this.FinishDrillAnimation();
		}
	}

	public void HandleBodyEntered(Node2D body, DrillFromDirection direction)
	{
		if (body.Name == "Player")
		{
			PlayerCharacterBody2D player = body as PlayerCharacterBody2D;
			player.RegisterDrillable(this, direction);
		}
	}

	public void HandleBodyExited(Node2D body)
	{
		if (body.Name == "Player")
		{
			PlayerCharacterBody2D player = body as PlayerCharacterBody2D;
			player.UnregisterDrillable(this);
		}
	}

	public void FinishDrillAnimation()
	{
		IsDug = true;
		EmitSignal(SignalName.dug, this);
		QueueFree();
	}

	private void _on_left_drill_zone_entered(Node2D body)
	{
		HandleBodyEntered(body, DrillFromDirection.LEFT);
	}

	private void _on_left_drill_zone_exited(Node2D body) 
	{
		HandleBodyExited(body);
	}

	private void _on_right_drill_zone_entered(Node2D body)
	{
		HandleBodyEntered(body, DrillFromDirection.RIGHT);
	}

	private void _on_right_drill_zone_exited(Node2D body)
	{
		HandleBodyExited(body);
	}

	private void _on_top_drill_zone_entered(Node2D body)
	{
		HandleBodyEntered(body, DrillFromDirection.UP);
	}

	private void _on_top_drill_zone_exited(Node2D body)
	{
		HandleBodyExited(body);
	}
}
