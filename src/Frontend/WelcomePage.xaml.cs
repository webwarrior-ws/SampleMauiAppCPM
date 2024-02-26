namespace Frontend;

using DataModel;
using Models;

public partial class WelcomePage : ContentPage
{
    bool permissionWasDenied = false;

    public WelcomePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        if (permissionWasDenied)
            await CheckIfPermissionGrantedInDeviceSettings();
    }

    async void GivePermissionButtonClicked(object sender, System.EventArgs e)
    {
        if (!await CheckIfNicknameWasSet())
            return;

        if (permissionWasDenied)
        {
            AppInfo.Current.ShowSettingsUI();
            return;
        }

        var status = await Permissions.RequestAsync<Permissions.LocationAlways>();
        if (status == PermissionStatus.Granted)
            App.Current.MainPage = new NavigationPage(new MainPage());
        else
            UpdateUIWhenPermissionWasDenied();
    }

    async Task CheckIfPermissionGrantedInDeviceSettings()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
        if (status == PermissionStatus.Granted)
            App.Current.MainPage = new NavigationPage(new MainPage());
    }

    void UpdateUIWhenPermissionWasDenied()
    {
        permissionWasDenied = true;
        GivePermissionButton.Text = Texts.OpenSettings;
        InfoText.Text = Texts.BackgroundLocationPermissionIsNeeded;
    }

    async Task<bool> CheckIfNicknameWasSet()
    {
        User.Instance.Nickname = NicknameEntry.Text;
        if (string.IsNullOrWhiteSpace(NicknameEntry.Text))
        {
            await DisplayAlert(Texts.Warning, Texts.YouDidntSetYourNickname, Texts.Ok);
            return false;
        }

        return true;
    }

}