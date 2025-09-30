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
        }
    }
}