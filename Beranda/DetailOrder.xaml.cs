using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using CommunityToolkit.Maui.Views;
namespace Resto2025.Beranda;

public partial class DetailOrder : Popup
{
    private List<list_order> _listorder;
    string ID_ORDER = "0";
    string KODE_PAYMENT=string.Empty;
   

    public DetailOrder(string idOrder, string kodePayment)
    {
        InitializeComponent();
        _listorder = new List<list_order>();
        ID_ORDER = idOrder ?? "0";
        KODE_PAYMENT = kodePayment ?? string.Empty;
        L_Title.Text = $"Detail Pesanan: {KODE_PAYMENT}";
        get_listorder();
    }



    public class list_order
    {
        public string kode_produk { get; set; } = string.Empty;
        public string url_gambar => App.IMAGE_HOST + kode_produk + ".jpg";
        public string nama_produk { get; set; } = string.Empty;
        public string nama_kategori { get; set; } = string.Empty;
        public string ready { get; set; } = string.Empty;
        public string ready_string { get; set; } = "DALAM PROSES";
        public string tgl_update { get; set; } = string.Empty;
        public int qty { get; set; } = 0;
        public string ta_dinein { get; set; } = string.Empty;    
        public string mode_pesanan { get; set; } = string.Empty;
        public string id_order { get; set; } = string.Empty;
        public string waktu_batal { get; set; } = "-";
        public string waktu_ready { get; set; } = "-";
        public string waktu_delivered { get; set; } = "-";

        public string warna_bg_ready { get; set; } = string.Empty;
        public string warna_tx_ready { get; set; } = string.Empty;

    }



    private async void get_listorder()
    {
        try
        {
            string url = App.API_HOST + "dapur/detail_order.php?id_order=" + ID_ORDER;
            using (HttpClient client = new HttpClient())
            {
                // Set timeout agar tidak menunggu terlalu lama
                client.Timeout = TimeSpan.FromSeconds(15);
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    List<list_order> rowData = JsonConvert.DeserializeObject<List<list_order>>(json);
                    _listorder.Clear();

                    foreach (var produk in rowData)
                    {
                        if (produk.ready == "1")
                        {
                            produk.ready_string = "SIAP DIAMBIL";
                            produk.warna_tx_ready = "#1A86F2";
                            produk.warna_bg_ready = "#8AC1F8";
                        }
                        else if (produk.ready == "2")
                        {
                            produk.ready_string = "DISAJIKAN";
                            produk.warna_tx_ready = "#52D356";
                            produk.warna_bg_ready = "#B3F1B5";
                        }
                        else if (produk.ready == "3")
                        {
                            produk.ready_string = "BATAL";
                            produk.warna_tx_ready = "#ED0D58";
                            produk.warna_bg_ready = "#EAA6BD";
                        }
                        else if(produk.ready == "0")
                        {
                            produk.ready_string = "DALAM PROSES";
                            produk.warna_tx_ready = "Grey";
                            produk.warna_bg_ready = "LightGrey";
                        }

                        if (produk.ta_dinein == "1")
                        {
                            produk.mode_pesanan = "TAKEAWAY";
                        }
                        else if (produk.ta_dinein == "0")
                        {
                            produk.mode_pesanan = "DINE IN";
                        }

                        //warna ready 1 #8AC1F8 #1A86F2

                        _listorder.Add(produk);
                    }

                    // Terapkan data baru ke CollectionView
                    CV_ListOrder.ItemsSource = null;
                    CV_ListOrder.ItemsSource = _listorder;
                }
                else
                {
                    // Handle jika server tidak merespon dengan baik
                    //await DisplayAlert("Gagal Terhubung", $"Server merespon dengan status: {response.StatusCode}", "OK");
                    CV_ListOrder.ItemsSource = null; // Kosongkan list jika gagal
                }
            }
        }
        catch (Exception ex)
        {
            // Handle jika terjadi error (misal: tidak ada koneksi internet)
           // await DisplayAlert("Error Produk", $"Terjadi kesalahan saat memuat produk: {ex.Message}", "OK");
        }
        finally
        {
            // 2. Sembunyikan loading indicator SETELAH semua proses selesai
          
           
        }

    }








}