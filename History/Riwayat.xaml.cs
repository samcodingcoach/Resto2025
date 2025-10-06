using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Diagnostics;
using System.Linq;

namespace Resto2025.History;

public partial class Riwayat : ContentPage
{
    private string? TGL_PAYMENT;
    private List<list_riwayat> _listriwayat;
    private List<list_metodebayar> _listmetodebayar;
    private string? ID_BAYAR; private string? KATEGORI_PEMBAYARAN;

    public Riwayat()
    {
        InitializeComponent();
        DatePicker_tgl.MaximumDate = DateTime.Today;
        DatePicker_tgl.Date = DateTime.Today;
        TGL_PAYMENT = DatePicker_tgl.Date.ToString("yyyy-MM-dd");
        _listriwayat = new List<list_riwayat>();
        _listmetodebayar = new List<list_metodebayar>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CV_List.ItemsSource = null;
        Picker_Kategori.ItemsSource = null;
        get_metodebayar();
        get_list();
    }

    public class list_riwayat
    {
        public string kode_payment { get; set; } = string.Empty;
        public string tanggal_payment { get; set; } = string.Empty;
        public string kategori { get; set; } = string.Empty;
        public string nama_konsumen { get; set; } = string.Empty;
        public string id_meja { get; set; } = string.Empty;
        public int nomor_antri { get; set; } = 0;
        public double total_cart { get; set; } = 0;
        public string total_cart_string { get; set; } = string.Empty;
        public string id_tagihan { get; set; } = string.Empty;
        public string keterangan { get; set; } = string.Empty;
        public string mode_pesanan { get; set; } = string.Empty;
        public string warna_bg_terbayar { get; set; } = "#FF2D2D";
        public string nomor_meja { get; set; } = "TAKEAWAY";
    }

    public class list_metodebayar
    {
        public string id_bayar { get; set; } = string.Empty;
        public string kategori { get; set; } = string.Empty;

        public override string ToString()
        {
            return kategori;
        }
    }

    private string FormatCurrency(double amount)
    {
        // Format the amount as Indonesian Rupiah with thousand separators
        return "Rp " + amount.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
    }

    private async void get_list()
    {
        string url = App.API_HOST + "riwayat/list_riwayat.php?tgl=" + TGL_PAYMENT;
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            var rowData = JsonConvert.DeserializeObject<List<list_riwayat>>(json);
            _listriwayat.Clear();

            if (rowData != null)
            {
                for (int i = 0; i < rowData.Count; i++)
                {
                    rowData[i].total_cart_string = FormatCurrency(rowData[i].total_cart);
                    string x = rowData[i].keterangan;
                    if (x == "DIBAYARKAN")
                    {
                        rowData[i].warna_bg_terbayar = "#14AE5C";
                    }
                    else if (x == "BELUM BAYAR")
                    {
                        rowData[i].warna_bg_terbayar = "#FF2D2D";
                    }

                    if (rowData[i].id_meja == "0")
                    {
                        rowData[i].nomor_meja = $"MEJA: {rowData[i].id_meja}";
                    }

                    _listriwayat.Add(rowData[i]);
                }
            }

            ApplyFilter(L_Search?.Text);
        }
        else
        {
            
        }
    }

    private async void get_metodebayar()
    {
        string url = App.API_HOST + "pembayaran/list_metodebayar.php";
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            var rowData = JsonConvert.DeserializeObject<List<list_metodebayar>>(json);
            _listmetodebayar.Clear();

            if (rowData != null)
            {
                _listmetodebayar.AddRange(rowData);
            }

            Picker_Kategori.ItemsSource = _listmetodebayar;
        }
        else
        {
            
        }
    }

    private void OnDatePickerSelected(object? sender, DateChangedEventArgs e)
    {
        TGL_PAYMENT = e.NewDate.ToString("yyyy-MM-dd");
        System.Diagnostics.Debug.WriteLine($"tanggal:{TGL_PAYMENT}");
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter(e.NewTextValue);
    }

    private void ApplyFilter(string? query)
    {
        IEnumerable<list_riwayat> items = _listriwayat;

        if (!string.IsNullOrWhiteSpace(KATEGORI_PEMBAYARAN))
        {
            items = items.Where(item => string.Equals(item.kategori, KATEGORI_PEMBAYARAN, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            items = items.Where(item => item.kode_payment.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        CV_List.ItemsSource = items.ToList();
    }

    private void OnKategoriSelected(object sender, EventArgs e)
    {
        if (Picker_Kategori.SelectedItem is list_metodebayar selected)
        {
            ID_BAYAR = selected.id_bayar;
            KATEGORI_PEMBAYARAN = selected.kategori;
            ApplyFilter(L_Search?.Text);
        }
        else
        {
            ID_BAYAR = null;
            KATEGORI_PEMBAYARAN = null;
            ApplyFilter(L_Search?.Text);
        }
    }
}