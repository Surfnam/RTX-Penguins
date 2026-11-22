using System;
using UnityEditor;
using UnityEngine;

// Run from Tools > Audio Self Check, or in batch mode with -executeMethod AudioSelfCheck.Run
public static class AudioSelfCheck
{
    [MenuItem("Tools/Audio Self Check")]
    public static void Run()
    {
        Check(MusicStateResolver.Resolve(true, true) == MusicState.Menu, "menu wins over danger");
        Check(MusicStateResolver.Resolve(false, true) == MusicState.Danger, "danger when not in menu");
        Check(MusicStateResolver.Resolve(false, false) == MusicState.Explore, "explore by default");

        DangerTracker tracker = new DangerTracker();
        tracker.Set(1, true);
        tracker.Set(2, true);
        tracker.Set(1, false);
        Check(tracker.Active, "one source leaving keeps danger on");
        tracker.Set(2, false);
        Check(!tracker.Active, "all sources gone clears danger");

        // No AudioManager exists outside Play mode, so every static call must be a safe no-op.
        AudioManager.PlaySfx("does_not_exist");
        AudioManager.PlayStinger("does_not_exist");
        AudioManager.SetDanger(null, true);
        AudioManager.SetMenuOpen(true);
        Check(AudioManager.CurrentState == MusicState.Silent, "no manager means silent");

        Check(Resources.Load<AudioClip>("Audio/SFX/coin") != null, "coin clip is present");

        Debug.Log("AUDIO SELF CHECK PASSED");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Audio self check failed: " + message);
        }
    }
}
