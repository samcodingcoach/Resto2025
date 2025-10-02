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

    private async void B_Login_Clicked(object sender, EventArgs e)
    {

        if (sender is Button image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        // Validasi email dan password
        string email = L_Email.Text?.Trim();
        string password = L_Password.Text?.Trim();

        // Cek apakah email kosong
        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlert("Error", "Email tidak boleh kosong", "OK");
            return;
        }

        // Cek apakah password kosong
        if (string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Password tidak boleh kosong", "OK");
            return;
        }

        // Cek apakah email dalam format yang benar
        bool isValidEmail = System.Text.RegularExpressions.Regex.IsMatch(email, 
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        
        if (!isValidEmail)
        {
            await DisplayAlert("Error", "Format email tidak valid", "OK");
            return;
        }

        // Cek apakah password minimal 4 digit
        if (password.Length < 4)
        {
            await DisplayAlert("Error", "Password minimal harus 4 karakter", "OK");
            return;
        }

       
    }
}