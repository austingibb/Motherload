using Godot;
using System;

public partial class Battery5 : Battery
{

    public Node2D front;
    public Node2D side;
    public TextureProgressBar frontCharge;
    public TextureProgressBar sideCharge;

    public override void _Ready()
    {
        frontCharge = GetNode<TextureProgressBar>("front/front_Charge");
        frontCharge.Value = 1;
        sideCharge = GetNode<TextureProgressBar>("side/side_Charge");
        sideCharge.Value = 1;
        
        front = GetNode<Node2D>("front");
        side = GetNode<Node2D>("side");
    }

    public override void SetCharge(float chargePercent)
    {
        double value = Mathf.Round(chargePercent * 5.0f);
        frontCharge.Value = 1 + value;
        sideCharge.Value = 1 + value;
    }

    public override void SetOrientation(bool showSide)
    {
        if (showSide)
        {
            side.Visible = true;
            front.Visible = false;
        } else {
            side.Visible = false;
            front.Visible = true;
        }
    }
}
