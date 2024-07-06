using Godot;
using System;
using System.Collections.Generic;

public partial class UpgradeMenu : CanvasLayer
{
    List<Button> upgradeButtons = new List<Button>();
    List<TextureRect> upgradeIcons = new List<TextureRect>();
    List<Label> upgradeDescriptions = new List<Label>();

    GridContainer currentUpgradeDisplay;

    DrillColorReplacements drillColorReplacements;
    DrillUpgrade currentDrillUpgrade = UpgradeConstants.upgrades[0] as DrillUpgrade;
    MoneyAuthorization moneyAuthorization;

    [Signal]
    public delegate void upgradeEventHandler(int upgradeIndex);

    public override void _Ready()
    {
        drillColorReplacements = GetNode<DrillColorReplacements>("%drillColorReplacements");

        foreach (Node child in Common.GetAllChildren(this))
        {
            if (child is Button)
            {
                Button button = (Button)child;
                upgradeButtons.Add(button);
                TextureRect textureRect = button.GetNode<TextureRect>("VBoxContainer/Panel/TextureRect"); 
                upgradeIcons.Add(textureRect);
                Label label = button.GetNode<Label>("VBoxContainer/Panel2/Type");
                upgradeDescriptions.Add(label);
            }
        }

        foreach (Upgrade upgrade in UpgradeConstants.upgrades)
        {
            if (upgrade.upgradeType != UpgradeType.Drill)
            {
                continue;
            }

            DrillUpgrade drillUpgrade = (DrillUpgrade)upgrade;
            int index = (int) drillUpgrade.drillType;
            if (index == 0)
            {
                continue;
            }

            index -= 1;
            Button button = upgradeButtons[index];
            TextureRect textureRect = upgradeIcons[index];
            Label label = upgradeDescriptions[index];

            label.Text = upgrade.name + ": " + upgrade.description + " - " + upgrade.cost + " credits";
            ShaderMaterial shaderMaterial = textureRect.Material as ShaderMaterial;
            PlayerShaderManager.UpdateColorReplaceParameters(shaderMaterial, drillColorReplacements.drillColorsToReplace, drillColorReplacements.DrillTypeToColorReplacement(drillUpgrade.drillType));
        }

        currentUpgradeDisplay = GetNode<GridContainer>("%CurrentUpgrade");
        UpdateCurrentDrillUpgradeDisplay();
    }
    
    public void UpdateCurrentDrillUpgradeDisplay()
    {
        Label currentUpgradeInfo = currentUpgradeDisplay.GetNode<Label>("Panel/InfoLabel");
        TextureRect currentUpgradeIcon = currentUpgradeDisplay.GetNode<TextureRect>("Panel/TextureRect");
        ShaderMaterial shaderMaterial = currentUpgradeIcon.Material as ShaderMaterial;
        currentUpgradeInfo.Text = currentDrillUpgrade.name + ": " + currentDrillUpgrade.description;
        PlayerShaderManager.UpdateColorReplaceParameters(shaderMaterial, drillColorReplacements.drillColorsToReplace, drillColorReplacements.DrillTypeToColorReplacement(currentDrillUpgrade.drillType));
    }

    public void DrillUpgraded()
    {
        UpdateCurrentDrillUpgradeDisplay();
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            Button button = upgradeButtons[i];
            DrillUpgrade drillUpgrade = (DrillUpgrade)UpgradeConstants.upgrades[i + 1];
            if ((int) drillUpgrade.drillType <= (int) currentDrillUpgrade.drillType) {
                button.Disabled = true;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("exit"))
        {
            this.Visible = false;
        }
    }

    public void AttemptDrillUpgrade(int upgradeIndex)
    {
        Upgrade upgrade = UpgradeConstants.upgrades[upgradeIndex];

        if (moneyAuthorization == null)
        {
            GD.PrintErr("Money authorization delegate not set.");
            return;
        }

        if (upgrade.upgradeType != UpgradeType.Drill)
        {
            GD.PrintErr("Attempted drill upgrade on non-drill upgrade");
            return;
        }

        if (upgrade.cost > 0 && moneyAuthorization(upgrade.cost))
        {
            currentDrillUpgrade = (DrillUpgrade)upgrade;
            EmitSignal(SignalName.upgrade, upgradeIndex);
            DrillUpgraded();
        }
    }

    public void _on_button_1_pressed()
    {
        AttemptDrillUpgrade(1);
    }

    public void _on_button_2_pressed()
    {
        AttemptDrillUpgrade(2);
    }

    public void _on_button_3_pressed()
    {
        AttemptDrillUpgrade(3);
    }

    public void _on_button_4_pressed()
    {
        AttemptDrillUpgrade(4);
    }

    public void _on_button_5_pressed()
    {
        AttemptDrillUpgrade(5);
    }

    public void RegisterMoneyAuthorization(MoneyAuthorization moneyAuthorization)
    {
        this.moneyAuthorization = moneyAuthorization;
    }
}
