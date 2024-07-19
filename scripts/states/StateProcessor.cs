using System;
using Godot;

public abstract class StateProcessor<T>
{

    public struct StateTransition
    {
        public T ToState;
        public T FromState;
        public Object TransitionData;
    }

    public abstract void SetupState(StateTransition transitionData);
    public abstract StateTransition ProcessState(double delta);
    public abstract void AnimationFinished(string animationName);
}