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
    public Label depthLabel;
    public ProgressBarNinePatch energyBar;
    public AnimationPlayer energyBarAnimator;
    public ProgressBarNinePatch healthBar;

    // Buildings
    public SellStation sellStation;
    public ChargeStation chargeStation;
    public UpgradeStation upgradeStation;

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
        depthLabel = GetNode<Label>("%HUD/Depth/DepthLabel");
        energyBar = GetNode<ProgressBarNinePatch>("%HUD/ProgressBars//Energy") as ProgressBarNinePatch;
        energyBarAnimator = GetNode<AnimationPlayer>("%HUD/ProgressBars/Energy/AnimationPlayer");
        healthBar = GetNode<ProgressBarNinePatch>("%HUD/ProgressBars/Health") as ProgressBarNinePatch;

        Money = 0;

        inventoryLabel.Text = GetInventoryString();
        moneyLabel.Text = GetMoneyString();

        // load buildings
        sellStation = GetNode<StaticBody2D>("%SellStation") as SellStation;
        sellStation.sellAll += _on_sell_all;
        chargeStation = GetNode<StaticBody2D>("%ChargeStation") as ChargeStation;
        chargeStation.charge += _on_charge;
        upgradeStation = GetNode<StaticBody2D>("%UpgradeStation") as UpgradeStation;
        upgradeStation.upgrade += _on_upgrade;

        List<Node2D> buildings = new List<Node2D>
        {
            sellStation,
            chargeStation,
            upgradeStation
        };
        drillableGrid.Init(buildings);

        int tileWidth = drillableGrid.TileSet.TileSize.X;
        Camera2D camera = GetNode<Camera2D>("%Camera2D");
        camera.LimitLeft = -tileWidth* drillableGrid.Width / 2 + tileWidth/2;
        camera.LimitRight = tileWidth* drillableGrid.Width / 2 - tileWidth/2;

        StaticBody2D leftBoundary = GetNode<StaticBody2D>("%LeftBoundary");
        leftBoundary.Position = new Vector2(-tileWidth * drillableGrid.Width / 2 + tileWidth/2, 0);
        StaticBody2D rightBoundary = GetNode<StaticBody2D>("%RightBoundary");
        rightBoundary.Position = new Vector2(tileWidth* drillableGrid.Width / 2 - tileWidth/2, 0);
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
        depthLabel.Text = $"Depth: {drillableGrid.GetDepth(playerCharacter)}";

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
        Money += inventory.SellAll(DrillableConstants.itemPrices);
        inventoryLabel.Text = GetInventoryString();
        moneyLabel.Text = GetMoneyString();
    }

    public void _on_charge()
    {
        playerCharacter.Energy = 100.0f;
    }

    public void _on_upgrade()
    {
        playerCharacter.Health = 100.0f;
    }

    public void _on_drillable_dug(Drillable drillable)
    {
        inventory.AddToInventory(drillable.DrillableType);
        inventoryLabel.Text = GetInventoryString();
    }
}
