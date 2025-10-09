using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

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
    
    // QR Code URL untuk Telegram
    private string _qrCodeUrl = string.Empty;
    
    // Telegram Bot Configuration
    private readonly string _telegramBotToken = App.API_TELEGRAM;
    private readonly string _telegramChatId = "-4887355399";
    
    // Countdown Timer
    private System.Timers.Timer _countdownTimer;
    private int _remainingSeconds = 420;
    
    // Auto Check Status Timer
    private System.Timers.Timer _autoCheckTimer;
    private int _checkStatusSeconds = 10;
    
    // Success Countdown Timer
    private System.Timers.Timer _successTimer;
    private int _successCountdown = 5;
    
    // Store settlement data
    private status_pembayaran _settlementData; 
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
                        
                        // Simpan settlement data untuk ditampilkan nanti
                        bool parseSuccess2 = await TryParseStatusResponse(json);
                        
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
                    _settlementData = row; // Simpan data settlement
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
                    _settlementData = singleData; // Simpan data settlement
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
            // Debug info pembayaran sukses
            if (_settlementData != null)
            {
                Debug.WriteLine($"=== PEMBAYARAN QRIS SUKSES ===");
                Debug.WriteLine($"Order ID: {_settlementData.order_id}");
                Debug.WriteLine($"Gross Amount: {_settlementData.gross_amount}");
                Debug.WriteLine($"Transaction Status: {_settlementData.transaction_status}");
                Debug.WriteLine($"Settlement Time: {_settlementData.settlement_time}");
                Debug.WriteLine($"===============================");
            }
            
            // Stop auto check timer dan QR expired timer
            StopAutoCheckTimer();
            StopCountdownTimer();
            
            // Tampilkan info sukses di UI dan mulai countdown 5 detik untuk tutup modal
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ShowSuccessAndStartCloseCountdown();
            });
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
        // Stop semua timer saat popup ditutup
        StopCountdownTimer();
        StopAutoCheckTimer();
        
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

        // Tampilkan Activity Indicator
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

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
            // Sembunyikan Activity Indicator
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            
            // Simpan URL QR code untuk pengiriman Telegram
            _qrCodeUrl = qrCodeUrl;
            QrisWebView.Source = ImageSource.FromUri(new Uri(qrCodeUrl));
            
            //jika gambar muncul, tunggu kode payment terisi lalu kirim ke Telegram
            
            // Tampilkan countdown UI
            HitungMundur.IsVisible = true;
            BGenerateQR.IsEnabled = false; BGenerateQR.Opacity=0.3;

            // Mulai countdown timer dan auto check status
            StartCountdownTimer();
            StartAutoCheckTimer();
        }
    }

    // Method untuk update kode payment setelah di-generate
    public void SetKodePayment(string kodePayment)
    {
        kode_payment = kodePayment;
        Debug.WriteLine($"Kode payment di-update di Qris_Modal: {kode_payment}");
        
        // Kirim QR Code ke Telegram setelah kode payment terisi
        if (!string.IsNullOrEmpty(_qrCodeUrl))
        {
            Debug.WriteLine($"Mengirim QR Code ke Telegram dengan kode payment: {kode_payment}");
            _ = Task.Run(async () => await SendQRCodeToTelegramAsync(_qrCodeUrl));
        }
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
    
    private void StartAutoCheckTimer()
    {
        // Stop timer yang ada jika sedang berjalan
        StopAutoCheckTimer();
        
        // Reset countdown ke 10 detik
        _checkStatusSeconds = 10;
        
        // Update button text pertama kali
        UpdateCheckStatusButton();
        
        // Buat timer baru
        _autoCheckTimer = new System.Timers.Timer(1000); // 1 detik interval
        _autoCheckTimer.Elapsed += OnAutoCheckTimerElapsed;
        _autoCheckTimer.AutoReset = true;
        _autoCheckTimer.Start();
        
        Debug.WriteLine("Auto check status timer dimulai - 10 detik interval");
    }
    
    private void OnAutoCheckTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        _checkStatusSeconds--;
        
        // Update UI di main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateCheckStatusButton();
            
            // Cek apakah waktu habis (auto click)
            if (_checkStatusSeconds <= 0)
            {
                // Auto click CEK STATUS
                _ = Task.Run(async () => await cek_status_pembayaran());
                
                // Reset countdown ke 10 detik untuk interval berikutnya
                _checkStatusSeconds = 10;
            }
        });
    }
    
    private void UpdateCheckStatusButton()
    {
        string buttonText = $"CEK STATUS ({_checkStatusSeconds}s)";
        Button_CekStatus.Text = buttonText;
    }
    
    private void StopAutoCheckTimer()
    {
        if (_autoCheckTimer != null)
        {
            _autoCheckTimer.Stop();
            _autoCheckTimer.Dispose();
            _autoCheckTimer = null;
            Debug.WriteLine("Auto check timer dihentikan");
        }
    }
    
    private void ShowSuccessAndStartCloseCountdown()
    {
        // Tampilkan info settlement di label
        if (_settlementData != null)
        {
            Label_HitungMundur.Text = $"{_settlementData.transaction_status.ToUpper()} - {_settlementData.settlement_time}";
            Label_HitungMundur.TextColor = Colors.Green;
        }
        else
        {
            Label_HitungMundur.Text = "PEMBAYARAN BERHASIL";
            Label_HitungMundur.TextColor = Colors.Green;
        }
        
        // Sembunyikan button CEK STATUS
        Button_CekStatus.IsVisible = false;
        
        // Start success countdown timer
        _successCountdown = 5;
        _successTimer = new System.Timers.Timer(1000); // 1 detik
        _successTimer.Elapsed += OnSuccessTimerElapsed;
        _successTimer.AutoReset = true;
        _successTimer.Start();
        
        Debug.WriteLine("Success countdown dimulai - 5 detik");
    }
    
    private void OnSuccessTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        _successCountdown--;
        
        // Update UI di main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_settlementData != null)
            {
                Label_HitungMundur.Text = $"{_settlementData.transaction_status.ToUpper()} - Tutup dalam {_successCountdown}s";
            }
            else
            {
                Label_HitungMundur.Text = $"PEMBAYARAN BERHASIL - Tutup dalam {_successCountdown}s";
            }
            
            // Tutup modal setelah countdown habis
            if (_successCountdown <= 0)
            {
                _successTimer?.Stop();
                _successTimer?.Dispose();
                _successTimer = null;
                
                // Tutup modal
                _onPopupClosed?.Invoke();
                Close();
            }
        });
    }

    private async Task<byte[]> DownloadImageFromUrl(string imageUrl)
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error downloading image: {ex.Message}");
            return null;
        }
    }

    private async Task SendQRCodeToTelegramAsync(string qrCodeUrl)
    {
        try
        {
            Debug.WriteLine($"Starting Telegram send - Token: {_telegramBotToken?.Substring(0, 10)}..., ChatID: {_telegramChatId}");
            
            if (string.IsNullOrEmpty(_telegramBotToken) || _telegramBotToken == "YOUR_BOT_TOKEN_HERE" ||
                string.IsNullOrEmpty(_telegramChatId) || _telegramChatId == "YOUR_CHAT_ID_HERE")
            {
                Debug.WriteLine("Telegram Bot Token or Chat ID not configured");
                return;
            }

            var botClient = new TelegramBotClient(_telegramBotToken);
            Debug.WriteLine("Bot client created successfully");
            
            // Download QR code image
            Debug.WriteLine($"Downloading QR code from: {qrCodeUrl}");
            var imageBytes = await DownloadImageFromUrl(qrCodeUrl);
            if (imageBytes == null)
            {
                Debug.WriteLine("Failed to download QR code image");
                return;
            }

            Debug.WriteLine($"QR code downloaded successfully, size: {imageBytes.Length} bytes");
            
            // Send photo to Telegram
            using var ms = new MemoryStream(imageBytes);
            Debug.WriteLine("Sending photo to Telegram...");
            await botClient.SendPhoto(
                chatId: long.Parse(_telegramChatId),
                photo: new Telegram.Bot.Types.InputFileStream(ms, "qrcode.png"),
                caption: $"QR Code Pembayaran\nKode Payment: {kode_payment}\nAmount: Rp {grossAmount:N0}",
                parseMode: ParseMode.Html
            );

            Debug.WriteLine("QR Code successfully sent to Telegram");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error sending QR Code to Telegram: {ex.Message}");
        }
    }

}
