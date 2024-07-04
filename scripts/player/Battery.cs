using Godot;
using System;

public abstract partial class Battery : Node2D
{
    public abstract void SetCharge(float chargePercent);
    public abstract void SetOrientation(bool side);
}
