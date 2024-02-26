using DataModel;

namespace Frontend;

public partial class ClosenessPage : ContentPage
{
    private readonly int userID;
    private readonly int folkID;
    private readonly GrpcClient.Instance grpcClient;
    private bool shouldUpdate = false;

    public ClosenessPage(int userID, int folkID, Closeness previousCloseness)
    {
        InitializeComponent();

        this.userID = userID;
        this.folkID = folkID;
        grpcClient = new GrpcClient.Instance();
        SetSwitchesProperly(previousCloseness);
        shouldUpdate = true;
    }

    void SetSwitchesProperly(Closeness closeness)
    {
        // NOTE: toggled = false means that the switch points to the left

        AcquaintanceComradeSwitcher.IsEnabled = true;
        switch (closeness)
        {
            case Closeness.Acquaintance:
                AcquaintanceComradeSwitcher.IsToggled = false;

                FriendFamilySwitcher.IsToggled = false;
                FriendFamilySwitcher.IsEnabled = false;

                RegularCloseSwitcher.IsToggled = false;
                RegularCloseSwitcher.IsEnabled = false;

                CloseCrushSwitcher.IsToggled = false;
                CloseCrushSwitcher.IsEnabled = false;
                break;

            case Closeness.RegularFamily:
                AcquaintanceComradeSwitcher.IsToggled = true;

                FriendFamilySwitcher.IsToggled = true;
                FriendFamilySwitcher.IsEnabled = true;

                RegularCloseSwitcher.IsToggled = false;
                RegularCloseSwitcher.IsEnabled = true;

                CloseCrushSwitcher.IsToggled = false;
                CloseCrushSwitcher.IsEnabled = false;
                break;

            case Closeness.CloseFamily:
                AcquaintanceComradeSwitcher.IsToggled = true;

                FriendFamilySwitcher.IsToggled = true;
                FriendFamilySwitcher.IsEnabled = true;

                RegularCloseSwitcher.IsToggled = true;
                RegularCloseSwitcher.IsEnabled = true;

                CloseCrushSwitcher.IsToggled = false;
                CloseCrushSwitcher.IsEnabled = false;
                break;

            case Closeness.RegularFriend:
                AcquaintanceComradeSwitcher.IsToggled = true;

                FriendFamilySwitcher.IsToggled = false;
                FriendFamilySwitcher.IsEnabled = true;

                RegularCloseSwitcher.IsToggled = false;
                RegularCloseSwitcher.IsEnabled = true;

                CloseCrushSwitcher.IsToggled = false;
                CloseCrushSwitcher.IsEnabled = false;
                break;

            case Closeness.CloseFriend:
                AcquaintanceComradeSwitcher.IsToggled = true;

                FriendFamilySwitcher.IsToggled = false;
                FriendFamilySwitcher.IsEnabled = true;

                RegularCloseSwitcher.IsToggled = true;
                RegularCloseSwitcher.IsEnabled = true;

                CloseCrushSwitcher.IsToggled = false;
                CloseCrushSwitcher.IsEnabled = true;
                break;

            case Closeness.Crush:
                AcquaintanceComradeSwitcher.IsToggled = true;

                FriendFamilySwitcher.IsToggled = false;
                FriendFamilySwitcher.IsEnabled = true;

                RegularCloseSwitcher.IsToggled = true;
                RegularCloseSwitcher.IsEnabled = true;

                CloseCrushSwitcher.IsToggled = true;
                CloseCrushSwitcher.IsEnabled = true;
                break;
        }
    }

    private async Task UpdateCloseness()
    {
        if (!shouldUpdate)
        {
            return;
        }

        var isComrade = AcquaintanceComradeSwitcher.IsToggled;

        Closeness newCloseness;
        if (!isComrade)
        {
            newCloseness = Closeness.Acquaintance;
        }
        else
        {
            var isFamily = FriendFamilySwitcher.IsToggled;
            var isClose = RegularCloseSwitcher.IsToggled;
            if (isFamily)
            {
                if (isClose)
                {
                    newCloseness = Closeness.CloseFamily;
                }
                else
                {
                    newCloseness = Closeness.RegularFamily;
                }
            }
            else
            {
                if (isClose)
                {
                    var isCrush = CloseCrushSwitcher.IsToggled;
                    if (isCrush)
                    {
                        newCloseness = Closeness.Crush;
                    }
                    else
                    {
                        newCloseness = Closeness.CloseFriend;
                    }
                }
                else
                {
                    newCloseness = Closeness.RegularFriend;
                }
            }
        }

        UpdateClosenessRequest updateCloseness = new(userID, folkID, (int)newCloseness);
        await grpcClient.UpdateCloseness(updateCloseness);
        SetSwitchesProperly(newCloseness);
    }

    async void AcquaintanceComradeSwitcherToggled(object sender, ToggledEventArgs e)
    {
        await UpdateCloseness();
    }

    async void FriendFamilySwitcherToggled(object sender, ToggledEventArgs e)
    {
        await UpdateCloseness();
    }

    async void RegularCloseSwitcherToggled(object sender, ToggledEventArgs e)
    {
        await UpdateCloseness();
    }

    async void CloseCrushSwitcherToggled(object sender, ToggledEventArgs e)
    {
        await UpdateCloseness();
    }
}

