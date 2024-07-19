using System.Collections.Generic;
using Godot;

public enum PlayerState {
	Grounded,
	Airborne,
	Drilling,
	Dead,
	None
}

public class PlayerStateManager : StateManager<PlayerState>
{
    public PlayerCharacterBody2D player;

    public PlayerStateManager(PlayerCharacterBody2D player) : base(PlayerState.Airborne, PlayerState.None)
    {
        this.player = player;
        stateProcessors.Add(PlayerState.Airborne, new AirbornePlayerStateProcessor(player));
        stateProcessors.Add(PlayerState.Grounded, new GroundedPlayerStateProcessor(player));
        stateProcessors.Add(PlayerState.Drilling, new DrillingPlayerStateProcessor(player));
        stateProcessors.Add(PlayerState.Dead, new DeadPlayerStateProcessor(player));
    }

    public bool CanShoot()
    {
        return (currentState == PlayerState.Airborne || currentState == PlayerState.Grounded);
    }
}