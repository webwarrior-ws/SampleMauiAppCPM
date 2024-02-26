namespace Frontend;

using DataModel;
using System;
using System.IO;
using Models;

using Microsoft.Maui.Devices.Sensors;
using System.Text.Json;

#if ANDROID || IOS
using Shiny.Push;
#endif

public partial class MainPage : ContentPage
{
    GrpcClient.Instance grpcClient = new GrpcClient.Instance();

#if ANDROID || IOS
    IPushManager pushManager;
#endif

    public MainPage()
    {
        InitializeComponent();

        MainThread.BeginInvokeOnMainThread(ReportGpsLocation);
    }

    async void ReportGpsLocation()
    {
        if (!await CheckIfLocationPermissionGranted())
            return;

        await LoadUser();
        try
        {
            await GatherLocation(User.Instance.ID);
        }
        catch (Microsoft.Maui.ApplicationModel.FeatureNotEnabledException)
        {
            FallbackLabel.Text = Texts.GpsLocationFeatureIsNeeded;
            MainLayout.IsVisible = false;
        }
    }


    async Task LoadUser()
    {
        if (!App.UserFile.Exists)
        {
            User.Instance.ID = await grpcClient.RegisterNewAppInstall();
            File.WriteAllText(App.UserFile.FullName, JsonSerializer.Serialize<User>(User.Instance));
#if ANDROID || IOS
            SetupPushNotifications(User.Instance.ID);
#endif
        }
        else
        {
            string userJSON = File.ReadAllText(App.UserFile.FullName);
            User.Instance = JsonSerializer.Deserialize<User>(userJSON);
        }
    }

    void SetupPushNotifications(int newUserId)
    {
#if ANDROID || IOS
        Task.Run(async () => await SetupPushNotificationsAsync(newUserId.ToString()));
#endif
    }

    protected override void OnAppearing()
    {
        MainThread.BeginInvokeOnMainThread(() => {
            freeOrBusySwitch.IsEnabled = true;
        });
        base.OnAppearing();
    }

    private void OnSwitchToggled(object sender, EventArgs e)
    {
        Switch me = (Switch)sender;
        if (!me.IsToggled)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                CounterLabel.Text = String.Empty;
                SemanticScreenReader.Announce(CounterLabel.Text);
            });
            return;
        }

        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                //FIXME: maybe we can now that this issue has been fixed: https://github.com/dotnet/maui/issues/8842
                CounterLabel.Text = Texts.CantGetGPSLocationBecauseOfRunningInWindows;
            });
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(ReportGpsLocation);
        }
    }

    private async Task GatherLocation(int userId)
    {
        MainThread.BeginInvokeOnMainThread(() => {
            CounterLabel.Text = Texts.GpsLocationInfoBegin;
            SemanticScreenReader.Announce(CounterLabel.Text);
        });
        var req = new GeolocationRequest(GeolocationAccuracy.Low);
        var location = await Geolocation.GetLocationAsync(req);
        var updateGpsLocationRequest =
            new DataModel.UpdateGpsLocationRequest(
                userId,
                location.Latitude,
                location.Longitude
            );
        MainThread.BeginInvokeOnMainThread(() => {
            CounterLabel.Text = string.Format(Texts.GpsLocationInfo, DateTime.Now.ToString(), location.Latitude, location.Longitude);
            SemanticScreenReader.Announce(CounterLabel.Text);
        });
        await grpcClient.UpdateGpsLocation(updateGpsLocationRequest);
        MainThread.BeginInvokeOnMainThread(() => {
            CounterLabel.Text = CounterLabel.Text + "!!!!!!!!!!!";
            SemanticScreenReader.Announce(CounterLabel.Text);
        });
    }

    #region Permissions
    private async Task<PermissionStatus> RequestAndGetLocationPermission()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

        if (status == PermissionStatus.Granted)
            return status;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            return status;

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            await DisplayAlert(Texts.Warning, Texts.BackgroundLocationPermissionIsNeeded, Texts.Ok);
            await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        status = await Permissions.RequestAsync<Permissions.LocationAlways>();

        return status;
    }

    private async Task<bool> CheckIfLocationPermissionGranted()
    {
        var locationPermissionStatus = await RequestAndGetLocationPermission();
        if (locationPermissionStatus != PermissionStatus.Granted)
        {
            MainThread.BeginInvokeOnMainThread(() =>
                MainLayout.IsVisible = false
            );
            return false;
        }

        return true;
    }
    #endregion

    #region Navigations
    void NavigateToAddFolkClicked(object sender, EventArgs evArgs)
    {
        Navigation.PushAsync(new AddFolkPage());
    }

    void NavigateToRelationshipsClicked(object sender, EventArgs evArgs)
    {
        Navigation.PushAsync(new RelationshipsPage());
    }
    #endregion


#if ANDROID || IOS
    async Task SetupPushNotificationsAsync(string userID)
    {
        pushManager = this.Handler.MauiContext.Services.GetService<IPushManager>();
        var result = await this.pushManager.RequestAccess();
        if (result.Status == AccessState.Available)
        {
            await pushManager.Tags.AddTag(userID);
        }
    }
#endif
}

