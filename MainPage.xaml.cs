using Microsoft.Extensions.Logging;

namespace Doodle
{
    public partial class MainPage : ContentPage
    {
        private readonly ILogger<MainPage>? _logger;

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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
#if DEBUG
            System.Diagnostics.Debug.WriteLine("MainPage: OnAppearing called");
            
            try
            {
                if (blazorWebView != null)
                {
                    System.Diagnostics.Debug.WriteLine("MainPage: BlazorWebView is ready");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainPage: Error in OnAppearing: {ex.Message}");
            }
#endif
        }
    }
}
