using System;
using Godot;

public abstract class PlayerStateProcessor : StateProcessor<PlayerState>
{
    public PlayerCharacterBody2D player;
    public PlayerDrillables playerDrillables;
    public PlayerAnimation playerAnimation;

    public PlayerStateProcessor(PlayerCharacterBody2D playerCharacterBody2D)
    {
        this.player = playerCharacterBody2D;
        this.playerDrillables = player.playerDrillables;
        this.playerAnimation = player.playerAnimation;
    }

    public virtual void ApplyGravity(double delta) 
    {
        Vector2 velocity = player.Velocity;
        velocity.Y += Game.GRAVITY * (float)delta;
        player.Velocity = velocity;
    }

    public virtual void ApplyVerticalFlight(double delta)
    {
        Vector2 velocity = player.Velocity;
        if (Input.IsActionPressed("fly"))
        {
            player.playerAnimation.SetFlightState(true);
            if (velocity.Y < 0) 
			{
				velocity.Y += player.VerticalFlightSpeed * (float)delta;
			} else
			{
				velocity.Y += player.CatchVerticalFlightSpeed * (float)delta;
			}
        } else 
        {
            player.playerAnimation.SetFlightState(false);
        }
        player.Velocity = velocity;
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

        if (Mathf.Abs(velocity.X) > player.MaxHorozontalSpeed)
		{
			velocity.X = Mathf.Sign(velocity.X) * player.MaxHorozontalSpeed;
		}
        player.Velocity = velocity;
    }

    public virtual void ApplyDrag() 
    {
        Vector2 velocity = player.Velocity;
		Vector2 drag = new(Mathf.Pow(Mathf.Abs(velocity.X), 1.05f), Mathf.Pow(Mathf.Abs(velocity.Y), 1.05f));
		if (velocity.X > 0)
			drag.X *= -1;
		if (velocity.Y > 0)
			drag.Y *= -1;
		drag *= PlayerConstants.DragConstant;
		player.Velocity = new(velocity.X + drag.X, velocity.Y + drag.Y);
    }
}