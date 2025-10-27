using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
namespace Resto2025;

public partial class ResetEmail : Popup
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
            await Application.Current.MainPage.DisplayAlert("Error", "Email tidak boleh kosong", "OK");
            L_Email.Focus();
            return;
        }
        //validasi format email
        bool isValidEmail = System.Text.RegularExpressions.Regex.IsMatch(email,
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        if (!isValidEmail)
        {
          
            await Application.Current.MainPage.DisplayAlert("Error", "Format email tidak valid", "OK");
            L_Email.Focus();
            return;
        }

        //validasi input nomor hp tidak boleh kosong
        string nomorHp = L_NoHp.Text?.Trim();
        if (string.IsNullOrEmpty(nomorHp))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Nomor HP tidak boleh kosong", "OK");
            L_NoHp.Focus();
            return;
        }

        //validasi input nomor hp hanya angka dan minimal 10 digit
        bool isNumeric = System.Text.RegularExpressions.Regex.IsMatch(nomorHp, @"^[0-9]+$");
        if (!isNumeric || nomorHp.Length < 10)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Nomor HP harus berupa angka dan minimal 10 digit", "OK");
            L_NoHp.Focus();
            return;
        }

        if(B_ResetUbahPassword.Text == "Reset")
        {
            request_pw();
        }
        else if(B_ResetUbahPassword.Text == "Lihat Password")
        {
            // await Application.Current.MainPage.DisplayAlert("Info", "Fitur Ubah Password belum tersedia", "OK");
            LihatPassword();
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

            await Application.Current.MainPage.DisplayAlert("Error", responseObject["message"], "OK");

        }
        else if (responseObject?["status"] == "success")
        {

            L_Email.IsEnabled = false;
            L_Email.Opacity = 0.6;
            L_NoHp.IsEnabled = false;
            L_NoHp.Opacity = 0.6;
            await Application.Current.MainPage.DisplayAlert("Success", "Permintaan reset password berhasil. Silakan cek email Anda.", "OK");
            Frame_Token.IsVisible = true;
            B_ResetUbahPassword.Text = "Lihat Password";
        }
    }

   


    private async void LihatPassword()
    {
        string? token = L_Token.Text?.Trim();
        if (string.IsNullOrEmpty(token))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Token tidak boleh kosong", "OK");
            L_Token.Focus();
            return;
        }

        // Validasi format base64
        if (!IsValidBase64(token))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Format token tidak valid (bukan base64)", "OK");
            return;
        }

        string phoneNumber = L_NoHp.Text?.Trim();
        if (string.IsNullOrEmpty(phoneNumber))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Nomor HP tidak ditemukan", "OK");
            return;
        }

        try
        {
            string decryptedPassword = DecryptPassword(token, phoneNumber);
            await Application.Current.MainPage.DisplayAlert("Password", $"Password: {decryptedPassword}", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error",
                $"Dekripsi gagal. Pastikan token dan nomor HP sesuai dengan yang terdaftar.\n\nDetail: {ex.Message}",
                "OK");
            Debug.WriteLine($"Decryption error: {ex}");
        }
    }


    private string DecryptPassword(string encryptedPassword, string key)
    {
        try
        {
            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = System.Security.Cryptography.CipherMode.CBC;
                aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

                // Generate IV: hash SHA256, ambil 16 char pertama hex string, convert ke bytes
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));

                    // Convert hash ke hex string (seperti PHP hash())
                    string hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                    // Ambil 16 karakter pertama, convert ke bytes (ini jadi 16 bytes)
                    string ivHexString = hashHex.Substring(0, 16);
                    byte[] iv = Encoding.UTF8.GetBytes(ivHexString);
                    aes.IV = iv;

                    // Key: gunakan key raw langsung, tapi perlu pad/truncate ke 32 bytes
                    byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                    byte[] aesKey = new byte[32]; // AES-256 needs 32 bytes

                    // Jika key lebih pendek dari 32 bytes, akan di-pad dengan 0
                    // Jika lebih panjang, akan di-truncate
                    Array.Copy(keyBytes, 0, aesKey, 0, Math.Min(keyBytes.Length, 32));
                    aes.Key = aesKey;
                }

                // Decode base64 string
                byte[] encryptedBytes = Convert.FromBase64String(encryptedPassword);

                // Decrypt
                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Dekripsi gagal: {ex.Message}", ex);
        }
    }


    private bool IsValidBase64(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return false;

        try
        {
            Convert.FromBase64String(base64String);
            return true;
        }
        catch
        {
            return false;
        }
    }

}