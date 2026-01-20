using System.Windows.Input;

namespace HealthTrackerApp.ViewModels
{
    public class SplashViewModel : BaseViewModel
    {
        public Command EnterCommand { get; }

        public SplashViewModel()
        {
            EnterCommand = new Command(async () => await EnterAppAsync());
        }

        private Task EnterAppAsync()
        {
            // Navigate to the main app shell
            Application.Current!.MainPage = new AppShell();
            return Task.CompletedTask;
        }
    }
}
