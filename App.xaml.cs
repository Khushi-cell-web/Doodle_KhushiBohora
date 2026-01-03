using Microsoft.Extensions.Logging;

namespace Doodle
{
    public partial class App : Application
    {
        private readonly ILogger<App>? _logger;

        public App()
        {
            try
            {
                InitializeComponent();
                
#if DEBUG
                System.Diagnostics.Debug.WriteLine("App: InitializeComponent completed successfully");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"App: Error during initialization: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"App: Stack trace: {ex.StackTrace}");
#endif
                throw;
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                var mainPage = new MainPage();
                var window = new Window(mainPage) { Title = "Doodle" };
                
#if DEBUG
                System.Diagnostics.Debug.WriteLine("App: MainPage created and Window initialized successfully");
#endif
                
                return window;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"App: Error creating window: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"App: Stack trace: {ex.StackTrace}");
#endif
                throw;
            }
        }
    }
}
