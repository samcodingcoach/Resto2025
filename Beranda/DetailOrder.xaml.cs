using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
namespace Resto2025.Beranda;

public partial class DetailOrder : ContentPage
{
    private List<list_order> _listorder;
    string ID_ORDER = "605";//string.Empty;
   
    public DetailOrder()
    {
        InitializeComponent(); 
        _listorder = new List<list_order>();
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
        public string waktu_batal { get; set; } = string.Empty;
        public string waktu_terima { get; set; } = string.Empty;
        public string waktu_delivered { get; set; } = string.Empty;


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
                        }
                        else if (produk.ready == "2")
                        {
                            produk.ready_string = "DISAJIKAN";
                        }
                        else if (produk.ready == "3")
                        {
                            produk.ready_string = "BATAL";
                        }

                        if(produk.ta_dinein == "1")
                        {
                            produk.mode_pesanan = "TAKEAWAY";
                        }
                        else if(produk.ta_dinein=="0")
                        {
                            produk.mode_pesanan = "DINE IN";
                        }
                            
                        _listorder.Add(produk);
                    }

                    // Terapkan data baru ke CollectionView
                    CV_ListOrder.ItemsSource = null;
                    CV_ListOrder.ItemsSource = _listorder;
                }
                else
                {
                    // Handle jika server tidak merespon dengan baik
                    await DisplayAlert("Gagal Terhubung", $"Server merespon dengan status: {response.StatusCode}", "OK");
                    CV_ListOrder.ItemsSource = null; // Kosongkan list jika gagal
                }
            }
        }
        catch (Exception ex)
        {
            // Handle jika terjadi error (misal: tidak ada koneksi internet)
            await DisplayAlert("Error Produk", $"Terjadi kesalahan saat memuat produk: {ex.Message}", "OK");
        }
        finally
        {
            // 2. Sembunyikan loading indicator SETELAH semua proses selesai
          
           
        }

    }








}