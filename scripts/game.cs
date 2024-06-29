using Godot;
using System;
using System.Text;
using System.Collections.Generic;

public partial class Game : Node2D
{
    // Nodes
    public PlayerCharacterBody2D playerCharacter;
    public DrillableGrid drillableGrid;
    public Inventory inventory;
    public CanvasLayer hud;
    public Label inventoryLabel;
    public Label moneyLabel;
    public ProgressBarNinePatch energyBar;
    public AnimationPlayer energyBarAnimator;
    public ProgressBarNinePatch healthBar;
    public SellStation sellStation;
    public ChargeStation chargeStation;

    // Member variables
    public int Money;

    public override void _Ready()
    {
        playerCharacter = GetNode<CharacterBody2D>("%Player") as PlayerCharacterBody2D;
        drillableGrid = GetNode<TileMap>("%DrillableGrid") as DrillableGrid;
        drillableGrid.drillableDug += _on_drillable_dug;
        inventory = GetNode<Inventory>("%Inventory") as Inventory;
        hud = GetNode<CanvasLayer>("%HUD") as CanvasLayer;
        hud.Visible = true;
        inventoryLabel = GetNode<Label>("%HUD/Inventory/InventoryLabel");
        moneyLabel = GetNode<Label>("%HUD/Money/MoneyLabel");
        sellStation = GetNode<StaticBody2D>("%SellStation") as SellStation;
        sellStation.sellAll += _on_sell_all;
        chargeStation = GetNode<StaticBody2D>("%ChargeStation") as ChargeStation;
        chargeStation.charge += _on_charge;
        energyBar = GetNode<ProgressBarNinePatch>("%HUD/ProgressBars//Energy") as ProgressBarNinePatch;
        energyBarAnimator = GetNode<AnimationPlayer>("%HUD/ProgressBars/Energy/AnimationPlayer");
        healthBar = GetNode<ProgressBarNinePatch>("%HUD/ProgressBars/Health") as ProgressBarNinePatch;

        Money = 0;

        inventoryLabel.Text = GetInventoryString();
        moneyLabel.Text = GetMoneyString();

        List<Node2D> buildings = new List<Node2D>();
        buildings.Add(sellStation);
        buildings.Add(chargeStation);
        drillableGrid.Init(buildings);
    }

    public override void _PhysicsProcess(double delta)
	{
        if (Input.IsActionPressed("reset"))
        {
            GetTree().ReloadCurrentScene();
        }

        drillableGrid.GetSurroundingDrillables(playerCharacter.Position, playerCharacter.SurroundingDrillables);

        energyBar.SetValue(playerCharacter.Energy);
        healthBar.SetValue(playerCharacter.Health);

        if (playerCharacter.Energy <= 10.0f)
        {
            energyBarAnimator.Play("low_energy");
        }
        else
        {
            energyBarAnimator.Stop();
        }
    }

    public string GetMoneyString()
    {
        return $"Credits: {Money}";
    } 

    public string GetInventoryString()
    {
        StringBuilder inventoryStringBuilder = new StringBuilder();
        inventoryStringBuilder.Append($"Inventory: ({inventory.GetCount()}/{inventory.Capacity})\n");
        foreach (KeyValuePair<DrillableType, int> kvp in inventory.inventory)
        {
            if (kvp.Key != DrillableType.DIRT)
            {
                inventoryStringBuilder.Append($"    {kvp.Key.ToString().ToLower().Capitalize()}: {kvp.Value}\n");
            }
        }
        return inventoryStringBuilder.ToString();
    }

    public void _on_sell_all()
    {
        Dictionary<DrillableType, int> itemPrices = new Dictionary<DrillableType, int>
        {
            { DrillableType.IRON, 1 },
            { DrillableType.GOLD, 5 },
            { DrillableType.SILVER, 3 }
        };
        Money += inventory.SellAll(itemPrices);
        inventoryLabel.Text = GetInventoryString();
        moneyLabel.Text = GetMoneyString();
    }

    public void _on_charge()
    {
        playerCharacter.Energy = 100.0f;
    }

    public void _on_drillable_dug(Drillable drillable)
    {
        inventory.AddToInventory(drillable.DrillableType);
        inventoryLabel.Text = GetInventoryString();
    }
}
