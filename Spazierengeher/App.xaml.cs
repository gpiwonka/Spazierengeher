namespace Spazierengeher
{
    public partial class App : Application
    {
        public App(IServiceProvider services)
        {
            InitializeComponent();
            MainPage = services.GetRequiredService<MainPage>();
        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new MainPage()) { Title = "Spazierengeher" };
        //}
    }
}
