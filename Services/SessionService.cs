using Microsoft.Maui.Storage;

namespace Doodle.Services;

/// <summary>
/// Service for managing app session state (unlocked/locked).
/// </summary>
public class SessionService
{
    private const string IsUnlockedKey = "session_is_unlocked";
    private const string UnlockTimestampKey = "session_unlock_timestamp";
    private const int SessionTimeoutMinutes = 30; // Session expires after 30 minutes of inactivity

    /// <summary>
    /// Checks if the app is currently unlocked.
    /// </summary>
    /// <returns>True if unlocked and session is still valid, false otherwise.</returns>
    public bool IsUnlocked()
    {
        if (!Preferences.Get(IsUnlockedKey, false))
        {
            return false;
        }

        // Check if session has expired
        var unlockTimestamp = Preferences.Get(UnlockTimestampKey, DateTime.MinValue);
        if (unlockTimestamp == DateTime.MinValue)
        {
            return false;
        }

        var timeSinceUnlock = DateTime.Now - unlockTimestamp;
        if (timeSinceUnlock.TotalMinutes > SessionTimeoutMinutes)
        {
            // Session expired
            SetLocked();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sets the app as unlocked.
    /// </summary>
    public void SetUnlocked()
    {
        Preferences.Set(IsUnlockedKey, true);
        Preferences.Set(UnlockTimestampKey, DateTime.Now);
    }

    /// <summary>
    /// Sets the app as locked.
    /// </summary>
    public void SetLocked()
    {
        Preferences.Set(IsUnlockedKey, false);
        Preferences.Remove(UnlockTimestampKey);
    }

    /// <summary>
    /// Updates the unlock timestamp to prevent session expiration.
    /// </summary>
    public void RefreshSession()
    {
        if (IsUnlocked())
        {
            Preferences.Set(UnlockTimestampKey, DateTime.Now);
        }
    }
}
