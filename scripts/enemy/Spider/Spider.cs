using System;
using Godot;

public partial class Spider : Enemy, ChunkItem
{
    public Node2D player;
    public CollisionPolygon2D collisionPolygon2D;
    public double Speed = 2500.0f;
    public double OscillationSpeed = 4.0f;
    public double RotationDir = 1.0f;
    public bool IsDisabled = false;
    public SpiderStateManager spiderStateManager;

    public override void _Ready()
    {
        this.Health = 100;
        this.MaxHealth = 100;
        this.Damage = 10;

        collisionPolygon2D = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
        player = GetNode<Node>("/root/Game").GetNode<Node2D>("%Player");
        Rotation = Mathf.Tau*3 + Mathf.Pi/3;
        spiderStateManager = new SpiderStateManager(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDisabled)
        {
            return;
        }

        spiderStateManager.Update(delta);

        MoveAndSlide();
    }

    public override void Disable()
    {
        IsDisabled = true;
        Visible = false;
        collisionPolygon2D.Disabled = true;
    }

    public override void Enable()
    {
        IsDisabled = false;
        Visible = true;
        collisionPolygon2D.Disabled = false;
    }

    public override Vector2 GetPosition()
    {
        return GlobalPosition;
    }
}