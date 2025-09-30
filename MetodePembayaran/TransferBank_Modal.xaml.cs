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
    private Stream _imageStream;  // Instance variable untuk image stream
    public string selectedBank = "";
    public string KODE_PAYMENT = "";
    public int transfer_or_edc = 0; // 0 untuk transfer, 1 untuk EDC

    private string direktori_lokal;  // Path foto dari kamera
    private string nama_file;  // Nama file foto


    public TransferBank_Modal(string kodePayment, double nominalTransfer, Action<bool, string> onResultReceived)
    {
        InitializeComponent();
        KODE_PAYMENT = kodePayment;
        nominal_transfer = nominalTransfer;
        _onResultReceived = onResultReceived;
    }

    //ambil foto dari camera
    public async Task TakePhoto()
    {
        try
        {
            // Request camera permission terlebih dahulu
            Debug.WriteLine("Meminta permission kamera...");
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                Debug.WriteLine("Permission belum granted, requesting...");
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status != PermissionStatus.Granted)
            {
                Debug.WriteLine("Permission kamera ditolak oleh user");
                return;
            }

            Debug.WriteLine("Permission kamera granted, membuka kamera...");

            if (MediaPicker.Default.IsCaptureSupported)
            {
                // Set options untuk menggunakan kamera belakang
                var options = new MediaPickerOptions
                {
                    Title = "Ambil Foto Bukti Transfer"
                };

                FileResult photo = await MediaPicker.Default.CapturePhotoAsync(options);

                if (photo != null)
                {
                    Debug.WriteLine($"Foto berhasil diambil: {photo.FileName}");
                    
                    string fileName = string.IsNullOrEmpty(photo.FileName)
                        ? $"IMG_{DateTime.Now:yyyyMMdd_HHmmss}.jpg"
                        : photo.FileName;

                    string localFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                    // Dispose stream lama jika ada
                    if (_imageStream != null)
                    {
                        _imageStream.Dispose();
                    }

                    // Baca foto dan compress dengan SkiaSharp
                    byte[] compressedBytes;
                    using (Stream tempStream = await photo.OpenReadAsync())
                    {
                        // Baca seluruh stream ke byte array
                        using (var memoryStream = new MemoryStream())
                        {
                            await tempStream.CopyToAsync(memoryStream);
                            byte[] originalBytes = memoryStream.ToArray();
                            
                            // Compress image dengan SkiaSharp
                            Debug.WriteLine("Memulai kompresi gambar...");
                            compressedBytes = CompressImage(originalBytes);
                            Debug.WriteLine($"Original size: {originalBytes.Length / 1024} KB, Compressed: {compressedBytes.Length / 1024} KB");
                        }
                    }

                    // Simpan compressed image ke MemoryStream untuk display
                    _imageStream = new MemoryStream(compressedBytes);
                    _imageStream.Position = 0;

                    // Simpan compressed file ke penyimpanan lokal
                    using (FileStream localFileStream = File.OpenWrite(localFilePath))
                    {
                        await localFileStream.WriteAsync(compressedBytes, 0, compressedBytes.Length);
                    }

                    if (string.IsNullOrEmpty(nama_file))
                    {
                        nama_file = $"IMG_{Guid.NewGuid()}.jpg";
                    }

                    string newLocalFilePath = Path.Combine(FileSystem.CacheDirectory, nama_file);

                    if (File.Exists(localFilePath))
                    {
                        if (File.Exists(newLocalFilePath))
                        {
                            File.Delete(newLocalFilePath);
                        }
                        File.Move(localFilePath, newLocalFilePath);
                    }

                    direktori_lokal = newLocalFilePath;
                    Debug.WriteLine($"File disimpan di: {direktori_lokal}");

                    // Buat salinan stream agar bisa digunakan di ImageSource
                    var imageStreamCopy = new MemoryStream();
                    _imageStream.Position = 0;
                    await _imageStream.CopyToAsync(imageStreamCopy);
                    imageStreamCopy.Position = 0;

                    // Gunakan salinan stream untuk ImageSource
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        T_Buktitransfer.Source = ImageSource.FromStream(() => new MemoryStream(imageStreamCopy.ToArray()));
                        T_Buktitransfer.IsVisible = true;
                        BUploadBukti.IsVisible = false;
                    });

                    Debug.WriteLine("Foto berhasil ditampilkan di UI");
                }
                else
                {
                    Debug.WriteLine("User membatalkan pengambilan foto");
                }
            }
            else
            {
                Debug.WriteLine("Kamera tidak didukung pada perangkat ini");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saat mengambil foto: {ex.Message}");
            Debug.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }



    // Compress image menggunakan SkiaSharp
    private byte[] CompressImage(byte[] originalBytes)
    {
        try
        {
            using (var bitmap = SKBitmap.Decode(originalBytes))
            {
                if (bitmap == null)
                {
                    Debug.WriteLine("Gagal decode bitmap, menggunakan original image");
                    return originalBytes;
                }

                int width = bitmap.Width;
                int height = bitmap.Height;

                // Resize image untuk mengurangi ukuran (scale 25% dari original)
                float scale = 0.25f; // Ubah nilai ini untuk mengatur ratio kompresi
                int newWidth = (int)(width * scale);
                int newHeight = (int)(height * scale);

                Debug.WriteLine($"Resizing dari {width}x{height} ke {newWidth}x{newHeight}");

                var resizedBitmap = new SKBitmap(newWidth, newHeight);
                using (var canvas = new SKCanvas(resizedBitmap))
                {
                    canvas.Clear(SKColors.White);
                    canvas.DrawBitmap(bitmap, new SKRect(0, 0, newWidth, newHeight), 
                        new SKPaint { FilterQuality = SKFilterQuality.High });
                }

                // Encode ke JPEG dengan kualitas 60%
                using (var image = SKImage.FromBitmap(resizedBitmap))
                {
                    var encodedData = image.Encode(SKEncodedImageFormat.Jpeg, 60);
                    return encodedData.ToArray();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saat compress image: {ex.Message}");
            Debug.WriteLine("Menggunakan original image tanpa kompresi");
            return originalBytes;
        }
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

            // Validasi bahwa nominal_transfer adalah angka yang valid (bukan NaN atau infinity)
            if (double.IsNaN(nominal_transfer) || double.IsInfinity(nominal_transfer))
            {
                _onResultReceived?.Invoke(false, "Nominal transfer tidak valid");
                Close();
                return;
            }

            // Tambahkan log untuk debugging nilai yang akan dikirim
            Debug.WriteLine($"Nominal Transfer sebelum dikirim: {nominal_transfer}");

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

            // Log nilai yang dikirim
            System.Diagnostics.Debug.WriteLine($"Kode Payment: {KODE_PAYMENT}");
            System.Diagnostics.Debug.WriteLine($"Transfer or EDC: {transfer_or_edc}");
            System.Diagnostics.Debug.WriteLine($"Nama Bank: {selectedBank}");
            System.Diagnostics.Debug.WriteLine($"Nama Pengirim: {EntryNamaPengirim.Text}");
            System.Diagnostics.Debug.WriteLine($"Nominal Transfer: {nominal_transfer}");

            // Tambahkan nomor referensi jika ada
            if (!string.IsNullOrEmpty(EntryReferensi.Text))
            {
                content.Add(new StringContent(EntryReferensi.Text), "no_referensi");
            }
            else
            {
                content.Add(new StringContent(""), "no_referensi");
            }

            // Upload file gambar dari kamera
            if (!string.IsNullOrEmpty(direktori_lokal) && File.Exists(direktori_lokal))
            {
                Debug.WriteLine($"Upload file dari kamera: {direktori_lokal}");
                var fileStream = File.OpenRead(direktori_lokal);
                var streamContent = new StreamContent(fileStream);

                // Menentukan contentType berdasarkan ekstensi file
                string mimeType = GetMimeType(direktori_lokal);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

                content.Add(streamContent, "img_ss", Path.GetFileName(direktori_lokal));
            }
            else
            {
                Debug.WriteLine("Tidak ada file gambar yang akan diupload");
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

   

    private async void Tap1_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        NavDataPembayaran.Opacity = 1;
        NavBuktiPembayaran.Opacity = 0.7;
        View_DataPembayaran.IsVisible = true;
        View_UploadBukti.IsVisible = false;

    }

    private async void Tap2_Tapped(object sender, TappedEventArgs e)
    {

        if (sender is Frame image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        NavDataPembayaran.Opacity = 0.7;
        NavBuktiPembayaran.Opacity = 1;
        View_DataPembayaran.IsVisible = false;
        View_UploadBukti.IsVisible = true;
    }

    private async void BUploadBukti_Clicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Tombol Upload Bukti Diklik");
        try
        {
            Debug.WriteLine("BUploadBukti_Clicked - Langsung buka kamera...");
            await TakePhoto();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saat BUploadBukti_Clicked: {ex.Message}");
            Debug.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }
}
