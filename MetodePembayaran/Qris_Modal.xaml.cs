using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using SkiaSharp;
using System.Text;
using Microsoft.Maui.ApplicationModel;
using System.Net.Http.Headers;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace Resto2025.MetodePembayaran;

public partial class Qris_Modal : Popup
{
    private readonly Action _onPopupClosed;
    private readonly Func<Task> _onGenerateQR;
	private readonly Action<string> _onQRReady;
	public double grossAmount = 0;
    
    // Countdown Timer
    private System.Timers.Timer _countdownTimer;
    private int _remainingSeconds = 300; // 5 menit = 300 detik
    public Qris_Modal(double grandTotal, Action onPopupClosed, Func<Task> onGenerateQR, Action<string> onQRReady = null)
	{
		InitializeComponent();
        grossAmount = grandTotal;
        L_grossAmount.Text = grossAmount.ToString("C0", new System.Globalization.CultureInfo("id-ID"));
        _onPopupClosed = onPopupClosed;
        _onGenerateQR = onGenerateQR;
        _onQRReady = onQRReady;
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
            
            // Mulai countdown timer
            StartCountdownTimer();
        }
    }

    private async void Button_CekStatus_Clicked(object sender, EventArgs e)
    {


        if (sender is Image image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }


    }
    
    private void StartCountdownTimer()
    {
        // Stop timer yang ada jika sedang berjalan
        StopCountdownTimer();
        
        // Reset countdown ke 5 menit
        _remainingSeconds = 300;
        
        // Update label pertama kali
        UpdateCountdownLabel();
        
        // Buat timer baru
        _countdownTimer = new System.Timers.Timer(1000); // 1 detik interval
        _countdownTimer.Elapsed += OnTimerElapsed;
        _countdownTimer.AutoReset = true;
        _countdownTimer.Start();
        
        Debug.WriteLine("Countdown timer dimulai - 5 menit (300 detik)");
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
        
        Debug.WriteLine($"Countdown update: {timeText}");
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
        Application.Current?.MainPage?.DisplayAlert("QR Expired", 
            "QR Code telah expired. Silakan generate QR baru.", "OK");
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
