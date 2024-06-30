using Godot;
using System;

public partial class EnergyShaderAnimationPlayer : AnimationPlayer
{
    public override void _Ready()
    {
        Play("RESET");
    }
}
