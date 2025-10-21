using Cashere.PageModels;

namespace Cashere.Pages;

public partial class CheckoutPage : ContentPage
{
    private readonly CheckoutPageModel _pageModel;

    public CheckoutPage()
    {
        InitializeComponent();
        _pageModel = new CheckoutPageModel();
        BindingContext = _pageModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _pageModel.InitializeAsync();
    }
}