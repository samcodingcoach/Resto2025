using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
namespace Resto2025.Beranda;

public partial class Dashboard : ContentPage
{
    string ID_USER = Preferences.Get("ID_USER", string.Empty);
    private List<list_order> _listorder; private List<list_invoice> _listinvoice;
    private List<list_batal> _listbatal; 
    public Dashboard()
    {
        InitializeComponent();

        get_summary();

        _listorder = new List<list_order>(); // taruh di public load 
       
      

        _listinvoice = new List<list_invoice>();
        _listbatal = new List<list_batal>();


    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        get_summary();
        lv_dapurorder.ItemsSource = null;
        get_listorder();
        lv_invoice.ItemsSource = null;
        get_listinvoice();
        lv_batal.ItemsSource = null;
        get_listbatal();
    }

    public class list_order
    {
        public string id_order { get; set; } = string.Empty;
        public string kode_payment { get; set; } = string.Empty;
        public string waktu_terima { get; set; } = string.Empty;
        public string id_tagihan { get; set; } = string.Empty;
        public int total_item { get; set; } = 0;
        public int siap { get; set; } = 0;
        public int tersaji { get; set; } = 0;
        public int batal { get; set; } = 0;
        public int nomor_antri { get; set; } = 0;

        // Hanya jam-menit dari waktu_terima (contoh: 14:07)
        public string waktu_terima_jam
        {
            get
            {
                if (TryParseDateTime(waktu_terima, out var dt))
                    return dt.ToLocalTime().ToString("HH:mm");
                return waktu_terima;
            }
        }

        // Selisih menit dari sekarang terhadap waktu_terima (output hanya angka menit)
        public string jumlah_menit
        {
            get
            {
                if (TryParseDateTime(waktu_terima, out var dt))
                {
                    var minutes = (int)Math.Max(0, (DateTime.Now - dt.ToLocalTime()).TotalMinutes);
                    return minutes.ToString();
                }
                return "0";
            }
        }

        public string jumlah_menit_tampil
        {
            get
            {
                if (TryParseDateTime(waktu_terima, out var dt))
                {
                    var minutes = (int)Math.Max(0, (DateTime.Now - dt.ToLocalTime()).TotalMinutes);
                    return minutes <= 60 ? $"{minutes} menit" : string.Empty;
                }
                return string.Empty;
            }
        }

        public bool jumlah_menit_is_visible
        {
            get
            {
                if (TryParseDateTime(waktu_terima, out var dt))
                {
                    var minutes = (int)Math.Max(0, (DateTime.Now - dt.ToLocalTime()).TotalMinutes);
                    return minutes <= 60;
                }
                return false;
            }
        }

        private static bool TryParseDateTime(string value, out DateTime dt)
        {
            // Coba beberapa format umum, fallback ke DateTime.TryParse
            var formats = new[]
            {
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd HH:mm",
                "yyyy/MM/dd HH:mm:ss",
                "dd/MM/yyyy HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssZ"
            };

            foreach (var fmt in formats)
            {
                if (DateTime.TryParseExact(value, fmt, System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeLocal | System.Globalization.DateTimeStyles.AllowWhiteSpaces,
                        out dt))
                {
                    return true;
                }
            }

            return DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeLocal | System.Globalization.DateTimeStyles.AllowWhiteSpaces, out dt);
        }
    }

    public class list_invoice
    {
        public string tgl { get; set; } = string.Empty;
        public string kode_payment { get; set; } = string.Empty;
        public string nama_konsumen { get; set; } = string.Empty;
        public string id_meja { get; set; } = string.Empty;
        public double total_cart { get; set; } = 0;
        public string total_cart_string { get;set; } = string.Empty;
        public string id_tagihan { get; set; } = string.Empty;

    }

    public class list_summary
    {
        public string tanggal_open { get; set; } = string.Empty;
        public double qris { get; set; } = 0;
        public double transfer { get; set; } = 0;
        public double total_transaksi { get; set; } = 0;
        public double kas_awal { get; set; } = 0;
        public double tunai { get; set; } = 0;
        public int jumlah_transaksi { get; set; } = 0;

    }

    

    public class list_batal
    {
        public string id_batal { get; } = string.Empty;
        public string waktu { get; set; } = string.Empty;
        public string alasan { get; set; } = string.Empty;
        public int qty { get; set; } = 0;
        public string status_dapur { get; set; } = string.Empty;
        public string ta_dinein { get; set; } = string.Empty;
        public double harga_jual { get; set; } = 0;
        public string harga_jual_string { get; set; } = string.Empty;
        public string kode_produk { get; set; } = string.Empty;
        public string url_gambar
        {
            get => $"{App.IMAGE_HOST}/{kode_produk}.jpg";
          
        }
        public string nama_produk { get; set; } = string.Empty;
        public string nama_kategori { get; set; } = string.Empty;
        public string id_meja { get; set; } = string.Empty;
        public string id_tagihan { get; set; } = string.Empty;
        public string kode_payment { get; set; } = string.Empty;
    }

    private async void get_listbatal()
    {
        string url = App.API_HOST + "dapur/list_batal.php";
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            List<list_batal> rowData = JsonConvert.DeserializeObject<List<list_batal>>(json);

            _listbatal.Clear();


            for (int i = 0; i < rowData.Count; i++)
            {
                rowData[i].harga_jual_string = FormatCurrency(rowData[i].harga_jual);
                _listbatal.Add(rowData[i]);
            }

            int total = rowData.Count;
            lv_batal.ItemsSource = _listbatal;
            if(total>0) 
            {
              FrameBorder_Batal.IsVisible = true;
            }
           
        }
        else
        {

        }
    }

    private async void get_summary()
    {
        try
        {
            string url = App.API_HOST + "kasir/summary.php?id_user=" + ID_USER;

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    List<list_summary> rowData = JsonConvert.DeserializeObject<List<list_summary>>(json);

                    if (rowData != null && rowData.Count > 0)
                    {
                        list_summary row = rowData[0];
                        System.Diagnostics.Debug.WriteLine(row.kas_awal.ToString());
                        L_JumlahTransaksi.Text = row.jumlah_transaksi.ToString(); // Jumlah transaksi doesn't need "Rp"
                        L_Qris.Text = FormatCurrency(row.qris);
                        L_TunaiAwal.Text = FormatCurrency(row.kas_awal);
                        L_Transfer.Text = FormatCurrency(row.transfer);
                        L_Tunai.Text = FormatCurrency(row.tunai);
                        L_Total.Text = FormatCurrency(row.total_transaksi);

                        // Format all monetary values as Rp with thousand separators, e.g., Rp 1.000.000
                    }
                    else
                    {
                        // Handle jumlah kosong dan grandtotal kosong
                        System.Diagnostics.Debug.WriteLine("Api Data Error");

                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Api Data 0");
                }
            }
        }

        catch (Exception ex)
        {

        }
    }

    private string FormatCurrency(double amount)
    {
        // Format the amount as Indonesian Rupiah with thousand separators
        return "Rp " + amount.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
    }

    private async void get_listorder()
    {
        string url = App.API_HOST + "dapur/order.php";
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            List<list_order> rowData = JsonConvert.DeserializeObject<List<list_order>>(json);

            _listorder.Clear(); // Hapus list sebelum diisi

            
            for (int i = 0; i < rowData.Count; i++)
            {

                _listorder.Add(rowData[i]); 
            }

            //total = rowData.Count;
            lv_dapurorder.ItemsSource = _listorder; 

        }
        else
        {

        }
    }

    private async void TapOrderList_Tapped(object sender, EventArgs e)
    {

       
        var i = (ViewCell)sender;
        var rows = (list_order)i.BindingContext;

        string id_order_selected = rows.id_order;
        string kode_payment_selected = rows.kode_payment;
        System.Diagnostics.Debug.WriteLine($"ID:{id_order_selected}, KODE:{kode_payment_selected}");

        try
        {
            var popup = new DetailOrder(id_order_selected, kode_payment_selected);
            await this.ShowPopupAsync(popup);
        }
        catch (Exception)
        {
            // ignore
        }

    }

    private async void get_listinvoice()
    {
        string url = App.API_HOST + "kasir/list_invoice.php";
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            List<list_invoice> rowData = JsonConvert.DeserializeObject<List<list_invoice>>(json);

            _listinvoice.Clear(); // Hapus list sebelum diisi


            for (int i = 0; i < rowData.Count; i++)
            {
                rowData[i].total_cart_string = FormatCurrency(rowData[i].total_cart);
                _listinvoice.Add(rowData[i]);
            }

            //total = rowData.Count;
            lv_invoice.ItemsSource = _listinvoice;

        }
        else
        {

        }
    }

    private async void B_More_Clicked(object sender, EventArgs e)
    {

        if (sender is Button image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        var button = sender as Button;
        if (button?.CommandParameter is list_batal item)
        {
            string nm_produk = item.nama_produk;
            string alasan = item.alasan;
            System.Diagnostics.Debug.WriteLine($"Alasan: {alasan}");
            string waktu = item.waktu;
            string qty = item.qty.ToString();
            string harga = item.harga_jual_string;
            string meja = item.id_meja;
            string mode_pesanan = string.Empty;
            string kode_payment = item.kode_payment;
            if (item.ta_dinein == "0")
            {
                mode_pesanan = $"DINE-IN#{meja}";
            }
            else
            {
                mode_pesanan = "TAKEAWAY";
            }
            
            //memanggil popup DetailBatal.xaml dan mengirim data ke popup
            try
            {
                var popup = new DetailBatal(nm_produk, alasan, waktu, harga, mode_pesanan, kode_payment);
                await this.ShowPopupAsync(popup);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing DetailBatal popup: {ex.Message}");
            }
        }

    }
}