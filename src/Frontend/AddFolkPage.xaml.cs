using System.Text.Json;

using Grpc.Core;
using ZXing.Net.Maui;
using ZXing.QrCode.Internal;
using ZXing.Net.Maui.Controls;

using DataModel;
using Frontend.Models;
using Frontend.Services;
using GrpcService;
using GrpcClient;

namespace Frontend;

public partial class AddFolkPage : ContentPage
{
    GrpcClient.Instance notificationClient = new Instance();
    IRelationshipsService relationshipsService = new RelationshipsService();
    CameraBarcodeReaderView cameraBarcodeReaderView = null;

    public AddFolkPage()
    {
        InitializeComponent();
        QRCode.Value = JsonSerializer.Serialize<User>(User.Instance);
    }

    protected override async void OnAppearing()
    {
        //TODO: should this be done outside of any page?
        await Task.Run(GetNotifications);
        base.OnAppearing();
    }

    private async Task GetNotifications()
    {
        //TODO: can we move this to GrpcClient.Instance?
        var client = notificationClient.Connect();
        GetNotificationRequest getNotificationRequest = new(User.Instance.ID);

        using var call = client.GenericStreamOutputMethod(new GenericInputParam
        {
            MsgIn = Marshaller.Serialize(getNotificationRequest),
        });

        while (await call.ResponseStream.MoveNext())
        {
            var (type, _version) = Marshaller.ExtractMetadata(call.ResponseStream.Current.MsgOut);

            var deserializedRequest = Marshaller.DeserializeAbstract(call.ResponseStream.Current.MsgOut, type);

            if (deserializedRequest is AddFolkSuccessNotification addFolkSuccessNotification)
            {
                relationshipsService.AddFolk(new Folk()
                {
                    ID = addFolkSuccessNotification.UserId,
                    Nickname = addFolkSuccessNotification.Nickname
                });
                var notificationText = string.Format(Texts.SuccessRelationshipMsg, addFolkSuccessNotification.Nickname);

                await MainThread.InvokeOnMainThreadAsync(()
                    => DisplayAlert(Texts.Success, notificationText, Texts.Ok));
            }
            else
            {
                throw new NotImplementedException("Unknown notification was received by AddFolk page!");
            }
        }
    }

    protected async void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        Folk folk = JsonSerializer.Deserialize<Folk>(e.Results.FirstOrDefault().Value);
        await AddFolk(folk);
    }

    async Task AddFolk(Folk folk)
    {
        cameraBarcodeReaderView.IsDetecting = false;
        var addFolk = new DataModel.AddFolkRequest(User.Instance.ID, folk.ID, User.Instance.Nickname);
        GrpcClient.Instance grpcClient = new GrpcClient.Instance();
        var response = await grpcClient.AddFolk(addFolk);

        if (response.StatusCode == AddFolkStatusCode.ConnectedSuccess
            || response.StatusCode == AddFolkStatusCode.ConnectedCompleted)
            await MainThread.InvokeOnMainThreadAsync(
                async () => {
                    Switcher.IsToggled = false;
                    await DisplayAlert(Texts.Success, string.Format(Texts.SuccessRelationshipMsg, folk.Nickname), Texts.Ok);
                });
        else if (response.StatusCode == AddFolkStatusCode.AlreadyDone)
            await MainThread.InvokeOnMainThreadAsync(
                async () => {
                    Switcher.IsToggled = false;
                    await DisplayAlert(Texts.Note, Texts.RelationshipAlreadyExistsMsg, Texts.Ok);
                });
        relationshipsService.AddFolk(folk);
    }

    private void SwitcherToggled(object sender, ToggledEventArgs _)
    {
        if (Switcher.IsToggled)
        {
            if (cameraBarcodeReaderView == null && DeviceInfo.DeviceType == DeviceType.Physical)
            {
                cameraBarcodeReaderView = new CameraBarcodeReaderView();
                cameraBarcodeReaderView.BarcodesDetected += BarcodesDetected;
                BarcodeContentView.Content = cameraBarcodeReaderView;
            }
            else if (cameraBarcodeReaderView != null)
            {
                cameraBarcodeReaderView.IsDetecting = true;
            }
        }
    }
}
