using Godot;
using System;
using System.Collections.Generic;

public class PlayerDrillables
{
    private class DrillableDictValue
    {
        public DrillFromDirection Direction;
        public double TimeActive;
        public Node2D Drillable;

        public DrillableDictValue(DrillFromDirection direction, Node2D drillable)
        {
            this.Direction = direction;
            this.Drillable = drillable;
            this.TimeActive = 0;
        }

        public void AddToDrillableTime(double delta) 
        {
            TimeActive += delta;
        }

        public void ResetDrillableTime() 
        {
            TimeActive = 0;
        }
        
    }

    public Node2D ActiveDrillable = null;
    private const double TimeToStartDrilling = 0.1;
    private Dictionary<string, DrillableDictValue> drillables = new();
    DrillFromDirection currentPendingDirection = DrillFromDirection.NONE;

    public Node2D DirectionHeld(DrillFromDirection direction, double delta)
    {
        if (currentPendingDirection != direction) {
            foreach (KeyValuePair<string, DrillableDictValue> kvp in drillables)
            {
                kvp.Value.ResetDrillableTime();
            }

            currentPendingDirection = direction;
        }

        if (currentPendingDirection == DrillFromDirection.NONE)
        {
            ActiveDrillable = null;
            return null;
        }

        foreach (KeyValuePair<string, DrillableDictValue> kvp in drillables)
        {
            DrillableDictValue drillableDictValue = kvp.Value;
            if (drillableDictValue.Direction == currentPendingDirection) 
            {
                drillableDictValue.AddToDrillableTime(delta);
            }

            if (drillableDictValue.TimeActive > TimeToStartDrilling)
            {
                currentPendingDirection = DrillFromDirection.NONE;
                ActiveDrillable = drillableDictValue.Drillable;
                return drillableDictValue.Drillable;
            }
        }

        ActiveDrillable = null;
        return null;
    }

    public void RegisterDrillable(Node2D drillable, DrillFromDirection direction)
    {
        drillables.Add(drillable.GetPath(), new(direction, drillable));
    }

	public void UnregisterDrillable(Node2D drillable) 
    {
        drillables.Remove(drillable.GetPath());
    }
}
