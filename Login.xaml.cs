using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using System.Net.Http;
using System.Globalization;


namespace Resto2025;

public partial class Login : ContentPage
{
	public Login()
	{
		InitializeComponent();
		// Don't set the text immediately since data might not be loaded yet
		System.Diagnostics.Debug.WriteLine($"Nama APP: {L_NamaApp.Text}");
		
		// Start a timer or check periodically for the value
		_ = Task.Run(async () => await WaitForAppName());
		
		// Set the version from the project properties
		SetVersion();
    }
	
	protected override void OnAppearing()
	{
		base.OnAppearing();
		UpdateNamaApp();
		CheckExistingSession();
	}
	private void CheckExistingSession()
	{
		bool hasId = Preferences.ContainsKey("ID_USER");
		bool hasName = Preferences.ContainsKey("NAMA_LENGKAP");
		bool hasTimestamp = Preferences.ContainsKey("TIMESTAMP");

		if (!hasId || !hasName || !hasTimestamp)
		{
			return;
		}

		string storedTimestamp = Preferences.Get("TIMESTAMP", string.Empty);
		string today = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
		string timestampDatePart = string.Empty;

		if (!string.IsNullOrEmpty(storedTimestamp))
		{
			var parts = storedTimestamp.Split('_');
			if (parts.Length > 0)
			{
				timestampDatePart = parts[0];
			}
		}

		if (!string.IsNullOrEmpty(timestampDatePart) && timestampDatePart == today)
		{
			Application.Current.MainPage = new MainPage();
			return;
		}

		Preferences.Clear();
	}

	
	private void UpdateNamaApp()
	{
		if (!string.IsNullOrEmpty(App.NAMA_APLIKASI))
		{
			L_NamaApp.Text = App.NAMA_APLIKASI;
			System.Diagnostics.Debug.WriteLine($"Nama APP: {L_NamaApp.Text}");
		}
	}
	
	private async Task WaitForAppName()
	{
		// Wait up to 10 seconds for the app name to be loaded
		int attempts = 0;
		const int maxAttempts = 20; // 20 attempts * 500ms = 10 seconds
		const int delayMs = 500;
		
		while (attempts < maxAttempts)
		{
			await Task.Delay(delayMs);
			
			if (!string.IsNullOrEmpty(App.NAMA_APLIKASI))
			{
				// Run UI update on the main thread
				MainThread.BeginInvokeOnMainThread(() => {
					L_NamaApp.Text = App.NAMA_APLIKASI;
					System.Diagnostics.Debug.WriteLine($"Nama APP: {L_NamaApp.Text}");
				});
				return; // Exit if we found the value
			}
			
			attempts++;
		}
	}
	
	private void SetVersion()
	{
		// Get the application version from the project properties
		var version = AppInfo.Current.Version;
		L_Version.Text = $"Point of Sales Version {version}";
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

        FormLogin.IsVisible = false;
        LoadingIndicator.IsVisible = true; LoadingIndicator.IsRunning = true;

		cek_login();

    }




    private async void cek_login()
    {
      
        var data = new Dictionary<string, string>
        {
            { "email", L_Email.Text.ToString() },
            { "password", L_Password.Text.ToString() }
        };

        var jsonData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var client = new HttpClient();
        string ip = App.API_LOGIN;

        var response = await client.PostAsync(ip, jsonData);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
		
		if (responseObject["status"] == "success")
        {

			await Task.Delay(3000);
            LoadingIndicator.IsVisible = false; LoadingIndicator.IsRunning = false;

            System.Diagnostics.Debug.WriteLine($"id_user:{responseObject["id_user"]}");
            //await DisplayAlert("Informasi Login", responseObject["message"], "OK");	

            Preferences.Set("ID_USER", responseObject["id_user"]);
            Preferences.Set("NAMA_LENGKAP", responseObject["nama_lengkap"]);
            Preferences.Set("TIMESTAMP", $"{DateTime.Now:yyyyMMdd}_{responseObject["id_user"]}");

            Application.Current.MainPage = new MainPage();


        }
        else
		{
            await Task.Delay(3000);
            FormLogin.IsVisible = true;
            LoadingIndicator.IsVisible = false; LoadingIndicator.IsRunning = false;
        }
    }

}