using Godot;
using System;

public partial class game : Node2D
{
    public override void _PhysicsProcess(double delta)
	{
        if (Input.IsActionPressed("reset"))
        {
            GetTree().ReloadCurrentScene();
        }
    }
}