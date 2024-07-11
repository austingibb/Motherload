using Godot;

public partial class Spider : Enemy
{
    public RayCast2D ray;
    public double Speed = 3000.0f;

    Vector2I direction;

    public override void _Ready()
    {
        this.Health = 100;
        this.MaxHealth = 100;

        ray = GetNode<RayCast2D>("RayCast2D");
        direction = new Vector2I(0, -1);
    }

    public override void _PhysicsProcess(double delta)
    {
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
}