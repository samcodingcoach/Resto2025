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
    string kode_payment = string.Empty;
    private bool _autoPrinted = false;
#if ANDROID
    private static readonly UUID SPP_UUID = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
#endif
    public Print1()
    {
        InitializeComponent();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        //panggil tes print

       
    }

    public Print1(string kodePayment) : this()
    {
        kode_payment = kodePayment ?? string.Empty;
    }

    public class ApiResponse
    {
        public bool success { get; set; }
        public Receipt receipt { get; set; }
    }

    public class Receipt
    {
        public Header header { get; set; }
        public List<ReceiptItem> items { get; set; }
    }

    public class Header
    {
        public string kode_payment { get; set; }
        public int id_pesanan { get; set; }
        public int? id_tagihan { get; set; }
        public string kategori { get; set; }
        public string nama_lengkap { get; set; }
        public string tanggal_payment { get; set; }
        public string kasir { get; set; }
        public string nama_konsumen { get; set; }
        public double grand_total { get; set; }
        public double jumlah_dibayarkan { get; set; }
        public double kembalian { get; set; }
        public double subtotal { get; set; }
        public double packing { get; set; }
        public double service { get; set; }
        public double promo { get; set; }
        public double ppn { get; set; }
        public string nilai_diskon { get; set; }
        public double diskon { get; set; }
        public string mode_pesanan { get; set; }
        public int nomor_antri { get; set; }
    }

    public class ReceiptItem
    {
        public string nama_produk { get; set; }
        public int qty { get; set; }
        public string mode_pesanan { get; set; }
        public double harga { get; set; }
        public double subtotal_produk { get; set; }
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

    public async Task<Receipt> FetchReceiptData(string kodePayment)
    {
        try
        {
            string url = App.API_HOST + "struk/print1.php?kode=" + kodePayment;
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response);
                
                if (apiResponse != null && apiResponse.success)
                {
                    return apiResponse.receipt;
                }
                else
                {
                    await ShowAlert("Error", "Gagal mengambil data struk");
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            await ShowAlert("Error", $"Error fetching data: {ex.Message}");
            return null;
        }
    }

    public async Task PrintReceipt(Receipt receiptData)
    {
        if (receiptData == null)
        {
            await ShowAlert("Error", "Data struk tidak tersedia");
            return;
        }

        StringBuilder strukBuilder = new StringBuilder();
        var header = receiptData.header;

        string FormatNominal(double value) => value.ToString("N0", new System.Globalization.CultureInfo("id-ID"));

        // Header Struk
        strukBuilder.Append(
            "\x1B\x21\x08" + "STRUK PEMBELIAN\n" + "\x1B\x21\x00" +
            $"No: {header.kode_payment} | {header.kategori}\n" +
            $"{header.tanggal_payment}. {header.kasir}\n\n" +
            "\x1B\x21\x08" + "RINCIAN\n" + "\x1B\x21\x00"
        );

        // Detail Produk dari API
        foreach (var item in receiptData.items)
        {
            strukBuilder.Append(
                $"{item.nama_produk}\n" +
                AlignRight($"{item.qty}x {FormatNominal(item.harga)}", FormatNominal(item.subtotal_produk)) + "\n\n"
            );
        }

        // Subtotal dan Total Harga
        strukBuilder.Append(
            "\x1B\x21\x08" + "SUBTOTAL\n" + "\x1B\x21\x00" +
            AlignRight("Produk", FormatNominal(header.subtotal)) + "\n"
        );

        if (header.packing > 0)
            strukBuilder.Append(AlignRight("Packing", FormatNominal(header.packing)) + "\n");
        
        if (header.service > 0)
            strukBuilder.Append(AlignRight("Service Charge", FormatNominal(header.service)) + "\n");
        
        if (header.ppn > 0)
            strukBuilder.Append(AlignRight("PPN Resto", FormatNominal(header.ppn)) + "\n");
        
        if (header.promo > 0)
            strukBuilder.Append(AlignRight("Promo", "-" + FormatNominal(header.promo)) + "\n");
        
      

        strukBuilder.Append(
            "\n--------------------------------\n" +
            "\x1B\x21\x08" + "TOTAL HARGA\n" +
            "\x1B\x61\x01" +
            "\x1D\x21\x11" + FormatNominal(header.jumlah_dibayarkan) + "\n" +
            "\x1D\x21\x00" +
            "\x1B\x61\x00" +
            "--------------------------------\n" +
            AlignRight(header.kategori, FormatNominal(header.grand_total)) + "\n" +
            AlignRight("Kembalian", FormatNominal(header.kembalian)) + "\n" +
            $"Kasir: {header.kasir}\n" +
            $"Konsumen: {header.nama_konsumen}\n" +
            $"Mode: {header.mode_pesanan}\n\n" +
            "\x1B\x21\x08" + "Terimakasih atas\npembayaran Anda!\n" + "\x1B\x21\x00" +
            $"{header.tanggal_payment}" + "\n\n\n"
        );

        string struk = strukBuilder.ToString();
        byte[] buffer = Encoding.GetEncoding(437).GetBytes(struk);

#if ANDROID
        await Print("RPP02N", buffer);
#else
        await ShowAlert("Error", "Bluetooth hanya didukung di Android!");
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_autoPrinted && !string.IsNullOrWhiteSpace(kode_payment))
        {
            _autoPrinted = true;
            var receiptData = await FetchReceiptData(kode_payment);
            if (receiptData != null)
            {
                await PrintReceipt(receiptData);
            }
        }
    }

    public async Task PrintByKodeAsync(string kodePayment)
    {
        if (string.IsNullOrWhiteSpace(kodePayment)) return;
        var receiptData = await FetchReceiptData(kodePayment);
        if (receiptData != null)
        {
            await PrintReceipt(receiptData);
        }
    }

#if ANDROID
    public async Task Print(string deviceName, byte[] buffer)
    {
        BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        if (bluetoothAdapter == null || !bluetoothAdapter.IsEnabled)
        {
            await ShowAlert("Error", "Bluetooth tidak tersedia atau belum diaktifkan.");
            return;
        }

        BluetoothDevice device = bluetoothAdapter.BondedDevices?.FirstOrDefault(bd => bd.Name == deviceName);
        if (device == null)
        {
            await ShowAlert("Error", "Printer tidak ditemukan.");
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
            await ShowAlert("Error", ex.Message);
        }
    }


#endif

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(kode_payment))
        {
            await ShowAlert("Error", "Kode payment tidak tersedia");
            return;
        }

        var receiptData = await FetchReceiptData(kode_payment);
        if (receiptData != null)
        {
            await PrintReceipt(receiptData);
        }
    }

    private async Task ShowAlert(string title, string message)
    {
        try
        {
            var page = Application.Current?.MainPage;
            if (page != null)
            {
                await page.DisplayAlert(title, message, "OK");
            }
            else
            {
                Debug.WriteLine($"ALERT [{title}]: {message}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to show alert: {ex.Message}");
        }
    }


}