using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography;

namespace Resto2025.Akun;

public partial class Akun : ContentPage
{
    private List<list_promo> _listpromo;
    string ID_USER = "4";
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
        public string id_user { get; set; }= string.Empty;
        public string nama_lengkap { get; set; }= string.Empty;
        public string jabatan { get; set; } = string.Empty;
        public string nomor_hp { get; set; }= string.Empty;
        public string email { get;set; }= string.Empty;
        public string password { get; set; }= string.Empty;
        
    }

    public class list_promo
    {
        public string id_promo { get; set; } = string.Empty;
        public string nama_promo { get;set; } = string.Empty;
        public string kode_promo { get; set; }  = string.Empty ;
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
                else if(rowData[i].pilihan_promo == "nominal")
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

    private void B_Update_Clicked(object sender, EventArgs e)
    {

    }

    private void B_UbahPassword_Clicked(object sender, EventArgs e)
    {

    }

    private void B_Logout_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void TapPromo_Tapped(object sender, EventArgs e)
    {

    }
}