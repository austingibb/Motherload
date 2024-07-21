using System;
using Godot;
using Godot.NativeInterop;

public class IdleSpiderStateProcessor : SpiderStateProcessor
{

    public enum IdleSpiderState
    {
        TURN_LEFT,
        TURN_RIGHT,
        WAIT,
        MOVE,
        NONE
    }

    public IdleSpiderState idleSpiderState = IdleSpiderState.NONE;

    public double ActionDuration = 0;
    public ulong ActionStartTime = 0;

    public IdleSpiderStateProcessor(Spider spider) : base(spider) {}

    public override void SetupState(StateTransition transition) {}

    public override StateTransition ProcessState(double delta)
    {
        if (spider.GlobalPosition.DistanceTo(spider.player.GlobalPosition) < 150)
        {
            return new StateTransition { ToState = SpiderState.FOLLOW, TransitionData = null };
        }

        float timeElapsed = (Time.GetTicksUsec() - ActionStartTime) / Mathf.Pow(10, 6);

        if (timeElapsed > ActionDuration)
        {
            int action;
            spider.Velocity = Vector2.Zero;
            if (idleSpiderState == IdleSpiderState.TURN_LEFT || idleSpiderState == IdleSpiderState.TURN_RIGHT)
            {
                action = GD.RandRange(2, 3);
            }
            else
            {
                action = GD.RandRange(0, 3);
            }

            if (spider.GlobalPosition.Y < 100.0f)
            {
                if (spider.Rotation > - Mathf.Pi/2 && spider.Rotation < Mathf.Pi/2) 
                {
                    if (action == 3 || action == 2)
                    {
                        action = GD.RandRange(0, 1);
                    }
                } else {
                    action = 3;
                }
            }

            switch (action)
            {
                case 0:
                    idleSpiderState = IdleSpiderState.TURN_LEFT;
                    ActionDuration = GD.RandRange(0.2f, 1.0f);
                    ActionStartTime = Time.GetTicksUsec();
                    break;
                case 1:
                    idleSpiderState = IdleSpiderState.TURN_RIGHT;
                    ActionDuration = GD.RandRange(0.2f, 1.0f);
                    ActionStartTime = Time.GetTicksUsec();
                    break;
                case 2:
                    idleSpiderState = IdleSpiderState.WAIT;
                    ActionDuration = GD.RandRange(0.5f, 2.0f);
                    ActionStartTime = Time.GetTicksUsec();
                    break;
                case 3:
                    idleSpiderState = IdleSpiderState.MOVE;
                    ActionDuration = GD.RandRange(0.5f, 2.0f);
                    ActionStartTime = Time.GetTicksUsec();
                    break;
            }
        }

        switch(idleSpiderState)
        {
            case IdleSpiderState.TURN_LEFT:
                spider.Rotation -= (float) (spider.OscillationSpeed * delta);
                break;
            case IdleSpiderState.TURN_RIGHT:
                spider.Rotation += (float) (spider.OscillationSpeed * delta);
                break;
            case IdleSpiderState.WAIT:
                break;
            case IdleSpiderState.MOVE:
                Vector2 velocity = new Vector2(Mathf.Cos(spider.Rotation - Mathf.Pi/2), Mathf.Sin(spider.Rotation - Mathf.Pi/2)) * (float)spider.Speed * (float)delta;
                spider.Velocity = velocity;
                break;
        }

        return new StateTransition { ToState = SpiderState.NONE, TransitionData = null };
    }

    public override void AnimationFinished(string animationName) {}
}