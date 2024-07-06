using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerShaderManager : Node2D
{
    // armor color replacements
    [Export]
    public Color[] armorColorsToReplace;
    [Export]
    public Color[] a_armorColorReplacements;
    [Export]
    public Color[] b_armorColorReplacements;
    [Export]
    public Color[] c_armorColorReplacements;
    [Export]
    public Color[] d_armorColorReplacements;
    [Export]
    public Color[] f_armorColorReplacements;

    // drill color replacements
    DrillColorReplacements drillColorReplacements;

    // nodes
    public ShaderMaterial bodyShader;
    // body
    // front
    public ShaderMaterial headShader;
    public ShaderMaterial drillsShader;
    public ShaderMaterial armsShader;
    // side
    public ShaderMaterial sideHeadShader;
    public ShaderMaterial frontDrillShader;
    public ShaderMaterial backDrillShader;
    public ShaderMaterial frontArmShader;
    public ShaderMaterial backArmShader;

    public override void _Ready()
    {
        drillColorReplacements = GetNode<DrillColorReplacements>("drillColorReplacements");

        // body
        AnimatedSprite2D bodyAnimation = GetNode<AnimatedSprite2D>("%body_AnimatedSprite2D");
        bodyShader = bodyAnimation.Material as ShaderMaterial;
		// front
        AnimatedSprite2D headAnimation = GetNode<AnimatedSprite2D>("%front_head_AnimatedSprite2D");
        headShader = headAnimation.Material as ShaderMaterial;
        AnimatedSprite2D drillsAnimation = GetNode<AnimatedSprite2D>("%drills_AnimatedSprite2D");
        drillsShader = drillsAnimation.Material as ShaderMaterial;
        AnimatedSprite2D armsAnimation = GetNode<AnimatedSprite2D>("%arms_AnimatedSprite2D");
        armsShader = armsAnimation.Material as ShaderMaterial;
        // side
        AnimatedSprite2D sideHeadAnimation = GetNode<AnimatedSprite2D>("%side_head_AnimatedSprite2D");
        sideHeadShader = sideHeadAnimation.Material as ShaderMaterial;
        AnimatedSprite2D frontArmAnimation = GetNode<AnimatedSprite2D>("%front_arm_AnimatedSprite2D");
        frontArmShader = frontArmAnimation.Material as ShaderMaterial;
        AnimatedSprite2D backArmAnimation = GetNode<AnimatedSprite2D>("%back_arm_AnimatedSprite2D");
        backArmShader = backArmAnimation.Material as ShaderMaterial;
        AnimatedSprite2D frontDrillAnimation = GetNode<AnimatedSprite2D>("%front_drill_AnimatedSprite2D");
        frontDrillShader = frontDrillAnimation.Material as ShaderMaterial;
        AnimatedSprite2D backDrillAnimation = GetNode<AnimatedSprite2D>("%back_drill_AnimatedSprite2D");
        backDrillShader = backDrillAnimation.Material as ShaderMaterial;
    }

    public void UpdateArmor(ArmorType armorType)
    {
        Color[] replacementArmorColors = ArmorTypeToColors(armorType); 
        UpdateColorReplaceParameters(bodyShader, armorColorsToReplace, replacementArmorColors);
        UpdateColorReplaceParameters(headShader, armorColorsToReplace, replacementArmorColors);
        UpdateColorReplaceParameters(armsShader,armorColorsToReplace, replacementArmorColors);
        UpdateColorReplaceParameters(sideHeadShader, armorColorsToReplace, replacementArmorColors);
        UpdateColorReplaceParameters(frontArmShader, armorColorsToReplace, replacementArmorColors);
        UpdateColorReplaceParameters(backArmShader, armorColorsToReplace, replacementArmorColors);
    }

    public void UpdateDrills(DrillType drillType)
    {
        Color[] drillColors = DrillTypeToColors(drillType);
        UpdateColorReplaceParameters(drillsShader, drillColorReplacements.drillColorsToReplace, drillColors);
        UpdateColorReplaceParameters(frontDrillShader, drillColorReplacements.drillColorsToReplace, drillColors);
        UpdateColorReplaceParameters(backDrillShader, drillColorReplacements.drillColorsToReplace, drillColors);
    }

    public static void UpdateColorReplaceParameters(ShaderMaterial shaderMaterial, Color[] colorsToReplace, Color[] replacementColors)
    {
        if (replacementColors == null || colorsToReplace.Length != replacementColors.Length)
        {
            colorsToReplace = new Color[0];
            replacementColors = new Color[0];
        }
        shaderMaterial.SetShaderParameter("colors_to_replace", colorsToReplace);
        shaderMaterial.SetShaderParameter("replacement_colors", replacementColors);
    }

    public Color[] ArmorTypeToColors(ArmorType armorType)
    {
        switch (armorType)
        {
            case ArmorType.A:
                return a_armorColorReplacements;
            case ArmorType.B:
                return b_armorColorReplacements;
            case ArmorType.C:
                return c_armorColorReplacements;
            case ArmorType.D:
                return d_armorColorReplacements;
            case ArmorType.F:
                return f_armorColorReplacements;
            default:
                return f_armorColorReplacements;
        }
    }

    public Color[] DrillTypeToColors(DrillType drillType)
    {
        switch (drillType)
        {
            case DrillType.QCarbon:
                return drillColorReplacements.s_drillColorReplacements;
            case DrillType.Diamond:
                return drillColorReplacements.a_drillColorReplacements;
            case DrillType.Sapphire:
                return drillColorReplacements.b_drillColorReplacements;
            case DrillType.Topaz:
                return drillColorReplacements.c_drillColorReplacements;
            case DrillType.Steel:
                return drillColorReplacements.d_drillColorReplacements;
            case DrillType.Base:
                return drillColorReplacements.f_drillColorReplacements;
            default:
                return drillColorReplacements.f_drillColorReplacements;
        }
    }
}
