using Godot;
using System;
using System.Collections.Generic;

public abstract partial class UpgradeTab : Control
{
    public List<Button> upgradeButtons = new List<Button>();
    public List<TextureRect> upgradeIcons = new List<TextureRect>();
    public List<Label> upgradeDescriptions = new List<Label>();

    public GridContainer currentUpgradeDisplay;

    public List<Upgrade> upgrades;
    public Upgrade currentUpgrade;
    public MoneyAuthorization moneyAuthorization;

    [Signal]
    public delegate void upgradeEventHandler(int upgradeIndex);

    public abstract List<Upgrade> GetUpgrades();

    public override void _Ready()
    {
        var buttonPressDelegates = new List<Action> {
            _on_button_1_pressed,
            _on_button_2_pressed,
            _on_button_3_pressed,
            _on_button_4_pressed,
            _on_button_5_pressed
        };

        upgrades = GetUpgrades();
        currentUpgrade = upgrades[0];

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

        foreach (Upgrade upgrade in upgrades)
        {
            int index = upgrade.Rank();
            if (index == 0)
            {
                continue;
            }

            index -= 1;
            Button button = upgradeButtons[index];
            button.Pressed += buttonPressDelegates[index];
            TextureRect textureRect = upgradeIcons[index];
            Label label = upgradeDescriptions[index];

            SetupUpgradeInfo(textureRect, label, upgrade);
        }

        currentUpgradeDisplay = GetNode<GridContainer>("VBoxContainer/CurrentUpgrade");
        UpdateCurrentUpgradeDisplay();
    }
    
    public virtual void SetupUpgradeInfo(TextureRect textureRect, Label label, Upgrade upgrade)
    {
        label.Text = upgrade.name + ": " + upgrade.description + $" - {upgrade.cost} credits";
    }

    public virtual void SetupCurrentUpgradeInfo(TextureRect textureRect, Label label, Upgrade upgrade)
    {
        label.Text = upgrade.name + ": " + upgrade.description;
    }

    public void UpdateCurrentUpgradeDisplay()
    {
        Label currentUpgradeInfo = currentUpgradeDisplay.GetNode<Label>("Panel/InfoLabel");
        TextureRect currentUpgradeIcon = currentUpgradeDisplay.GetNode<TextureRect>("Panel/TextureRect");
        SetupCurrentUpgradeInfo(currentUpgradeIcon, currentUpgradeInfo, currentUpgrade);
    }

    public void Upgraded()
    {
        UpdateCurrentUpgradeDisplay();
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            Button button = upgradeButtons[i];
            Upgrade upgrade = upgrades[i + 1];
            if (upgrade.Rank() <= currentUpgrade.Rank()) {
                button.Disabled = true;
            }
        }
    }

    public void AttemptUpgrade(int upgradeIndex)
    {
        Upgrade upgrade = upgrades[upgradeIndex];

        if (moneyAuthorization == null)
        {
            GD.PrintErr("Money authorization delegate not set.");
            return;
        }

        if (upgrade.cost > 0 && moneyAuthorization(upgrade.cost))
        {
            currentUpgrade = upgrade;
            EmitSignal(SignalName.upgrade, upgradeIndex);
            Upgraded();
        }
    }

    public void _on_button_1_pressed()
    {
        AttemptUpgrade(1);
    }

    public void _on_button_2_pressed()
    {
        AttemptUpgrade(2);
    }

    public void _on_button_3_pressed()
    {
        AttemptUpgrade(3);
    }

    public void _on_button_4_pressed()
    {
        AttemptUpgrade(4);
    }

    public void _on_button_5_pressed()
    {
        AttemptUpgrade(5);
    }

    public void RegisterMoneyAuthorization(MoneyAuthorization moneyAuthorization)
    {
        this.moneyAuthorization = moneyAuthorization;
    }
}
