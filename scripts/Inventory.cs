using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Node2D
{
    public Dictionary<DrillableType, int> inventory = new();
    public uint Capacity;
    private uint Count;

    public override void _Ready()
    {
        Capacity = 10;
        Count = 0;
        foreach (DrillableType drillableType in Enum.GetValues(typeof(DrillableType)))
        {
            if (drillableType == DrillableType.DIRT)
            {
                continue;
            }

            inventory.Add(drillableType, 0);
        }
    }

    public bool AddToInventory(DrillableType drillableType)
    {
        if (drillableType == DrillableType.DIRT)
        {
            return false;
        }

        if(Count < Capacity)
        {
            inventory[drillableType] += 1;
            Count += 1;
            return true;
        }
        else
        {
            return false;
        }
    }

    public int SellAll(Dictionary<DrillableType, int> itemPrices)
    {
        int total = 0;
        foreach (KeyValuePair<DrillableType, int> kvp in inventory)
        {
            total += kvp.Value * itemPrices[kvp.Key];
            inventory[kvp.Key] = 0;
        }
        Count = 0;
        return total;
    }

    public uint GetCount()
    {
        return Count;
    }
}