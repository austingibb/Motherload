using System;
using Godot;

public abstract class SpiderStateProcessor : StateProcessor<SpiderState>
{
    public Spider spider;

    public SpiderStateProcessor(Spider spider)
    {
        this.spider = spider;
    }
}