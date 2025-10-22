using Cashere.PageModels;

namespace Cashere.Pages;

public partial class AdminPanelPage : ContentPage
{
    private readonly AdminPanelPageModel _pageModel;

    public AdminPanelPage()
    {
        InitializeComponent();
        _pageModel = new AdminPanelPageModel();
        BindingContext = _pageModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _pageModel.InitializeAsync();
    }
}