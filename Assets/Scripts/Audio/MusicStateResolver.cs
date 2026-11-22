using System.Collections.Generic;

public enum MusicState { Silent, Menu, Explore, Danger }

public static class MusicStateResolver
{
    // Menu wins over danger so pausing near a turret still switches to menu music.
    public static MusicState Resolve(bool menuOpen, bool dangerNearby)
    {
        if (menuOpen)
        {
            return MusicState.Menu;
        }
        return dangerNearby ? MusicState.Danger : MusicState.Explore;
    }
}

// Tracks which sources (turrets, enemies) currently see the player, so one source leaving never clears another's danger.
public class DangerTracker
{
    private readonly HashSet<int> sources = new HashSet<int>();

    public bool Active => sources.Count > 0;

    public void Set(int sourceId, bool nearby)
    {
        if (nearby) sources.Add(sourceId);
        else sources.Remove(sourceId);
    }

    public void Clear()
    {
        sources.Clear();
    }
}
