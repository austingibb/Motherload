using Godot;
using System;

public partial class Battery1 : Battery
{

    public Sprite2D frontBattery;
    public Sprite2D sideBattery;
    public TextureProgressBar frontCharge;
    public TextureProgressBar sideCharge;

    public override void _Ready()
    {
        frontCharge = GetNode<TextureProgressBar>("front_Charge");
        frontCharge.Value = 0;
        sideCharge = GetNode<TextureProgressBar>("side_Charge");
        sideCharge.Value = 0;
        frontBattery = GetNode<Sprite2D>("front_Sprite2D");
        sideBattery = GetNode<Sprite2D>("side_Sprite2D"); 
    }

    public override void SetCharge(float chargePercent)
    {
        double value = Mathf.Round(chargePercent * 4.0f);
        frontCharge.Value = value;
        sideCharge.Value = value;
    }

    public override void SetOrientation(bool side)
    {
        if (side)
        {
            sideCharge.Visible = true;
            frontCharge.Visible = false;
            sideBattery.Visible = true;
            frontBattery.Visible = false;
        } else {
            sideCharge.Visible = false;
            frontCharge.Visible = true;
            sideBattery.Visible = false;
            frontBattery.Visible = true;
        }
    }
}
