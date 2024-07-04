using Godot;
using System;

public partial class UpgradeStation : StaticBody2D
{
    [Signal]
    public delegate void upgradeEventHandler();

    public override void _Ready()
    {
        
    }

    private void _on_upgrade_zone_entered(Node2D body)
	{
		EmitSignal(SignalName.upgrade);
	}
}
