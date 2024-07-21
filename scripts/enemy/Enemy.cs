using Godot;

public abstract partial class Enemy : Godot.CharacterBody2D, ChunkItem
{
    public float Health;
    public float MaxHealth;
    public float Damage;

    public abstract void Disable();

    public abstract void Enable();

    public abstract Vector2 GetPosition();

    public abstract void TakeDamage(float damage);
}