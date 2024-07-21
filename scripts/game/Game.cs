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
    public Sky sky;

    // Buildings
    public SellStation sellStation;
    public ChargeStation chargeStation;
    public UpgradeStation upgradeStation;

    // Other
    public TimeManager timeManager;

    // UI
    public UpgradeTab drillUpgradeTab;
    public UpgradeTab batteryUpgradeTab;

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
        drillUpgradeTab = GetNode<UpgradeTab>("%UpgradeMenu/PanelContainer/TabContainer/DrillUpgrades") as UpgradeTab;
        drillUpgradeTab.RegisterMoneyAuthorization(CanAfford);
        drillUpgradeTab.upgrade += DrillUpgradePurchased;

        batteryUpgradeTab = GetNode<UpgradeTab>("%UpgradeMenu/PanelContainer/TabContainer/BatteryUpgrades") as UpgradeTab;
        batteryUpgradeTab.RegisterMoneyAuthorization(CanAfford);
        batteryUpgradeTab.upgrade += BatteryUpgradePurchased;

        sky = GetNode<Sky>("%Sky") as Sky;

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

        // load other
        timeManager = new TimeManager();
        timeManager.Start();
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

        timeManager.UpdateTime(delta);
        sky.UpdateSky(timeManager.GameTime);
    }

    public string GetMoneyString()
    {
        return $"Credits: {Money}";
    } 

    public string GetInventoryString()
    {
        StringBuilder inventoryStringBuilder = new StringBuilder();
        int inventoryValue = inventory.GetValue(GameGridConstants.itemPrices);
        inventoryStringBuilder.Append($"Inventory:\nCount: {inventory.GetCount()}\nValue: {inventoryValue}\n");
        foreach (KeyValuePair<DrillableType, int> kvp in inventory.inventory)
        {
            if (kvp.Key != DrillableType.DIRT)
            {
                inventoryStringBuilder.Append($"    {kvp.Key.ToString().ToLower().Capitalize()}: {kvp.Value}\n");
            }
        }
        return inventoryStringBuilder.ToString();
    }

    public void DrillUpgradePurchased(int upgradeIndex)
    {
        Upgrade upgrade = UpgradeConstants.drillUpgrades[upgradeIndex];
        Money -= upgrade.cost;
        moneyLabel.Text = GetMoneyString();

        if (upgrade.upgradeType == UpgradeType.Drill)
        {
            DrillUpgrade drillUpgrade = (DrillUpgrade)upgrade;
            playerCharacter.HandleDrillUpgrade(drillUpgrade);
        }
    }

    public void BatteryUpgradePurchased(int upgradeIndex)
    {
        Upgrade upgrade = UpgradeConstants.batteryUpgrades[upgradeIndex];
        Money -= upgrade.cost;
        moneyLabel.Text = GetMoneyString();

        if (upgrade.upgradeType == UpgradeType.Battery)
        {
            BatteryUpgrade batteryUpgrade = (BatteryUpgrade)upgrade;
            playerCharacter.HandleBatteryUpgrade(batteryUpgrade);
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
        playerCharacter.Energy = playerCharacter.MaxEnergy;
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

    public void _on_item_spawned(ChunkItemType chunkItemType, Node2D item)
    {
        if (chunkItemType == ChunkItemType.Chest)
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
