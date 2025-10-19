using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using Newtonsoft.Json;

#if ANDROID
using Android.Bluetooth;
using Java.Util;
#endif

namespace Resto2025.Struk;

public partial class Print1 : ContentPage
{
#if ANDROID
    private static readonly UUID SPP_UUID = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
#endif
    public Print1()
    {
        InitializeComponent();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        //panggil tes print

       
    }

    public string AlignRight(string label, string value, int totalLength = 32)
    {
        int spacing = totalLength - (label.Length + value.Length);
        return label + new string(' ', spacing) + value;
    }

    public string CenterText(string text, int totalLength)
    {
        int padding = (totalLength - text.Length) / 2;
        return new string(' ', padding) + text + new string(' ', padding);
    }

    public async Task PrintReceipt()
    {
        StringBuilder strukBuilder = new StringBuilder();

        // Helper untuk format nominal
        string FormatNominal(decimal value) => value.ToString("N0", new System.Globalization.CultureInfo("id-ID"));

        // Header Struk
        strukBuilder.Append(
            "\x1B\x21\x08" + "STRUK PEMBELIAN\n" + "\x1B\x21\x00" +
            "No: INV-20251019 | Makanan\n" +
            "19-10-2025. Samsu Bahri\n\n" +
            "\x1B\x21\x08" + "RINCIAN\n" + "\x1B\x21\x00"
        );

        // Detail Produk (statis dengan format nominal)
        strukBuilder.Append(
            "Nasi Goreng Spesial\n" +
            AlignRight("2x 25.000", FormatNominal(50_000m)) + "\n\n" +

            "Es Teh Manis\n" +
            AlignRight("1x 8.000", FormatNominal(8_000m)) + "\n\n"
        );

        // Subtotal dan Total Harga
        strukBuilder.Append(
            "\x1B\x21\x08" + "SUBTOTAL\n" + "\x1B\x21\x00" +
            AlignRight("Produk", FormatNominal(58_000m)) + "\n" +
            AlignRight("Take Away", FormatNominal(2_000m)) + "\n" +
            AlignRight("Service Charge", FormatNominal(3_000m)) + "\n" +
            AlignRight("PPN Resto", FormatNominal(5_800m)) + "\n" +
            AlignRight("Promo", FormatNominal(-5_000m)) + "\n\n" +
            "--------------------------------\n" +
            "\x1B\x21\x08" + "TOTAL HARGA\n" +  // Bold
            "\x1B\x61\x01" +  // Center
            "\x1D\x21\x11" + FormatNominal(63_800m) + "\n" +  // Font lebih besar
            "\x1D\x21\x00" +  // Reset ukuran normal
            "\x1B\x61\x00" +  // Reset align kiri
            "--------------------------------\n" +
            AlignRight("Cash", FormatNominal(100_000m)) + "\n" +
            AlignRight("Kembalian", FormatNominal(36_200m)) + "\n" +
            "Kasir: Admin\n\n" +
            "\x1B\x21\x08" + "Terimakasih atas\npembayaran Anda!\n" + "\x1B\x21\x00" +
            "19-10-2025 15:30:45" + "\n\n\n"
        );

        string struk = strukBuilder.ToString();
        byte[] buffer = Encoding.GetEncoding(437).GetBytes(struk);

#if ANDROID
        await Print("RPP02N", buffer);
#else
    await DisplayAlert("Error", "Bluetooth hanya didukung di Android!", "OK");
#endif
    }

#if ANDROID
    public async Task Print(string deviceName, byte[] buffer)
    {
        BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        if (bluetoothAdapter == null || !bluetoothAdapter.IsEnabled)
        {
            await DisplayAlert("Error", "Bluetooth tidak tersedia atau belum diaktifkan.", "OK");
            return;
        }

        BluetoothDevice device = bluetoothAdapter.BondedDevices?.FirstOrDefault(bd => bd.Name == deviceName);
        if (device == null)
        {
            await DisplayAlert("Error", "Printer tidak ditemukan.", "OK");
            return;
        }

        try
        {
            await Task.Delay(500); // Stabilkan koneksi
            using (BluetoothSocket bluetoothSocket = device.CreateRfcommSocketToServiceRecord(SPP_UUID))
            {
                bluetoothSocket.Connect();

                for (int i = 0; i < buffer.Length; i += 512)
                {
                    int size = Math.Min(512, buffer.Length - i);
                    bluetoothSocket.OutputStream.Write(buffer, i, size);
                    await Task.Delay(10); // Kurangi delay agar cepat
                }

                bluetoothSocket.OutputStream.Flush();
                bluetoothSocket.Close();
                Debug.WriteLine("Print berhasil.");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await PrintReceipt();
    }
#endif
}