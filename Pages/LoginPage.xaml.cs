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
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Optional: Add fade-in animation
        this.Opacity = 0;
        this.FadeTo(1, 500, Easing.CubicInOut);
    }
}