using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Resto2025.MetodePembayaran;

public partial class TransferBank_Modal : Popup
{
    private readonly Action _onPopupClosed;
    public string selectedPaymentMethod = "";
    public int transfer_or_edc = 0; // 0 untuk transfer, 1 untuk EDC
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
}