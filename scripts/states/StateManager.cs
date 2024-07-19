using System;
using System.Collections.Generic;
using Godot;

public class StateManager<T> where T : IComparable
{
    public T currentState;
    public T emptyState;
    public Dictionary<T, StateProcessor<T>> stateProcessors = new Dictionary<T, StateProcessor<T>>();

    public StateManager(T startState, T emptyState)
    {
        this.currentState = startState;
        this.emptyState = emptyState;
    }

    public void Update(double delta)
    {
        StateProcessor<T>.StateTransition stateTransition = stateProcessors[currentState].ProcessState(delta);
        if (stateTransition.ToState.CompareTo(currentState) != 0 && stateTransition.ToState.CompareTo(emptyState) != 0)
        {
            GD.Print("Transitioning from " + currentState + " to " + stateTransition.ToState);
            stateTransition.FromState = currentState;
            currentState = stateTransition.ToState;
            stateProcessors[currentState].SetupState(stateTransition);
        }
    }

    public void AnimationFinished(string animationName)
    {
        stateProcessors[currentState].AnimationFinished(animationName);
    }
}