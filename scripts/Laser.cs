using Godot;
using System;

public partial class Laser : CharacterBody2D
{
    public float speed = 20.0f;
    public Node2D transformSource;
    public bool flipDirection;

    public bool initialized = false;

    public override void _Ready()
    {
        Visible = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
        if (!initialized)
        {
            Visible = true;
            if (transformSource != null)
            {
                GlobalPosition = transformSource.GlobalPosition;
                GlobalRotation = transformSource.GlobalRotation;
                if (flipDirection)
                {
                    GlobalRotation += Mathf.DegToRad(180);
                }
            }

            initialized = true;
            Velocity = new Vector2(0, -speed).Rotated(GlobalRotation + Mathf.DegToRad(90)) * speed;
        }
    }

    private void _on_body_entered(Node2D body)
    {
        QueueFree();
    }
}
