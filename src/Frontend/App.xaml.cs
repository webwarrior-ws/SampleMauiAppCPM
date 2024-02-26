namespace Frontend;

public partial class App : Application
{
    internal static FileInfo UserFile = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "user.json"));

    public App()
    {
        InitializeComponent();

        Page mainPage = UserFile.Exists ? new MainPage() : new WelcomePage();
        MainPage = new NavigationPage(mainPage);
    }
}
