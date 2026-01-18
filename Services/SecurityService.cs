using Microsoft.Maui.Storage;
using System.Security.Cryptography;
using System.Text;

namespace Doodle.Services;

/// <summary>
/// Service for managing app security (PIN/password protection).
/// </summary>
public class SecurityService
{
    private const string PinHashKey = "app_pin_hash";
    private const string SecurityEnabledKey = "app_security_enabled";
    private const string FailedAttemptsKey = "security_failed_attempts";
    private const string LockoutUntilKey = "security_lockout_until";
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    /// <summary>
    /// Checks if security (PIN/password) is enabled.
    /// </summary>
    /// <returns>True if security is enabled, false otherwise.</returns>
    public bool IsSecurityEnabled()
    {
        return Preferences.Get(SecurityEnabledKey, false);
    }

    /// <summary>
    /// Sets up PIN/password protection.
    /// </summary>
    /// <param name="pin">The PIN or password to set.</param>
    /// <returns>True if setup was successful, false otherwise.</returns>
    public bool SetupSecurity(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4)
        {
            return false;
        }

        var hash = HashPin(pin);
        Preferences.Set(PinHashKey, hash);
        Preferences.Set(SecurityEnabledKey, true);
        return true;
    }

    /// <summary>
    /// Verifies the provided PIN/password.
    /// </summary>
    /// <param name="pin">The PIN or password to verify.</param>
    /// <returns>True if the PIN is correct, false otherwise.</returns>
    public bool VerifyPin(string pin)
    {
        if (!IsSecurityEnabled())
        {
            return true; // No security enabled, always allow
        }

        if (string.IsNullOrWhiteSpace(pin))
        {
            return false;
        }

        // Check if locked out
        if (IsLockedOut())
        {
            return false;
        }

        var storedHash = Preferences.Get(PinHashKey, string.Empty);
        if (string.IsNullOrEmpty(storedHash))
        {
            return false;
        }

        var inputHash = HashPin(pin);
        bool isValid = storedHash == inputHash;

        if (isValid)
        {
            // Reset failed attempts on successful login
            ResetFailedAttempts();
        }
        else
        {
            // Increment failed attempts
            IncrementFailedAttempts();
        }

        return isValid;
    }

    /// <summary>
    /// Gets the number of failed attempts.
    /// </summary>
    public int GetFailedAttempts()
    {
        return Preferences.Get(FailedAttemptsKey, 0);
    }

    /// <summary>
    /// Gets the remaining attempts before lockout.
    /// </summary>
    public int GetRemainingAttempts()
    {
        return Math.Max(0, MaxFailedAttempts - GetFailedAttempts());
    }

    /// <summary>
    /// Checks if the account is currently locked out.
    /// </summary>
    public bool IsLockedOut()
    {
        var lockoutUntil = Preferences.Get(LockoutUntilKey, DateTime.MinValue);
        if (lockoutUntil == DateTime.MinValue)
        {
            return false;
        }

        if (DateTime.Now < lockoutUntil)
        {
            return true;
        }

        // Lockout expired, clear it
        Preferences.Remove(LockoutUntilKey);
        ResetFailedAttempts();
        return false;
    }

    /// <summary>
    /// Gets the lockout expiration time.
    /// </summary>
    public DateTime? GetLockoutUntil()
    {
        var lockoutUntil = Preferences.Get(LockoutUntilKey, DateTime.MinValue);
        if (lockoutUntil == DateTime.MinValue)
        {
            return null;
        }

        return lockoutUntil > DateTime.Now ? lockoutUntil : null;
    }

    /// <summary>
    /// Increments failed attempts and locks out if threshold reached.
    /// </summary>
    private void IncrementFailedAttempts()
    {
        var attempts = GetFailedAttempts() + 1;
        Preferences.Set(FailedAttemptsKey, attempts);

        if (attempts >= MaxFailedAttempts)
        {
            var lockoutUntil = DateTime.Now.AddMinutes(LockoutMinutes);
            Preferences.Set(LockoutUntilKey, lockoutUntil);
            System.Diagnostics.Debug.WriteLine($"Account locked out until {lockoutUntil}");
        }
    }

    /// <summary>
    /// Resets failed attempts counter.
    /// </summary>
    private void ResetFailedAttempts()
    {
        Preferences.Remove(FailedAttemptsKey);
        Preferences.Remove(LockoutUntilKey);
    }

    /// <summary>
    /// Disables security protection.
    /// </summary>
    public void DisableSecurity()
    {
        Preferences.Remove(PinHashKey);
        Preferences.Set(SecurityEnabledKey, false);
        ResetFailedAttempts(); // Clear any lockout state
    }

    /// <summary>
    /// Resets security (clears PIN and all security data).
    /// WARNING: This will permanently delete all security settings.
    /// </summary>
    public void ResetSecurity()
    {
        Preferences.Remove(PinHashKey);
        Preferences.Remove(SecurityEnabledKey);
        Preferences.Remove(FailedAttemptsKey);
        Preferences.Remove(LockoutUntilKey);
    }

    /// <summary>
    /// Changes the PIN/password.
    /// </summary>
    /// <param name="oldPin">The current PIN.</param>
    /// <param name="newPin">The new PIN.</param>
    /// <returns>True if the change was successful, false otherwise.</returns>
    public bool ChangePin(string oldPin, string newPin)
    {
        if (!VerifyPin(oldPin))
        {
            return false;
        }

        return SetupSecurity(newPin);
    }

    /// <summary>
    /// Hashes a PIN using SHA256.
    /// </summary>
    /// <param name="pin">The PIN to hash.</param>
    /// <returns>The hashed PIN as a hex string.</returns>
    private string HashPin(string pin)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(pin);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
