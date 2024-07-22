using Godot;
using System;
using System.Collections.Generic;

public class PlayerDrillables
{
    private class DrillableDictValue
    {
        public DrillFromDirection Direction;
        public double TimeActive;
        public Drillable Drillable;

        public DrillableDictValue(DrillFromDirection direction, Drillable drillable)
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

    public Drillable ActiveDrillable = null;
    private const double TimeToStartDrilling = 0.1;
    private const double AdditionalTimeToStartDrillingUp = 0.3;
    private Dictionary<DrillFromDirection, DrillableDictValue> drillables = new();
    DrillFromDirection currentPendingDirection = DrillFromDirection.NONE;

    public Node2D DirectionHeld(DrillFromDirection direction, double delta, float digUpResistance = 0)
    {
        if (currentPendingDirection != direction) {
            foreach (KeyValuePair<DrillFromDirection, DrillableDictValue> kvp in drillables)
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

        foreach (KeyValuePair<DrillFromDirection, DrillableDictValue> kvp in drillables)
        {
            DrillableDictValue drillableDictValue = kvp.Value;
            if (drillableDictValue.Direction == currentPendingDirection) 
            {
                drillableDictValue.AddToDrillableTime(delta);
            }

            double targetTime = TimeToStartDrilling;

            if (currentPendingDirection == DrillFromDirection.DOWN)
            {
                targetTime = TimeToStartDrilling + digUpResistance * AdditionalTimeToStartDrillingUp;
                targetTime = Mathf.Clamp(targetTime, TimeToStartDrilling, TimeToStartDrilling + AdditionalTimeToStartDrillingUp);
            }

            if (drillableDictValue.TimeActive > targetTime)
            {
                currentPendingDirection = DrillFromDirection.NONE;
                ActiveDrillable = drillableDictValue.Drillable;
                return drillableDictValue.Drillable;
            }
        }

        ActiveDrillable = null;
        return null;
    }

    public void RegisterDrillable(Drillable drillable, DrillFromDirection direction)
    {
        if (!drillables.ContainsKey(direction))
        {
            drillables[direction] = new DrillableDictValue(direction, drillable);
        } else {
            if (drillable.GetPath() != drillables[direction].Drillable.GetPath())
            {
                drillables[direction] = new DrillableDictValue(direction, drillable);
            }
        }
    }

	public void UnregisterDrillable(DrillFromDirection direction) 
    {
        if (drillables.ContainsKey(direction))
        {
            drillables.Remove(direction);
        }
    }
}
