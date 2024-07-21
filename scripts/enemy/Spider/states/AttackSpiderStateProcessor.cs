using System;
using Godot;
using Godot.NativeInterop;

public class AttackSpiderStateProcessor : SpiderStateProcessor
{
    public enum AttackSpiderState
    {
        ROTATE,
        ATTACK,
        NONE
    }

    public AttackSpiderState attackSpiderState;

    public AttackSpiderStateProcessor(Spider spider) : base(spider) {}

    public float RotationDir;
    public double ActionDuration = 0.75f;
    public ulong ActionStartTime = 0;

    public override void SetupState(StateTransition transition) 
    {
        attackSpiderState = AttackSpiderState.ROTATE;
        spider.Velocity = Vector2.Zero;
        float angleToPlayer = spider.GlobalPosition.DirectionTo(spider.player.GlobalPosition).Angle();
        float angleDiff;
        float direction;
        Common.GetAngleDiff(spider.Rotation - Mathf.Pi/2, angleToPlayer, out direction, out angleDiff);
        RotationDir = direction;
    }

    public override StateTransition ProcessState(double delta)
    {
        if (attackSpiderState == AttackSpiderState.ROTATE)
        {
            float angleToPlayer = spider.GlobalPosition.DirectionTo(spider.player.GlobalPosition).Angle();
            float angleDiff;
            float direction;
            Common.GetAngleDiff(spider.Rotation - Mathf.Pi/2, angleToPlayer, out direction, out angleDiff);
            
            spider.Rotation += (float) (spider.OscillationSpeed * delta * RotationDir);
            if (Mathf.Abs(angleDiff) < 0.1)
            {
                attackSpiderState = AttackSpiderState.ATTACK;
                ActionStartTime = Time.GetTicksUsec();
            }
        } else if (attackSpiderState == AttackSpiderState.ATTACK)
        {
            float speedModifier = (float)spider.Speed * (float)delta * 3.0f;
            Vector2 velocity = new Vector2(Mathf.Cos(spider.Rotation - Mathf.Pi/2) * speedModifier, Mathf.Sin(spider.Rotation - Mathf.Pi/2) * speedModifier);
            spider.Velocity = velocity;
            if (Time.GetTicksUsec() - ActionStartTime > ActionDuration * Mathf.Pow(10, 6))
            {
                return new StateTransition { ToState = SpiderState.FOLLOW, TransitionData = null };
            }
        }
        return new StateTransition { ToState = SpiderState.NONE, TransitionData = null };
    }

    public override void AnimationFinished(string animationName) {}
}