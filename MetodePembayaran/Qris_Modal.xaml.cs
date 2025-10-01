using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Resto2025.MetodePembayaran;

public partial class Qris_Modal : Popup
{
    private readonly Action _onPopupClosed;
    private readonly Func<Task> _onGenerateQR;
	private readonly Action<string> _onQRReady;
    private string QR_STATUS = string.Empty;
	public double grossAmount = 0;
    
    // Kode Payment
    private string kode_payment = string.Empty;
    
    // Countdown Timer
    private System.Timers.Timer _countdownTimer;
    private int _remainingSeconds = 420; 
    public Qris_Modal(double grandTotal, string kodePayment, Action onPopupClosed, Func<Task> onGenerateQR, Action<string> onQRReady = null)
    {
        InitializeComponent();
        grossAmount = grandTotal;
        kode_payment = kodePayment;
        L_grossAmount.Text = grossAmount.ToString("C0", new System.Globalization.CultureInfo("id-ID"));
        _onPopupClosed = onPopupClosed;
        _onGenerateQR = onGenerateQR;
        _onQRReady = onQRReady;
    }

    public class status_pembayaran
    {
        public string order_id { get; set; } = string.Empty; 
        public string gross_amount { get; set; } = string.Empty; 
        public string transaction_status { get; set; } = string.Empty;
        public string settlement_time { get; set; } = string.Empty;
    }

    private async Task cek_status_pembayaran()
    {
        try
        {
            string url = App.API_HOST + "midtrans/midtrans_status.php?kode_payment=" + kode_payment;
            Debug.WriteLine($"Cek status pembayaran URL: {url}");

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Response JSON: {json}");

                    // Handle berbagai format response
                    bool parseSuccess = await TryParseStatusResponse(json);
                    
                    if (parseSuccess && QR_STATUS == "SETTLEMENT")
                    {
                        Debug.WriteLine("Status SETTLEMENT, memperbarui status pembayaran...");
                        settlement_update();
                    }
                    else if (parseSuccess)
                    {
                        Debug.WriteLine($"Status belum SETTLEMENT: {QR_STATUS}");
                    }
                    else
                    {
                        Debug.WriteLine("Gagal memparse response atau data kosong");
                    }
                }
                else
                {
                    Debug.WriteLine($"HTTP Error ({response.StatusCode}): {response.ReasonPhrase}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Terjadi kesalahan: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    // Method tambahan untuk handle berbagai format response
    private async Task<bool> TryParseStatusResponse(string json)
    {
        try
        {
            Debug.WriteLine($"Mencoba parse JSON: {json}");

            // Cek jika response adalah error object
            if (json.TrimStart().StartsWith("{") && json.Contains("error"))
            {
                var errorObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (errorObj.ContainsKey("error"))
                {
                    Debug.WriteLine($"Server error: {errorObj["error"]}");
                    return false;
                }
            }

            // Coba deserialize sebagai array dulu (sesuai format yang Anda berikan)
            if (json.TrimStart().StartsWith("["))
            {
                var arrayData = JsonConvert.DeserializeObject<List<status_pembayaran>>(json);
                if (arrayData != null && arrayData.Count > 0)
                {
                    var row = arrayData[0];
                    QR_STATUS = row.transaction_status.ToUpper();
                    Debug.WriteLine($"Status dari array: {QR_STATUS}");
                    return true;
                }
            }
            // Jika bukan array, coba deserialize sebagai single object
            else if (json.TrimStart().StartsWith("{"))
            {
                var singleData = JsonConvert.DeserializeObject<status_pembayaran>(json);
                if (singleData != null && !string.IsNullOrEmpty(singleData.transaction_status))
                {
                    QR_STATUS = singleData.transaction_status.ToUpper();
                    Debug.WriteLine($"Status dari object: {QR_STATUS}");
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error parsing response: {ex.Message}");
            return false;
        }
    }

    private async void settlement_update()
    {
        Debug.WriteLine($"Memperbarui status pembayaran menjadi SETTLEMENT: {kode_payment}");
        
        if (string.IsNullOrEmpty(kode_payment))
        {
            
            Debug.WriteLine("Kode payment kosong, tidak dapat memperbarui status pembayaran.");
            return;
        }
        var data = new Dictionary<string, string>
        {
                { "kode_payment", kode_payment },
                     
        };

        var jsonData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var client = new HttpClient();
        string ip = App.API_HOST + "pembayaran/settlement_update.php";

        var response = await client.PostAsync(ip, jsonData);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

        if (responseObject["status"] == "success")
        {

            //tutup modal

        }
        else
        {
           System.Diagnostics.Debug.WriteLine("Gagal memperbarui status pembayaran.");
        }
    }

    private void CheckSetuju_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value == true)
        {
            BGenerateQR.IsEnabled = e.Value;
            BGenerateQR.Opacity = 1; 
        }
        else
        {
            BGenerateQR.IsEnabled = e.Value;
            BGenerateQR.Opacity = 0.3;
        }
    }

    private async void CloseModal_Tapped(object sender, TappedEventArgs e)
    {
        // Stop countdown timer saat popup ditutup
        StopCountdownTimer();
        
        _onPopupClosed?.Invoke();
        Close();
    }

    private async void BGenerateQR_Clicked(object sender, EventArgs e)
    {

        if (sender is Image image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        // Panggil ProsesDanSimpanTransaksiAsync dari ProdukMenu
        if (_onGenerateQR != null)
        {
            await _onGenerateQR();
        }
    }
    
    public void SetQRCode(string qrCodeUrl)
    {
        if (!string.IsNullOrEmpty(qrCodeUrl))
        {
            QrisWebView.Source = ImageSource.FromUri(new Uri(qrCodeUrl));
            
            // Tampilkan countdown UI
            HitungMundur.IsVisible = true;
            BGenerateQR.IsEnabled = false; BGenerateQR.Opacity=0.3;

            // Mulai countdown timer
            StartCountdownTimer();
        }
    }

    // Method untuk update kode payment setelah di-generate
    public void SetKodePayment(string kodePayment)
    {
        kode_payment = kodePayment;
        Debug.WriteLine($"Kode payment di-update di Qris_Modal: {kode_payment}");
    }

    private async void Button_CekStatus_Clicked(object sender, EventArgs e)
    {


        if (sender is Image image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        await cek_status_pembayaran();
    }
    
    private void StartCountdownTimer()
    {
        // Stop timer yang ada jika sedang berjalan
        StopCountdownTimer();
        
        // Reset countdown ke 7 menit
        _remainingSeconds = 420;
        
        // Update label pertama kali
        UpdateCountdownLabel();
        
        // Buat timer baru
        _countdownTimer = new System.Timers.Timer(1000); // 1 detik interval
        _countdownTimer.Elapsed += OnTimerElapsed;
        _countdownTimer.AutoReset = true;
        _countdownTimer.Start();
        
        Debug.WriteLine("Countdown timer dimulai - 7 menit (420 detik)");
    }
    
    private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        _remainingSeconds--;
        
        // Update UI di main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateCountdownLabel();
            
            // Cek apakah waktu habis
            if (_remainingSeconds <= 0)
            {
                OnTimerExpired();
            }
        });
    }
    
    private void UpdateCountdownLabel()
    {
        int minutes = _remainingSeconds / 60;
        int seconds = _remainingSeconds % 60;
        
        string timeText = $"QR EXPIRED DALAM {minutes:D2}:{seconds:D2}";
        Label_HitungMundur.Text = timeText;
        
       // Debug.WriteLine($"Countdown update: {timeText}");
    }
    
    private void OnTimerExpired()
    {
        Debug.WriteLine("QR Code expired - timer habis");
        
        // Stop timer
        StopCountdownTimer();
        
        // Update label untuk menunjukkan expired
        Label_HitungMundur.Text = "QR CODE EXPIRED";
        Label_HitungMundur.TextColor = Colors.Red;
        
        // Sembunyikan QR code
        QrisWebView.IsVisible = false;
        
        // Show alert
        Debug.WriteLine("QR Code telah expired. Silakan generate QR baru.");
    }
    
    private void StopCountdownTimer()
    {
        if (_countdownTimer != null)
        {
            _countdownTimer.Stop();
            _countdownTimer.Dispose();
            _countdownTimer = null;
            Debug.WriteLine("Countdown timer dihentikan");
        }
    }

}
