using DataModel;
using Frontend.Models;
using Frontend.Services;

namespace Frontend;

public partial class RelationshipsPage : ContentPage
{
    Dictionary<int, int> folks;
    IRelationshipsService relationshipsService = new RelationshipsService();

    public RelationshipsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        folks = await GetFolks();
        relationshipsService.UpdateRelationshipsInFile(folks);
        var folksList = relationshipsService.GetUserListOfFolks();
        MainThread.BeginInvokeOnMainThread(() => {
            ListOfFolks.ItemsSource = folksList;
            this.EmptyViewLabel.Text = Texts.NoFolksAddedYet;
        });
    }

    async Task<Dictionary<int, int>> GetFolks()
    {
        var instance = new GrpcClient.Instance();
        var response = await instance.GetFolks(new GetFolksRequest(User.Instance.ID));
        return response.Folks;
    }

    async void ListOfFolksItemSelected(object sender, TappedEventArgs e)
    {
        if (e.Parameter == null)
            return;

        Folk folk = (Folk)e.Parameter;

        if (!folks.ContainsKey(folk.ID))
            return;

        ListOfFolks.SelectedItem = null;
        await Navigation.PushAsync(new ClosenessPage(User.Instance.ID, folk.ID, (Closeness)folks[folk.ID]));
    }

}
