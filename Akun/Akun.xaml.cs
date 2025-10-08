using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography;
using System.Linq;
using System.Text.RegularExpressions;

namespace Resto2025.Akun;

public partial class Akun : ContentPage
{
    private List<list_promo> _listpromo;
    string ID_USER = Preferences.Get("ID_USER", string.Empty);
    string PASSWORD_ENC = string.Empty;
    string PASSWORD_DEC = string.Empty;
    public Akun()
    {
        InitializeComponent();
        _listpromo = new List<list_promo>(); // taruh di public load 
        get_listpromo();
        get_profile();
    }

    public class list_profile
    {
        public string id_user { get; set; } = string.Empty;
        public string nama_lengkap { get; set; } = string.Empty;
        public string jabatan { get; set; } = string.Empty;
        public string nomor_hp { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;

    }

    public class list_promo
    {
        public string id_promo { get; set; } = string.Empty;
        public string nama_promo { get; set; } = string.Empty;
        public string kode_promo { get; set; } = string.Empty;
        public string pilihan_promo { get; set; } = string.Empty;
        public string tanggalmulai_promo { get; set; } = string.Empty;
        public string tanggalselesai_promo { get; set; } = string.Empty;
        public double nilai { get; set; } = 0;
        public int kuota { get; set; } = 0;
        public double min_pembelian { get; set; } = 0;
        public string nilai_string { get; set; } = string.Empty;


    }

    private async void get_profile()
    {
        try
        {

            string url = App.API_HOST + "kasir/pegawai.php?id_user=" + ID_USER;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    List<list_profile> rowData = JsonConvert.DeserializeObject<List<list_profile>>(json);

                    if (rowData != null && rowData.Count > 0)
                    {
                        list_profile row = rowData[0];
                        L_Nama.Text = row.nama_lengkap;
                        L_NoHP.Text = row.nomor_hp;
                        L_Jabatan.Text = row.jabatan;
                        L_Email.Text = row.email;
                        PASSWORD_ENC = row.password;
                        PASSWORD_DEC = DecryptPassword(PASSWORD_ENC, row.nomor_hp);
                        System.Diagnostics.Debug.WriteLine($"OLD:{PASSWORD_ENC}");
                        System.Diagnostics.Debug.WriteLine($"PW: {PASSWORD_DEC}");
                        L_Password.Text = PASSWORD_DEC;
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
        }

        catch (Exception ex)
        {

        }
    }

    private string DecryptPassword(string encryptedPassword, string key)
    {
        if (string.IsNullOrEmpty(encryptedPassword) || string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        try
        {
            byte[] cipherBytes = Convert.FromBase64String(encryptedPassword);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] aesKey = new byte[32];
            Array.Clear(aesKey, 0, aesKey.Length);
            Array.Copy(keyBytes, aesKey, Math.Min(keyBytes.Length, aesKey.Length));

            byte[] ivBytes;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] keyHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                string hashHex = BitConverter.ToString(keyHash).Replace("-", string.Empty).ToLowerInvariant();
                ivBytes = Encoding.UTF8.GetBytes(hashHex.Substring(0, 16));
            }

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = aesKey;
                aes.IV = ivBytes;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    private string EncryptPassword(string plainPassword, string key)
    {
        if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        try
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainPassword);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] aesKey = new byte[32];
            Array.Clear(aesKey, 0, aesKey.Length);
            Array.Copy(keyBytes, aesKey, Math.Min(keyBytes.Length, aesKey.Length));

            byte[] ivBytes;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] keyHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                string hashHex = BitConverter.ToString(keyHash).Replace("-", string.Empty).ToLowerInvariant();
                ivBytes = Encoding.UTF8.GetBytes(hashHex.Substring(0, 16));
            }

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = aesKey;
                aes.IV = ivBytes;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    return Convert.ToBase64String(cipherBytes);
                }
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    private async void get_listpromo()
    {


        string url = App.API_HOST + "promo/list_promo.php";
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            List<list_promo> rowData = JsonConvert.DeserializeObject<List<list_promo>>(json);

            _listpromo.Clear();


            for (int i = 0; i < rowData.Count; i++)
            {
                if (rowData[i].pilihan_promo == "persen")
                {
                    rowData[i].nilai_string = $"{rowData[i].nilai}%";
                }
                else if (rowData[i].pilihan_promo == "nominal")
                {
                    rowData[i].nilai_string = FormatCurrency(rowData[i].nilai);
                }
                _listpromo.Add(rowData[i]);
            }

            //total = rowData.Count;
            lv_promo.ItemsSource = _listpromo;

        }
        else
        {

        }

    }

    private string FormatCurrency(double amount)
    {
        // Format the amount as Indonesian Rupiah with thousand separators
        return "Rp " + amount.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
    }

    private async void B_Update_Clicked(object sender, EventArgs e)
    {
        if (sender is Button image)
        {
            await image.FadeTo(0.3, 100);
            await image.FadeTo(1, 200);
        }

        string nama = L_Nama.Text?.Trim() ?? string.Empty;
        string jabatan = L_Jabatan.Text?.Trim() ?? string.Empty;
        string nomorHp = L_NoHP.Text?.Trim() ?? string.Empty;
        string email = L_Email.Text?.Trim() ?? string.Empty;
        string password = L_Password.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nama) || string.IsNullOrWhiteSpace(jabatan) ||
            string.IsNullOrWhiteSpace(nomorHp) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Validasi", "Semua field wajib diisi.", "OK");
            return;
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            await DisplayAlert("Validasi", "Format email tidak valid.", "OK");
            return;
        }

        if (!nomorHp.StartsWith("08") || nomorHp.Length < 11 || !nomorHp.All(char.IsDigit))
        {
            await DisplayAlert("Validasi", "Nomor handphone harus diawali 08 dan minimal 11 digit.", "OK");
            return;
        }

        await update_akun();
    }

    private async void B_UbahPassword_Clicked(object sender, EventArgs e)
    {
        if (sender is Button image)
        {
            await image.FadeTo(0.3, 100);
            await image.FadeTo(1, 200);
        }

        string result = await DisplayPromptAsync("Password Baru", "Inputkan Password Baru");
        if (string.IsNullOrWhiteSpace(result))
        {
            return;
        }

        string key = (L_NoHP.Text ?? string.Empty).Trim();
        string encrypted = EncryptPassword(result.Trim(), key);

        if (string.IsNullOrEmpty(encrypted))
        {
            await DisplayAlert("Gagal", "Enkripsi password gagal.", "OK");
            return;
        }

        PASSWORD_DEC = result.Trim();
        PASSWORD_ENC = encrypted;
        await simpan_password();
    }

    private async void B_Logout_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is VisualElement element)
        {
            await element.FadeTo(0.3, 100);
            await element.FadeTo(1, 200);
        }
        else if (sender is TapGestureRecognizer gesture && gesture.Parent is VisualElement parentElement)
        {
            await parentElement.FadeTo(0.3, 100);
            await parentElement.FadeTo(1, 200);
        }

        bool confirm = await DisplayAlert("Konfirmasi", "Apakah anda ingin logout?", "Ya", "Tidak");
        if (!confirm)
        {
            return;
        }

        Preferences.Clear();
        Application.Current.MainPage = new NavigationPage(new global::Resto2025.Login());
    }

    private void TapPromo_Tapped(object sender, EventArgs e)
    {

    }

    private async Task simpan_password()
    {
        //staffID sementara nanti ganti sama temp login

        var data = new Dictionary<string, string>
                {
                    { "id_user", ID_USER },
                    { "password", PASSWORD_ENC }
                };

        var jsonData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        using var client = new HttpClient();
        string ip = App.API_HOST + "kasir/update_pw.php";

        var response = await client.PostAsync(ip, jsonData);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

        if (responseObject["status"] == "success")
        {
            await DisplayAlert("Update Info", responseObject["message"], "OK");

        }
    }

    private async Task update_akun()
    {
        string nama = L_Nama.Text?.Trim() ?? string.Empty;
        string jabatan = L_Jabatan.Text?.Trim() ?? string.Empty;
        string nomorHp = L_NoHP.Text?.Trim() ?? string.Empty;
        string email = L_Email.Text?.Trim() ?? string.Empty;
        string plainPassword = L_Password.Text?.Trim();

        if (string.IsNullOrEmpty(plainPassword))
        {
            plainPassword = PASSWORD_DEC;
        }

        if (string.IsNullOrEmpty(plainPassword))
        {
            await DisplayAlert("Validasi", "Password tidak boleh kosong.", "OK");
            return;
        }

        string encryptedPassword = EncryptPassword(plainPassword, nomorHp);

        if (string.IsNullOrEmpty(encryptedPassword))
        {
            await DisplayAlert("Gagal", "Enkripsi password gagal.", "OK");
            return;
        }

        PASSWORD_DEC = plainPassword;
        PASSWORD_ENC = encryptedPassword;

        var data = new Dictionary<string, string>
        {
            { "id_user", ID_USER },
            { "password", PASSWORD_ENC },
            { "nama_lengkap", nama },
            { "email", email },
            { "nomor_hp", nomorHp },

        };

        var jsonData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        using var client = new HttpClient();
        string ip = App.API_HOST + "kasir/update_akun.php";

        var response = await client.PostAsync(ip, jsonData);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

        if (responseObject["status"] == "success")
        {
            await DisplayAlert("Update Info", responseObject["message"], "OK");

        }
    }

    private async void B_Closing_Tapped(object sender, TappedEventArgs e)
    {



        if (sender is Image image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }


        bool confirm = await DisplayAlert("Konfirmasi", "Yakin untuk melakukan closing?", "Yes", "No");

        if (confirm)
        {
            // code
        }


    }



    private async void simpan()
    {
        //staffID sementara nanti ganti sama temp login
        string nama = "ValueXXX";
        var data = new Dictionary<string, string>
        {
            { "id_user", ID_USER }
                    
        };

        var jsonData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var client = new HttpClient();
        string ip = App.API_HOST + "kasir/closing.php";

        var response = await client.PostAsync(ip, jsonData);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

        if (responseObject["status"] == "success")
        {

            string total_pembayaran = responseObject["total_pembayaran"];
            string total_tunai = responseObject["total_tunai"];

            //jadikan 2 varible diatas berformat contoh rupiah Rp 500.000
            //buatkan pesan yang menyertakan 2 informasi diatas.
            //Kembali ke Login.xaml.cs
            //hapus semua References
        }

        else
        {
            await DisplayAlert("ERROR CLOSING", responseObject["message"], "OK");
        }
    }


}