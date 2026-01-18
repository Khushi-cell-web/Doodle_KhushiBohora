using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Doodle.Services;
using Doodle.Models;
using System.Text.Json;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using System;
using System.IO;

namespace Doodle.ViewModels;

/// <summary>
/// ViewModel for the Settings page.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ThemeService _themeService;
    private readonly SecurityService _securityService;
    private readonly JournalService _journalService;
    private readonly MoodService? _moodService;
    private readonly TagService? _tagService;

    [ObservableProperty]
    private string _selectedTheme = "auto";

    [ObservableProperty]
    private string _customDeepGreen = "#30360E";

    [ObservableProperty]
    private string _customOlive = "#787F56";

    [ObservableProperty]
    private string _customClayBeige = "#E2D4B9";

    [ObservableProperty]
    private bool _enableNotifications = true;

    [ObservableProperty]
    private bool _autoSave = true;

    [ObservableProperty]
    private string _defaultEntryTime = "20:00";

    [ObservableProperty]
    private int _totalEntries = 0;

    [ObservableProperty]
    private bool _isSecurityEnabled;

    [ObservableProperty]
    private string _currentPin = string.Empty;

    [ObservableProperty]
    private string _newPin = string.Empty;

    [ObservableProperty]
    private string _confirmPin = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string _statusMessageType = "info";

    public SettingsViewModel(ThemeService themeService, SecurityService securityService, JournalService journalService, MoodService? moodService = null, TagService? tagService = null)
    {
        _themeService = themeService;
        _securityService = securityService;
        _journalService = journalService;
        _moodService = moodService;
        _tagService = tagService;
    }

    /// <summary>
    /// Loads settings from storage.
    /// </summary>
    [RelayCommand]
    public async Task LoadSettingsAsync()
    {
        try
        {
            IsLoading = true;

            SelectedTheme = await _themeService.GetThemeAsync();
            IsSecurityEnabled = _securityService.IsSecurityEnabled();

            // Load custom theme colors
            var customColors = await _themeService.GetCustomColorsAsync();
            CustomDeepGreen = customColors.DeepGreen;
            CustomOlive = customColors.Olive;
            CustomClayBeige = customColors.ClayBeige;

            // Load statistics
            var entries = await _journalService.GetAllEntriesAsync();
            TotalEntries = entries.Count;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading settings: {ex.Message}";
            StatusMessageType = "error";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Saves settings to storage.
    /// </summary>
    [RelayCommand]
    public async Task SaveSettingsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = null;

            System.Diagnostics.Debug.WriteLine($"Saving theme: {SelectedTheme}");
            await _themeService.SetThemeAsync(SelectedTheme);
            
            // Save custom colors if custom theme is selected
            if (SelectedTheme == "custom")
            {
                await _themeService.SetCustomColorsAsync(CustomDeepGreen, CustomOlive, CustomClayBeige);
                System.Diagnostics.Debug.WriteLine($"Custom colors saved: DeepGreen={CustomDeepGreen}, Olive={CustomOlive}, ClayBeige={CustomClayBeige}");
            }
            
            System.Diagnostics.Debug.WriteLine("Theme saved and applied");

            StatusMessage = "Settings saved successfully!";
            StatusMessageType = "success";

            await Task.Delay(2000);
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            StatusMessage = $"Error saving settings: {ex.Message}";
            StatusMessageType = "error";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Sets up PIN/password security.
    /// </summary>
    [RelayCommand]
    public async Task SetupSecurityAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = null;

            if (string.IsNullOrWhiteSpace(NewPin) || NewPin.Length < 4)
            {
                StatusMessage = "PIN must be at least 4 characters long.";
                StatusMessageType = "error";
                return;
            }

            if (NewPin != ConfirmPin)
            {
                StatusMessage = "PINs do not match.";
                StatusMessageType = "error";
                return;
            }

            if (_securityService.SetupSecurity(NewPin))
            {
                IsSecurityEnabled = true;
                NewPin = string.Empty;
                ConfirmPin = string.Empty;
                StatusMessage = "Security PIN set successfully!";
                StatusMessageType = "success";
                System.Diagnostics.Debug.WriteLine("Security PIN set successfully");
            }
            else
            {
                StatusMessage = "Failed to set security PIN.";
                StatusMessageType = "error";
            }

            await Task.Delay(2000);
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error setting up security: {ex.Message}";
            StatusMessageType = "error";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Disables security protection.
    /// </summary>
    [RelayCommand]
    public async Task DisableSecurityAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = null;

            if (string.IsNullOrWhiteSpace(CurrentPin) || !_securityService.VerifyPin(CurrentPin))
            {
                StatusMessage = "Incorrect PIN. Cannot disable security.";
                StatusMessageType = "error";
                return;
            }

            _securityService.DisableSecurity();
            IsSecurityEnabled = false;
            CurrentPin = string.Empty;
            StatusMessage = "Security disabled successfully.";
            StatusMessageType = "success";
            System.Diagnostics.Debug.WriteLine("Security disabled successfully");

            await Task.Delay(2000);
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error disabling security: {ex.Message}";
            StatusMessageType = "error";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Changes the security PIN.
    /// </summary>
    [RelayCommand]
    public async Task ChangePinAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = null;

            if (string.IsNullOrWhiteSpace(CurrentPin) || !_securityService.VerifyPin(CurrentPin))
            {
                StatusMessage = "Current PIN is incorrect.";
                StatusMessageType = "error";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPin) || NewPin.Length < 4)
            {
                StatusMessage = "New PIN must be at least 4 characters long.";
                StatusMessageType = "error";
                return;
            }

            if (NewPin != ConfirmPin)
            {
                StatusMessage = "New PINs do not match.";
                StatusMessageType = "error";
                return;
            }

            if (_securityService.ChangePin(CurrentPin, NewPin))
            {
                CurrentPin = string.Empty;
                NewPin = string.Empty;
                ConfirmPin = string.Empty;
                StatusMessage = "PIN changed successfully!";
                StatusMessageType = "success";
                System.Diagnostics.Debug.WriteLine("PIN changed successfully");
            }
            else
            {
                StatusMessage = "Failed to change PIN.";
                StatusMessageType = "error";
            }

            await Task.Delay(2000);
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error changing PIN: {ex.Message}";
            StatusMessageType = "error";
        }
        finally
        {
            IsLoading = false;
        }
    }

}
