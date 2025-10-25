using Cashere.Models;
using Cashere.PageModels;

namespace Cashere.Pages;

public partial class CheckoutPage : ContentPage
{
    private readonly CheckoutPageModel _pageModel;

    public CheckoutPage(List<CartItemModel> cartItems)
    {
        InitializeComponent();
        _pageModel = new CheckoutPageModel(cartItems);
        BindingContext = _pageModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _pageModel.InitializeAsync();
    }
}
