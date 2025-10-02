namespace Resto2025;

public partial class Login : ContentPage
{
	public Login()
	{
		InitializeComponent();
	}

    private void TapViewPW_Tapped(object sender, TappedEventArgs e)
    {
		if(L_Lihat_Password.Text == "Lihat Password")
		{
			L_Lihat_Password.Text = "Sembunyikan Password";
			L_Password.IsPassword = false;
		}
		else
		{
            L_Lihat_Password.Text = "Lihat Password";
            L_Password.IsPassword = true;
        }
    }
}