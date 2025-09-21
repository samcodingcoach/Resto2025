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

public partial class TransferBank_Modal : Popup
{
    private readonly Action _onPopupClosed;
    public string selectedPaymentMethod = "";
    public TransferBank_Modal(Action onPopupClosed)
	{
		InitializeComponent();
        _onPopupClosed = onPopupClosed;
    }

    private void RadioTransfer_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {

    }

    private void RadioEDC_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {

    }

    private async void CloseModal_Tapped(object sender, TappedEventArgs e)
    {
        _onPopupClosed?.Invoke();
        Close();
    }
}