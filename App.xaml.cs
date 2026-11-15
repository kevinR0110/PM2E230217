namespace PM2E230217
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {

            return new Window(new NavigationPage(new LoginPage())
            {
                BarBackgroundColor = Color.FromArgb("#374151"),
                BarTextColor = Colors.White
            });
        }
    }
}