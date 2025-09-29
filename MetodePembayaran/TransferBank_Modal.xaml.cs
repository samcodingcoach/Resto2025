using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Resto2025.MetodePembayaran;

public partial class TransferBank_Modal : Popup
{
    private readonly Action _onPopupClosed;
    public string selectedPaymentMethod = "";
    public string selectedBank = "";
    public string KODE_PAYMENT = "TES09111"; // Ini harus diisi dari luar
    public int transfer_or_edc = 0; // 0 untuk transfer, 1 untuk EDC
    private string imagePath = "";
    public TransferBank_Modal(Action onPopupClosed)
	{
		InitializeComponent();
        _onPopupClosed = onPopupClosed;
    }

    private string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        switch (ext)
        {
            case ".jpg":
           
            default:
                return "application/octet-stream";
        }
    }

    private void RadioTransfer_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value) // Jika RadioButton dipilih
        {
            RadioButton selectedRadio = sender as RadioButton;
            selectedPaymentMethod = selectedRadio.Content.ToString();
            transfer_or_edc = 0;
            System.Diagnostics.Debug.WriteLine("Selected Payment Method: " + selectedPaymentMethod);
        }
    }

    private void RadioEDC_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value) // Jika RadioButton dipilih
        {
            RadioButton selectedRadio = sender as RadioButton;
            selectedPaymentMethod = selectedRadio.Content.ToString();
            transfer_or_edc = 1;
            System.Diagnostics.Debug.WriteLine("Selected Payment Method: " + selectedPaymentMethod);
        }
    }

    private async void CloseModal_Tapped(object sender, TappedEventArgs e)
    {
        _onPopupClosed?.Invoke();
        Close();
    }

    private void PickerBank_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        if (picker != null)
        {
            selectedBank = picker.SelectedItem?.ToString() ?? "";
            System.Diagnostics.Debug.WriteLine("Selected Bank: " + selectedBank);
        }
    }

    private async void BTerapkanBank_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Validasi input
            if (string.IsNullOrEmpty(selectedBank))
            {
                await Shell.Current.DisplayAlert("Error", "Silakan pilih bank terlebih dahulu", "OK");
                return;
            }

            if (string.IsNullOrEmpty(EntryNamaPengirim.Text))
            {
                await Shell.Current.DisplayAlert("Error", "Silakan masukkan nama pengirim", "OK");
                return;
            }

            // Kirim data ke API
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{App.API_HOST}pembayaran/simpan_bank.php");
            var content = new MultipartFormDataContent();

            // Tambahkan parameter ke formulir
            content.Add(new StringContent(KODE_PAYMENT), "kode_payment"); // Ini seharusnya dinamis, tetapi dari contoh Postman
            content.Add(new StringContent(transfer_or_edc.ToString()), "transfer_or_edc");
            content.Add(new StringContent(selectedBank), "nama_bank");
            content.Add(new StringContent(EntryNamaPengirim.Text), "nama_pengirim");

            // Tambahkan nomor referensi jika ada
            if (!string.IsNullOrEmpty(EntryReferensi.Text))
            {
                content.Add(new StringContent(EntryReferensi.Text), "no_referensi");
            }
            else
            {
                content.Add(new StringContent(""), "no_referensi");
            }

            // Tambahkan file gambar jika dipilih
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                var stream = File.OpenRead(imagePath);
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(streamContent, "img_ss", Path.GetFileName(imagePath));
            }

            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(responseContent);

            // Parsing response
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
            
            if (responseObject.status == "success")
            {
                await Shell.Current.DisplayAlert("Sukses", responseObject.message, "OK");
                _onPopupClosed?.Invoke();
                Close();
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", $"Gagal menyimpan data: {responseObject.message}", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saat mengirim data: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Terjadi kesalahan: {ex.Message}", "OK");
        }
    }

    private async void BUploadBukti_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Pilih gambar bukti transfer"
            });

            if (result != null)
            {
                imagePath = result.FullPath;
                Debug.WriteLine($"File dipilih: {imagePath}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saat memilih file: {ex.Message}");
        }
    }
}