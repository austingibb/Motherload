using Godot;
using System;

public enum DrillFromDirection 
{
	LEFT,
	UP,
	RIGHT,
	NONE
}

public partial class Drillable : StaticBody2D
{
	public AnimationPlayer animationPlayer;
	public CollisionShape2D collisionShape2D;

	public override void _Ready()
	{
		AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("./AnimatedSprite2D/AnimationPlayer");
		this.animationPlayer = animationPlayer;
		this.collisionShape2D = GetNode<CollisionShape2D>("./CollisionShape2D");
	}

	public void StartDrillAnimation(DrillFromDirection direction) 
	{
		if (direction == DrillFromDirection.LEFT)
			this.animationPlayer.Play("dig_from_left");
		else if (direction == DrillFromDirection.RIGHT)
			this.animationPlayer.Play("dig_from_right");
		else if (direction == DrillFromDirection.UP)
			this.animationPlayer.Play("dig_top");
		this.collisionShape2D.Disabled = true;
		GD.Print("Drillable disabling collision shape");

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

	private void _on_animation_player_animation_finished(string anim_name)
	{
		if (anim_name == "dig_from_left" || anim_name == "dig_from_right" || anim_name == "dig_top")
		{
			QueueFree();
		}
	}
}
