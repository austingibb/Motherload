using Godot;
using System;

public partial class ChargeStation : StaticBody2D
{
    [Signal]
    public delegate void chargeEventHandler();

    public override void _Ready()
    {
        
    }

    private void _on_charge_zone_entered(Node2D body)
	{
		EmitSignal(SignalName.charge);
	}
}
