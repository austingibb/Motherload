using System;
using Godot;

public abstract class SpiderStateProcessor : StateProcessor<SpiderState>
{
    public Spider spider;

    public SpiderStateProcessor(Spider spider)
    {
        this.spider = spider;
    }

    public bool FacePlayer(ref float Rotation, double delta, float rotationSpeed = 5.0f)
    {
        float angleToPlayer = spider.GlobalPosition.DirectionTo(spider.player.GlobalPosition).Angle() + Mathf.Pi/2;
        float targetAngle = 0;
        if (angleToPlayer > Mathf.Pi)
        {
            angleToPlayer -= Mathf.Tau;
        }

        float smallestDifference = float.MaxValue;
        for (int i = -1; i < 2; i++)
        {
            float angle = angleToPlayer + 2*Mathf.Pi * i;
            float difference = Mathf.Abs(angle - Rotation);
            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                targetAngle = angle;
            }
        }

        Rotation = (float) Mathf.MoveToward(Rotation, targetAngle, rotationSpeed * delta);
        spider.Rotation = Rotation;

        return smallestDifference < 0.1f;
    }
}