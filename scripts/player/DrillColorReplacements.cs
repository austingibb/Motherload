using Godot;
using System;

public partial class DrillColorReplacements : Node2D
{
    // drill color replacements
    [Export]
    public Color[] drillColorsToReplace;

    [Export]
    public Color[] s_drillColorReplacements;
    [Export]
    public Color[] a_drillColorReplacements;
    [Export]
    public Color[] b_drillColorReplacements;
    [Export]
    public Color[] c_drillColorReplacements;
    [Export]
    public Color[] d_drillColorReplacements;
    [Export]
    public Color[] f_drillColorReplacements;


    public Color[] DrillTypeToColorReplacement(DrillType drillType)
    {
        switch (drillType)
        {
            case DrillType.Base:
                return f_drillColorReplacements;
            case DrillType.Steel:
                return d_drillColorReplacements;
            case DrillType.Topaz:
                return c_drillColorReplacements;
            case DrillType.Sapphire:
                return b_drillColorReplacements;
            case DrillType.Diamond:
                return a_drillColorReplacements;
            case DrillType.QCarbon:
                return s_drillColorReplacements;
            default:
                return f_drillColorReplacements;
        }
    }
}
