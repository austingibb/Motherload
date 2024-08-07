using System;
using Godot;
using Godot.NativeInterop;

public class FollowingSpiderStateProcessor : SpiderStateProcessor
{
    public enum FollowSpiderState
    {
        ROTATE,
        FOLLOW,
        NONE
    }

    public FollowSpiderState followSpiderState;
    public float RotationDir = 1;
    public float Rotation = 0;

    public FollowingSpiderStateProcessor(Spider spider) : base(spider) {}

    public override void SetupState(StateTransition transition) 
    {
        followSpiderState = FollowSpiderState.ROTATE;
        Rotation = spider.Rotation;
    }

    public override StateTransition ProcessState(double delta)
    {
        if (spider.GlobalPosition.DistanceTo(spider.player.GlobalPosition) > 150)
        {
            return new StateTransition { ToState = SpiderState.IDLE, TransitionData = null };
        } else if (spider.GlobalPosition.DistanceTo(spider.player.GlobalPosition) < 60 && spider.GlobalPosition.Y > 100.0f)
        {
            return new StateTransition { ToState = SpiderState.ATTACK, TransitionData = null };
        }

        if (followSpiderState == FollowSpiderState.ROTATE)
        {
            if (FacePlayer(ref Rotation, delta)) {
                followSpiderState = FollowSpiderState.FOLLOW;
            }
            spider.Velocity = new Vector2(Mathf.Cos(spider.Rotation - Mathf.Pi/2), Mathf.Sin(spider.Rotation - Mathf.Pi/2)) * (float)spider.Speed/2 * (float)delta;
        } else if (followSpiderState == FollowSpiderState.FOLLOW)
        {
            Vector2 targetPosition = spider.player.GlobalPosition;
            if (spider.GlobalPosition.Y < 100.0f)
            {
                targetPosition = new Vector2(spider.player.GlobalPosition.X, 100.0f);
            }
            float angleToPlayer = spider.GlobalPosition.DirectionTo(targetPosition).Angle();
            float angleDiff;
            float direction;
            Common.GetAngleDiff(spider.Rotation - Mathf.Pi/2, angleToPlayer, out direction, out angleDiff);
            
            if (Mathf.Abs(angleDiff) > Mathf.Pi/2.5)
            {
                RotationDir = direction;
                spider.Rotation += (float) (spider.OscillationSpeed * delta * RotationDir);
            } else 
            {
                if ((Mathf.Pi/2.5 - Math.Abs(angleDiff)) < Mathf.Pi/6) 
                {
                    spider.Rotation += (float) (spider.OscillationSpeed/2 * delta * RotationDir);
                } else 
                {
                    spider.Rotation += (float) (spider.OscillationSpeed * delta * RotationDir);
                }
            }

            float speedModifier = (float)spider.Speed * (float)delta * Mathf.Clamp(2*Mathf.Abs(angleDiff), 1.0f, 3.0f);
            Vector2 velocity = new Vector2(Mathf.Cos(spider.Rotation - Mathf.Pi/2) * speedModifier, Mathf.Sin(spider.Rotation - Mathf.Pi/2) * speedModifier);
            spider.Velocity = velocity;
        }
        return new StateTransition { ToState = SpiderState.NONE, TransitionData = null };
    }

    public override void AnimationFinished(string animationName) {}
}