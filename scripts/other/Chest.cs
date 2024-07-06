using Godot;
using System;

public enum ChestType
{
    Red,
    Green,
    Gold,
    Silver,
    Brown
}

public partial class Chest : CharacterBody2D
{
    // nodes
    public Sprite2D closedSprite;
    public Sprite2D openSprite;
    public CollisionShape2D closedCollisionShape2D;
    public CollisionShape2D openCollisionShape2D;
    public Label purchasePrompt;

    // member variables
    [Export]
    public int Cost;
    [Export]
    public ChestType chestType;
    public bool isPlayerNearby = false;

    // signals
    MoneyAuthorization moneyAuthorization;
    [Signal]
    public delegate void chestOpenedEventHandler(int chestType);

    public override void _Ready()
    {
        closedSprite = GetNode<Sprite2D>("closed_Sprite2D");
        openSprite = GetNode<Sprite2D>("open_Sprite2D");
        closedCollisionShape2D = GetNode<CollisionShape2D>("closed_CollisionShape2D");
        openCollisionShape2D = GetNode<CollisionShape2D>("open_CollisionShape2D");
        purchasePrompt = GetNode<Label>("purchasePrompt");
        purchasePrompt.Visible = false;
        openSprite.Visible = false;
        openCollisionShape2D.Disabled = true;

        purchasePrompt.Text = "Purchase for " + Cost + " credits (e)";
    }

    public override void _PhysicsProcess(double delta)
    {
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
                EmitSignal(SignalName.chestOpened, (int)chestType);
                closedSprite.Visible = false;
                closedCollisionShape2D.Disabled = true;
                openSprite.Visible = true;
                openCollisionShape2D.Disabled = false;
            }
        }
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
}
