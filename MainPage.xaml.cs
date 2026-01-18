using Doodle.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Doodle
{
    public partial class MainPage : ContentPage
    {
        private SecurityService? _securityService;

        public MainPage()
        {
            try
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("MainPage: Starting initialization");
#endif
                
                InitializeComponent();
                
#if DEBUG
                System.Diagnostics.Debug.WriteLine("MainPage: InitializeComponent completed");
                
                // Verify BlazorWebView is initialized
                if (blazorWebView != null)
                {
                    System.Diagnostics.Debug.WriteLine($"MainPage: BlazorWebView found - HostPage: {blazorWebView.HostPage}");
                    System.Diagnostics.Debug.WriteLine($"MainPage: BlazorWebView RootComponents count: {blazorWebView.RootComponents?.Count ?? 0}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("MainPage: WARNING - BlazorWebView is null!");
                }
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"MainPage: ERROR during initialization: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"MainPage: Exception type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"MainPage: Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"MainPage: Inner exception: {ex.InnerException.Message}");
                }
#endif
                throw;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
#if DEBUG
            System.Diagnostics.Debug.WriteLine("MainPage: OnAppearing called");
#endif
            
            try
            {
                // Check security on app appearing
                await CheckSecurityAsync();
                
#if DEBUG
                if (blazorWebView != null)
                {
                    System.Diagnostics.Debug.WriteLine("MainPage: BlazorWebView is ready");
                }
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"MainPage: Error in OnAppearing: {ex.Message}");
#endif
            }
        }

        private Task CheckSecurityAsync()
        {
            try
            {
                // Get SecurityService from DI
                _securityService = Handler?.MauiContext?.Services?.GetService<SecurityService>();
                
                if (_securityService != null && _securityService.IsSecurityEnabled())
                {
                    System.Diagnostics.Debug.WriteLine("Security enabled, checking if we need to show lock screen");
                    // Navigation to lock screen will be handled by Blazor routing
                    // For now, we'll let the Routes component handle it
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking security: {ex.Message}");
            }
            return Task.CompletedTask;
        }
    }
}
