using Godot;
using System;

public partial class SellStation : StaticBody2D
{
    [Signal]
    public delegate void sellAllEventHandler();

    public override void _Ready()
    {
        
    }

    private void _on_sell_zone_entered(Node2D body)
	{
		EmitSignal(SignalName.sellAll);
	}
}
