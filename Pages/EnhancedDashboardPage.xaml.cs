using Cashere.PageModels;

namespace Cashere.Pages;

public partial class EnhancedDashboardPage : ContentPage
{
    private readonly EnhancedDashboardPageModel _pageModel;

    public EnhancedDashboardPage()
    {
        InitializeComponent();
        _pageModel = new EnhancedDashboardPageModel();
        BindingContext = _pageModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _pageModel.InitializeAsync();
    }
}