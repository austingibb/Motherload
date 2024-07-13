using Godot;

public partial class Spider : Enemy, ChunkItem
{
    public CollisionPolygon2D collisionPolygon2D;
    public RayCast2D ray;
    public double Speed = 3000.0f;

    Vector2I direction;

    public bool IsDisabled = false;

    public override void _Ready()
    {
        this.Health = 100;
        this.MaxHealth = 100;

        ray = GetNode<RayCast2D>("RayCast2D");
        collisionPolygon2D = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
        direction = new Vector2I(0, -1);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDisabled)
        {
            return;
        }

        if (ray.IsColliding() || (GlobalPosition.Y < 10 && direction.Y < 0))
        {
            direction = new Vector2I(direction.X, direction.Y * -1);
            Scale = new Vector2(Scale.X, Scale.Y * -1);
        }

        Vector2 velocity = Velocity;
        velocity.Y = (float) Speed * (float) delta;

        if (direction.Y < 0)
        {
            velocity.Y *= -1;
        }


        Velocity = velocity;
        MoveAndSlide();
    }

    public void Disable()
    {
        IsDisabled = true;
        Visible = false;
        collisionPolygon2D.Disabled = true;
    }

    public void Enable()
    {
        IsDisabled = false;
        Visible = true;
        collisionPolygon2D.Disabled = false;
    }

    public Vector2 GetPosition()
    {
        return GlobalPosition;
    }
}