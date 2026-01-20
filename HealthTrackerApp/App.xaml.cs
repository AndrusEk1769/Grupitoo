namespace HealthTrackerApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Show splash page first
            return new Window(new Views.SplashPage());
        }
    }
}