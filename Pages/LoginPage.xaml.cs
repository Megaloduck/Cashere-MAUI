namespace Cashere.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginPageModel _pageModel;

    public LoginPage()
    {
        InitializeComponent();
        _pageModel = new LoginPageModel();
        BindingContext = _pageModel;
    }
}