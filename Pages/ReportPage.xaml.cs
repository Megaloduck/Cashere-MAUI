using Cashere.PageModels;

namespace Cashere.Pages;

public partial class ReportPage : ContentPage
{
    private readonly ReportPageModel _pageModel;

    public ReportPage()
    {
        InitializeComponent();
        _pageModel = new ReportPageModel();
        BindingContext = _pageModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _pageModel.InitializeAsync();
    }
}