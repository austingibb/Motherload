using System.Collections.Generic;
using Godot;

public enum SpiderState
{
    IDLE,
    FOLLOW,
    ATTACK,
    DEAD,
    NONE
}

public class SpiderStateManager : StateManager<SpiderState>
{
    public Spider spider;

    public SpiderStateManager(Spider spider) : base(SpiderState.IDLE, SpiderState.NONE)
    {
        this.spider = spider;
        stateProcessors.Add(SpiderState.IDLE, new IdleSpiderStateProcessor(spider));
        stateProcessors.Add(SpiderState.FOLLOW, new FollowingSpiderStateProcessor(spider));
        stateProcessors.Add(SpiderState.ATTACK, new AttackSpiderStateProcessor(spider));
        // stateProcessors.Add(SpiderState.DEAD, new DeadSpiderStateProcessor(spider));
    }
}