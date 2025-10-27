using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
namespace Resto2025;

public partial class ResetEmail : ContentPage
{
	public ResetEmail()
	{
		InitializeComponent();
    }

    private async void B_ResetUbahPassword_Clicked(object sender, EventArgs e)
    {


        if (sender is Button image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        // Validasi input email tidak boleh kosong
        string email = L_Email.Text?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlert("Error", "Email tidak boleh kosong", "OK");
            L_Email.Focus();
            return;
        }
        //validasi format email
        bool isValidEmail = System.Text.RegularExpressions.Regex.IsMatch(email,
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        if (!isValidEmail)
        {
          
            await DisplayAlert("Error", "Format email tidak valid", "OK");
            L_Email.Focus();
            return;
        }

        //validasi input nomor hp tidak boleh kosong
        string nomorHp = L_NoHp.Text?.Trim();
        if (string.IsNullOrEmpty(nomorHp))
        {
            await DisplayAlert("Error", "Nomor HP tidak boleh kosong", "OK");
            L_NoHp.Focus();
            return;
        }

        //validasi input nomor hp hanya angka dan minimal 10 digit
        bool isNumeric = System.Text.RegularExpressions.Regex.IsMatch(nomorHp, @"^[0-9]+$");
        if (!isNumeric || nomorHp.Length < 10)
        {
            await DisplayAlert("Error", "Nomor HP harus berupa angka dan minimal 10 digit", "OK");
            L_NoHp.Focus();
            return;
        }

        if(B_ResetUbahPassword.Text == "Reset")
        {
            request_pw();
        }
        else if(B_ResetUbahPassword.Text == "Ubah Password")
        {
            await DisplayAlert("Info", "Fitur Ubah Password belum tersedia", "OK");
        }

    }




    private async void request_pw()
    {
        //staffID sementara nanti ganti sama temp login
     
        var data = new Dictionary<string, string>
                {
                    { "nomor_hp", L_NoHp.Text },
                    { "email", L_Email.Text }
                };

        var jsonData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var client = new HttpClient();
        string ip = App.API_HOST + "reset/reset_pw.php";

        var response = await client.PostAsync(ip, jsonData);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

        if (responseObject?["status"] == "duplikat" || responseObject?["status"] == "error")
        {

            await DisplayAlert("Error", responseObject["message"], "OK");

        }
        else if (responseObject?["status"] == "success")
        {

            L_Email.IsEnabled = false;
            L_Email.Opacity = 0.6;
            L_NoHp.IsEnabled = false;
            L_NoHp.Opacity = 0.6;
            await DisplayAlert("Success", "Permintaan reset password berhasil. Silakan cek email Anda.", "OK");
            Frame_Token.IsVisible = true;
            B_ResetUbahPassword.Text = "Lihat Password";
        }
    }

}