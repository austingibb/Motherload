using Godot;

public class PlayerDash
{
    public AnimationPlayer dashAnimationPlayer;
    public bool dashActive = false;
    public double dashDuration = 0.15f;
    public ulong dashStartTime = 0;
    public double dashCooldown = 1.0f;
    public ulong dashCooldownStartTime = 0;

    public PlayerDash(AnimationPlayer dashAnimationPlayer)
    {
        this.dashAnimationPlayer = dashAnimationPlayer;
    }

    public bool IsDashActive()
    {
        return dashActive;
    }

    public bool CanDash()
    {
        return !dashActive && CooldownPassed();
    }

    public void Dash()
    {
        dashActive = true;
        dashStartTime = Time.GetTicksUsec();
        dashAnimationPlayer.Play("dash");
    }

    public bool CooldownPassed()
    {
        return (Time.GetTicksUsec() - dashCooldownStartTime) / Mathf.Pow(10, 6) > dashCooldown;
    }

    public void Update(double delta)
    {
        if (dashActive)
        {
            if ((Time.GetTicksUsec() - dashStartTime) / Mathf.Pow(10, 6) > dashDuration)
            {
                dashActive = false;
                dashCooldownStartTime = Time.GetTicksUsec();
                dashAnimationPlayer.Play("RESET");
            }
        }
    }
}