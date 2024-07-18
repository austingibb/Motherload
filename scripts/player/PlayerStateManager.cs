using System.Collections.Generic;
using Godot;

public class PlayerStateManager
{
    public PlayerCharacterBody2D player;
    public PlayerState currentState;
    public Dictionary<PlayerState, PlayerStateProcessor> playerStateProcessors = new Dictionary<PlayerState, PlayerStateProcessor>();

    public PlayerStateManager(PlayerCharacterBody2D player)
    {
        this.player = player;
        currentState = PlayerState.Airborne;
        playerStateProcessors.Add(PlayerState.Airborne, new AirbornePlayerStateProcessor(player));
        playerStateProcessors.Add(PlayerState.Grounded, new GroundedPlayerStateProcessor(player));
        playerStateProcessors.Add(PlayerState.Drilling, new DrillingPlayerStateProcessor(player));
        playerStateProcessors.Add(PlayerState.Dead, new DeadPlayerStateProcessor(player));
    }

    public void Update(double delta)
    {
        PlayerStateTransition stateTransition = playerStateProcessors[currentState].ProcessState(delta);
        if (stateTransition.ToState != currentState && stateTransition.ToState != PlayerState.None)
        {
            GD.Print("Transitioning from " + currentState + " to " + stateTransition.ToState);
            stateTransition.FromState = currentState;
            currentState = stateTransition.ToState;
            playerStateProcessors[currentState].SetupState(stateTransition);
        }
    }

    public void AnimationFinished(string animationName)
    {
        playerStateProcessors[currentState].AnimationFinished(animationName);
    }

    public bool CanShoot()
    {
        return (currentState == PlayerState.Airborne || currentState == PlayerState.Grounded);
    }
}