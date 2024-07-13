using Godot;
using System;

public enum ChestType
{
    Red,
    Green,
    Gold,
    Silver,
    Brown,
    None
}

public partial class Chest : CharacterBody2D, ChunkItem
{
    // nodes
    public Sprite2D closedSprite;
    public Sprite2D openSprite;
    public CollisionShape2D closedCollisionShape2D;
    public CollisionShape2D openCollisionShape2D;
    public Label purchasePrompt;
    public Timer timer;

    // member variables
    [Export]
    public int Cost;
    [Export]
    public ChestType chestType;
    public bool isPlayerNearby = false;
    public bool isDisabled = false;

    // signals
    MoneyAuthorization moneyAuthorization;

    public ChunkItemType gameGridItemType => ChunkItemType.Chest;

    [Signal]
    public delegate void chestOpenedEventHandler(Chest chest);

    public override void _Ready()
    {
        closedSprite = GetNode<Sprite2D>("closed_Sprite2D");
        openSprite = GetNode<Sprite2D>("open_Sprite2D");
        closedCollisionShape2D = GetNode<CollisionShape2D>("closed_CollisionShape2D");
        openCollisionShape2D = GetNode<CollisionShape2D>("open_CollisionShape2D");
        purchasePrompt = GetNode<Label>("purchasePrompt");
        timer = GetNode<Timer>("Timer");
        purchasePrompt.Visible = false;
        openSprite.Visible = false;
        openCollisionShape2D.Disabled = true;

        purchasePrompt.Text = "Purchase for " + Cost + " credits (e)";
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isDisabled)
        {
            return;
        }

        Vector2 velocity = Velocity;
        if (!IsOnFloor())
        {
            velocity.Y += Game.GRAVITY * (float)delta;
        }
        Velocity = velocity;
        MoveAndSlide();

        if (isPlayerNearby && Input.IsActionJustPressed("interact"))
        {
            if (moneyAuthorization == null)
            {
                GD.PrintErr("Chest money authorization delegate not set.");
                return;
            }

            if (moneyAuthorization(Cost))
            {
                ChestOpened();
            }
        }
    }

    private void ChestOpened()
    {
        EmitSignal(SignalName.chestOpened, this);
        closedSprite.Visible = false;
        closedCollisionShape2D.Disabled = true;
        openSprite.Visible = true;
        openCollisionShape2D.Disabled = false;
        timer.Start();
    }

    public void _on_timer_timeout()
    {
        QueueFree();
    }

    public void _on_body_entered(Node2D body)
    {
        if (body.Name == "Player")
        {
            isPlayerNearby = true;
            purchasePrompt.Visible = true;
        }
    }

    public void _on_body_exited(Node2D body)
    {
        if (body.Name == "Player")
        {
            isPlayerNearby = false;
            purchasePrompt.Visible = false;
        }
    }

    public void RegisterMoneyAuthorization(MoneyAuthorization moneyAuthorization)
    {
        this.moneyAuthorization = moneyAuthorization;
    }

    public void Disable()
    {
        this.Visible = false;
        this.isDisabled = true;
        this.openCollisionShape2D.Disabled = true;
        this.closedCollisionShape2D.Disabled = true;
    }

    public void Enable()
    {
        this.Visible = true;
        this.isDisabled = false;
        this.openCollisionShape2D.Disabled = false;
        this.closedCollisionShape2D.Disabled = false;
    }

    public Vector2 GetPosition()
    {
        return this.GlobalPosition;
    }
}
