using Doodle.Services;

namespace Doodle
{
    public partial class App : Application
    {
        private ThemeService? _themeService;

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

        public void SetThemeService(ThemeService themeService)
        {
            _themeService = themeService;
            _themeService?.LoadTheme();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                var mainPage = new MainPage();
                var window = new Window(mainPage) 
                { 
                    Title = "Doodle - Journaling App",
                    Width = 1400,
                    Height = 900,
                    MinimumWidth = 1000,
                    MinimumHeight = 600
                };
                
#if DEBUG
                System.Diagnostics.Debug.WriteLine("App: MainPage created and Window initialized successfully");
#endif
                
                // Load theme immediately when window is created
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(500); // Small delay to ensure services are ready
                        if (_themeService != null)
                        {
                            _themeService.LoadTheme();
                            System.Diagnostics.Debug.WriteLine("App: Theme loaded on window creation");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"App: Error loading theme on window creation: {ex.Message}");
                    }
                });
                
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
