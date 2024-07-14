using Godot;

public class TimeManager
{
    // one minute to 
    public double TimeScale = 120.0f/60.0f;
    public double GameTime = 0f;
    public ulong ActualElapsedTime;
    public ulong ActualElapsedTimeStart;

    public bool Paused = true;

    public void Start()
    {
        ActualElapsedTimeStart = Time.GetTicksUsec();
        Paused = false;
    }

    public void Pause()
    {
        Paused = true;
    }

    public void UpdateTime(double delta)
    {
        if (Paused)
            return;
        GameTime += delta * TimeScale;
        if (GameTime > 100.0f)
        { 
            GameTime = 100.0f;
        }
        ActualElapsedTime += Time.GetTicksUsec() - ActualElapsedTimeStart;
        ActualElapsedTimeStart = Time.GetTicksUsec();
        if (ActualElapsedTime / Mathf.Pow(10, 6) > 60)
        {
            ActualElapsedTime = 0;
        }
    } 
}