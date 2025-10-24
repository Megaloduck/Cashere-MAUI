using Cashere.PageModels;
using Cashere.Models;

namespace Cashere.Pages;

public partial class AddEditMenuItemPage : ContentPage
{
    private readonly AddEditMenuItemPageModel _pageModel;

    public AddEditMenuItemPage(MenuItemModel itemToEdit = null)
    {
        InitializeComponent();
        _pageModel = new AddEditMenuItemPageModel(itemToEdit);
        BindingContext = _pageModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _pageModel.InitializeAsync();
    }
}