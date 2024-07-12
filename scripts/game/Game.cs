using Godot;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node2D
{
    public static float GRAVITY = 500f;

    // Nodes
    public PlayerCharacterBody2D playerCharacter;
    public GameGrid gameGrid;
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

    // UI
    public UpgradeMenu upgradeMenu;

    // Member variables
    public int Money;

    public int ChunkRows = 3;

    public override void _Ready()
    {
        playerCharacter = GetNode<CharacterBody2D>("%Player") as PlayerCharacterBody2D;
        gameGrid = GetNode<TileMap>("%GameGrid") as GameGrid;
        gameGrid.drillableDug += _on_drillable_dug;
        gameGrid.itemSpawned += _on_item_spawned;
        inventory = GetNode<Inventory>("%Inventory") as Inventory;
        hud = GetNode<CanvasLayer>("%HUD") as CanvasLayer;
        hud.Visible = true;
        inventoryLabel = GetNode<Label>("%HUD/InventoryLabel");
        moneyLabel = GetNode<Label>("%HUD/MoneyLabel");
        depthLabel = GetNode<Label>("%HUD/DepthLabel");
        energyBar = GetNode<ProgressBarNinePatch>("%HUD/ProgressBars//Energy") as ProgressBarNinePatch;
        energyBarAnimator = GetNode<AnimationPlayer>("%HUD/ProgressBars/Energy/AnimationPlayer");
        healthBar = GetNode<ProgressBarNinePatch>("%HUD/ProgressBars/Health") as ProgressBarNinePatch;
        upgradeMenu = GetNode<UpgradeMenu>("%UpgradeMenu") as UpgradeMenu;
        upgradeMenu.RegisterMoneyAuthorization(CanAfford);
        upgradeMenu.upgrade += UpgradePurchased;

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
        gameGrid.SetBuildings(buildings);

        for (int i = 0; i < gameGrid.ChunkWidth; i++)
        {
            for (int j = 0; j < ChunkRows; j++)
            {
                gameGrid.SpawnChunk(new Vector2I(i, j));
            }
        }
        
        int tileWidth = gameGrid.TileSet.TileSize.X;
        Camera2D camera = GetNode<Camera2D>("%Camera2D");
        camera.LimitLeft = -tileWidth* gameGrid.Width / 2 + tileWidth/2;
        camera.LimitRight = tileWidth* gameGrid.Width / 2 - tileWidth/2;

        StaticBody2D leftBoundary = GetNode<StaticBody2D>("%LeftBoundary");
        leftBoundary.Position = new Vector2(-tileWidth * gameGrid.Width / 2 + tileWidth/2, 0);
        StaticBody2D rightBoundary = GetNode<StaticBody2D>("%RightBoundary");
        rightBoundary.Position = new Vector2(tileWidth* gameGrid.Width / 2 - tileWidth/2, 0);
    }

    public override void _Process(double delta)
	{
        if (Input.IsActionPressed("reset"))
        {
            GetTree().ReloadCurrentScene();
        }

        if (Input.IsActionJustPressed("test1"))
        {
            Money += 1000;
            moneyLabel.Text = GetMoneyString();
        } else if (Input.IsActionJustPressed("test2"))
        {
            Money += 10_000;
            moneyLabel.Text = GetMoneyString();
        } else if (Input.IsActionJustPressed("test3"))
        {
            Money += 100_000;
            moneyLabel.Text = GetMoneyString();
        }

        gameGrid.GetSurroundingDrillables(playerCharacter.Position, playerCharacter.SurroundingDrillables);

        energyBar.SetValue(playerCharacter.Energy);
        healthBar.SetValue(playerCharacter.Health);
        depthLabel.Text = $"Depth: {gameGrid.GetDepth(playerCharacter)}";

        if (playerCharacter.Energy <= 10.0f)
        {
            energyBarAnimator.Play("low_energy");
        }
        else
        {
            energyBarAnimator.Stop();
        }

        gameGrid.Update(playerCharacter.GlobalPosition);
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

    public void UpgradePurchased(int upgradeIndex)
    {
        Upgrade upgrade = UpgradeConstants.upgrades[upgradeIndex];
        Money -= upgrade.cost;
        moneyLabel.Text = GetMoneyString();

        if (upgrade.upgradeType == UpgradeType.Drill)
        {
            DrillUpgrade drillUpgrade = (DrillUpgrade)upgrade;
            playerCharacter.HandleDrillUpgrade(drillUpgrade);
        }
    }

    public bool CanAfford(int cost)
    {
        return Money >= cost;
    }

    public void _on_sell_all()
    {
        Money += inventory.SellAll(GameGridConstants.itemPrices);
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

    public void _on_item_spawned(GameGridItemType gameGridItemType, Node2D item)
    {
        if (gameGridItemType == GameGridItemType.Chest)
        {
            Chest chest = (Chest) item;
            chest.RegisterMoneyAuthorization(CanAfford);
            chest.chestOpened += _on_chest_opened;
        }
    }

    public void _on_chest_opened(Chest chest)
    {
        Money -= chest.Cost;
        moneyLabel.Text = GetMoneyString();
    }
}
