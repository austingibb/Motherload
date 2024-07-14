using Godot;
using System;

public enum DrillFromDirection 
{
	LEFT,
	UP,
	DOWN,
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
	public delegate void dugEventHandler(Drillable drillable);

	[Signal]
	public delegate void preDugEventHandler(Drillable drillable, DrillFromDirection direction);

	public override void _Ready()
	{
		this.tileType = TileType.Drillable;
		this.collisionShape2D = GetNode<CollisionShape2D>("./CollisionShape2D");
		this.drillableAnimation = new DrillableAnimation(this);

		base._Ready();
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

	public void FinishDrillAnimation()
	{
		IsDug = true;
		EmitSignal(SignalName.dug, this);
		QueueFree();
	}
}
