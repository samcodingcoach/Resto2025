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

    public Qris_Modal(Action onPopupClosed)
	{
		InitializeComponent();
        _onPopupClosed = onPopupClosed;
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
}