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
    private readonly Action<bool, string> _onResultReceived; // Parameter: (isSuccess, message)
    public double nominal_transfer = 0;
    public string selectedPaymentMethod = "";
    public string selectedBank = "";
    public string KODE_PAYMENT = "";
    public int transfer_or_edc = 0; // 0 untuk transfer, 1 untuk EDC
    private string imagePath = "";
    public TransferBank_Modal(string kodePayment,double nominalTransfer, Action<bool, string> onResultReceived)
	{
		InitializeComponent();
        KODE_PAYMENT = kodePayment;
        nominal_transfer = nominalTransfer;
        _onResultReceived = onResultReceived;
    }

    private string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        switch (ext)
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".png":
                return "image/png";
            case ".gif":
                return "image/gif";
            case ".bmp":
                return "image/bmp";
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
        // Tampilkan konfirmasi sebelum menutup modal
        bool confirm = await Shell.Current.DisplayAlert("Konfirmasi", "Anda belum menyelesaikan proses transfer. Apakah Anda yakin ingin menutup modal ini?", "Ya", "Batal");
        
        if (confirm)
        {
            // Jika pengguna yakin, tutup modal
            _onResultReceived?.Invoke(false, "Pengguna menutup modal tanpa menyelesaikan proses transfer");
            Close();
        }
        // Jika tidak, biarkan modal tetap terbuka
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
            if (string.IsNullOrEmpty(KODE_PAYMENT))
            {
                _onResultReceived?.Invoke(false, "Kode payment belum diisi");
                Close();
                return;
            }

            if (string.IsNullOrEmpty(selectedBank))
            {
                _onResultReceived?.Invoke(false, "Silakan pilih bank terlebih dahulu");
                Close();
                return;
            }

            if (string.IsNullOrEmpty(EntryNamaPengirim.Text))
            {
                _onResultReceived?.Invoke(false, "Silakan masukkan nama pengirim");
                Close();
                return;
            }

            // Kirim data ke API
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{App.API_HOST}pembayaran/simpan_bank.php");
            var content = new MultipartFormDataContent();

            // Tambahkan parameter ke formulir
            content.Add(new StringContent(KODE_PAYMENT), "kode_payment");
            content.Add(new StringContent(transfer_or_edc.ToString()), "transfer_or_edc");
            content.Add(new StringContent(selectedBank), "nama_bank");
            content.Add(new StringContent(EntryNamaPengirim.Text), "nama_pengirim");
            content.Add(new StringContent(nominal_transfer.ToString()), "nominal_transfer");

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
                
                // Menentukan contentType berdasarkan ekstensi file
                string mimeType = GetMimeType(imagePath);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                
                content.Add(streamContent, "img_ss", Path.GetFileName(imagePath));
            }

            request.Content = content;
            var response = await client.SendAsync(request);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(responseContent);

            if (response.IsSuccessStatusCode)
            {
                // Parsing response hanya jika status sukses
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                
                if (responseObject != null && responseObject.status != null && responseObject.status == "success")
                {
                    // Kirim hasil sukses ke halaman sebelumnya
                    _onResultReceived?.Invoke(true, responseObject.message?.ToString() ?? "Data pembayaran berhasil disimpan");
                    Close();
                }
                else
                {
                    // Kirim hasil error ke halaman sebelumnya
                    string errorMessage = responseObject?.message != null ? responseObject.message.ToString() : "Gagal menyimpan data";
                    _onResultReceived?.Invoke(false, errorMessage);
                    Close();
                }
            }
            else
            {
                // Tangani kasus error response
                var errorResponseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                string errorMessage = errorResponseObject?.message?.ToString() ?? $"Error: {response.StatusCode} - Gagal menyimpan data";
                
                _onResultReceived?.Invoke(false, errorMessage);
                Close();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saat mengirim data: {ex.Message}");
            _onResultReceived?.Invoke(false, $"Terjadi kesalahan: {ex.Message}");
            Close();
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