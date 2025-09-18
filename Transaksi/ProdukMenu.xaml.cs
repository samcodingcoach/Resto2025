using Microsoft.Maui.Controls.Shapes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using static Microsoft.Maui.Controls.Compatibility.Grid;

namespace Resto2025.Transaksi;

public partial class ProdukMenu : ContentPage
{
    private Border borderMejaTerpilih;
    public string ID_KATEGORI = string.Empty;
    public string ID_KONSUMEN = "1"; // Default ID_KONSUMEN GUEST
    public string ID_MEJA = "0";
    public string ID_BAYAR = "1"; // Default Tunai
    public double BIAYA_ADMIN = 0;
    
    public string NOMORHP = string.Empty;

    private List<list_produk> _listproduk;
    private List<list_meja> listMeja = new List<list_meja>();
    private List<list_metodepembayaran> _listmetodepembayaran = new List<list_metodepembayaran>();

    public ProdukMenu()
	{
		InitializeComponent();
        get_data_kategori();

        _listproduk = new List<list_produk>();
        get_listproduk();
        get_metode_pembayaran();
    }

    public class data_kategori()
    {
        public string id_kategori { get; set; } = string.Empty;

        

    }

    public class list_produk
    {
        public string id_produk { get; set; } = string.Empty; public string id_produk_sell { get; set; } = string.Empty;
        public string kode_produk { get; set; } = string.Empty;
        public string url_gambar => App.IMAGE_HOST + kode_produk + ".jpg";
        public string nama_produk { get; set; } = string.Empty;
        public int stok { get; set; } = 0;
        public double harga_jual { get; set; } = 0; 
        public string new_harga_jual { get; set; } = string.Empty;
        public double opacity_produk { get; set; } = 1;
        public bool enabled_produk { get; set; } = true;
    }

    public class list_meja
    {
        public int id_meja { get; set; } = 0; 
        public int in_used { get; set; } = 0;
        public string nomor_meja { get; set; } = string.Empty;
        public string pos_x { get; set; } = string.Empty;
        public string pos_y { get; set; } = string.Empty;
        public Color warna { get; set; } = Color.FromRgba("075E54"); // Warna default, warna in used = #FF2D2D
    }

    public class list_metodepembayaran
    {
        public string id_bayar { get; set; } = string.Empty;
        public string kategori { get; set; } = string.Empty;
        public string no_rek { get; set; } = string.Empty;
        public double biaya_admin { get; set; } = 0;
        public string keterangan { get; set; } = string.Empty;
        public string pramusaji { get; set; } = string.Empty;
        public string aktif { get; set; } = string.Empty;

    }



    public class list_konsumen
    {
        public string id_konsumen { get; set; } = string.Empty;
        public string nama_konsumen { get; set; } = string.Empty;
    }

    private async void get_konsumen()
    {
        try
        {
            string url = App.API_HOST + "konsumen/search_konsumen.php?nomor=" + NOMORHP;
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    List<list_konsumen> rowData = JsonConvert.DeserializeObject<List<list_konsumen>>(json);

                    if (rowData != null && rowData.Count > 0)
                    {
                        list_konsumen row = rowData[0];
                        string nama = row.nama_konsumen;
                        ID_KONSUMEN = row.id_konsumen;
                    }
                    else
                    {
                        // Handle jumlah kosong dan grandtotal kosong
                        await DisplayAlert("Informasi", "Konsumen dengan nomor HP tersebut tidak ditemukan.", "OK");
                        ID_KONSUMEN = "1";
                    }
                }
                else
                {
                    await DisplayAlert("Gagal Terhubung", $"Tidak dapat mengambil data dari server. Status: {response.StatusCode}", "OK");
                }
            
            }
        }

        catch (Exception ex)
        {
           await DisplayAlert("Error", ex.Message, "OK");
        }
    }




    private async void get_metode_pembayaran()
    {
        string url = App.API_HOST + "pembayaran/list_metodebayar.php";
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            List<list_metodepembayaran> rowData = JsonConvert.DeserializeObject<List<list_metodepembayaran>>(json);

            _listmetodepembayaran.Clear(); // Hapus list sebelum diisi

           
            for (int i = 0; i < rowData.Count; i++)
            {

                _listmetodepembayaran.Add(rowData[i]); 
            }

           
            lv_metodepembayaran.ItemsSource = _listmetodepembayaran;

        }
        else
        {
            // Tangani error di sini, misalnya dengan alert
        }

    }

    private async Task get_listproduk()
    {
       // contentview.IsVisible = true;
        await Task.Delay(1000);
        string url = App.API_HOST + "produk/list_produk.php?id_kategori=" + ID_KATEGORI;
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            List<list_produk> rowData = JsonConvert.DeserializeObject<List<list_produk>>(json);
            _listproduk.Clear();


            for (int i = 0; i < rowData.Count; i++)
            {

                var formated = "Rp " + ((int)rowData[i].harga_jual).ToString("N0");
                rowData[i].new_harga_jual = formated;
                int cek_sisa = rowData[i].stok;
                if (cek_sisa <= 0)
                {
                    rowData[i].opacity_produk = 0.3; rowData[i].enabled_produk = false;
                }
                _listproduk.Add(rowData[i]);
            }

            if (rowData.Count >= 1)
            {
                

                CV_ListProduk.ItemsSource = _listproduk;
            }
            else if (rowData.Count <= 0)
            {
               
            }

            //T_TotalVariatif.Text = $"Total {kategori}: {rowData.Count.ToString()} Produk";
        }
    }

    private async void get_data_kategori()
    {
        string url = App.API_HOST + "kategori_menu/list_kategori.php";
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            List<string> nama = JsonConvert.DeserializeObject<List<string>>(json);

            Picker_Kategori.ItemsSource = nama;
            System.Diagnostics.Debug.WriteLine("Load Kategori Menu Berhasil");
        }
        else
        {
            await DisplayAlert("Error", "Gagal mendapatkan data kategori", "OK");
        }
    }

    private async void Picker_Kategori_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        string nilaiTerpilih = picker.SelectedItem.ToString();

        string url = App.API_HOST + "kategori_menu/id_kategori.php?nama_kategori="+ nilaiTerpilih;
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            var kategoriData = JsonConvert.DeserializeObject<List<data_kategori>>(json);


          
            if (kategoriData != null && kategoriData.Count > 0)
            {
                // 3. Ambil nilai "id_kategori" dari item pertama di list
                string idKategori = kategoriData[0].id_kategori;

               ID_KATEGORI = idKategori;
               get_listproduk();

            }
            else
            {
                await DisplayAlert("Info", "Data kategori tidak ditemukan.", "OK");
            }
        }
        else
        {
            await DisplayAlert("Error", "Gagal mendapatkan data kategori", "OK");
        }
    }

    private async void Tap_ModePesanan_Tapped(object sender, TappedEventArgs e)
    {

        if (sender is Frame image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        GridModePesanan.IsVisible = true;
        GridProduk.IsVisible = false;
        lv_metodepembayaran.IsVisible = false;

        LNavModePesanan.TextColor = Color.FromArgb("#FFFFFF");
        NavModePesanan.BackgroundColor = Color.FromArgb("#075E54");

        LNavProduk.TextColor = Color.FromArgb("#333");
        NavProduk.BackgroundColor = Color.FromArgb("#F2F2F2");

        LNavKonsumen.TextColor = Color.FromArgb("#333");
        NavKonsumen.BackgroundColor = Color.FromArgb("#F2F2F2");

        LNavPembayaran.TextColor = Color.FromArgb("#333");
        NavPembayaran.BackgroundColor = Color.FromRgba("#F2F2F2");

    }

    private void Tap_Konsumen_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void Tap_Produk_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        GridModePesanan.IsVisible = false;
        GridProduk.IsVisible = true;
        lv_metodepembayaran.IsVisible = false;

        LNavModePesanan.TextColor = Color.FromArgb("#333");
        NavModePesanan.BackgroundColor = Color.FromArgb("#F2F2F2");

        LNavProduk.TextColor = Color.FromArgb("#FFFFFF");
        NavProduk.BackgroundColor = Color.FromArgb("#075E54");

        LNavKonsumen.TextColor = Color.FromArgb("#333");
        NavKonsumen.BackgroundColor = Color.FromArgb("#F2F2F2");

        LNavPembayaran.TextColor = Color.FromArgb("#333");
        NavPembayaran.BackgroundColor = Color.FromRgba("#F2F2F2");
    }

    private async void Tap_Pembayaran_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        GridModePesanan.IsVisible = false;
        GridProduk.IsVisible = false;
        lv_metodepembayaran.IsVisible = true;

        LNavModePesanan.TextColor = Color.FromArgb("#333");
        NavModePesanan.BackgroundColor = Color.FromArgb("#F2F2F2");

        LNavProduk.TextColor = Color.FromArgb("#333");
        NavProduk.BackgroundColor = Color.FromArgb("#F2F2F2");

        LNavKonsumen.TextColor = Color.FromArgb("#333");
        NavKonsumen.BackgroundColor = Color.FromArgb("#F2F2F2");

        LNavPembayaran.TextColor = Color.FromArgb("#FFFFFF");
        NavPembayaran.BackgroundColor = Color.FromRgba("#075E54");
    }

    private void RadioTakeaway_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
      
        if (!e.Value) return;
        DenahMejaContainer.IsVisible = false;
        if (borderMejaTerpilih != null)
        {
          
            borderMejaTerpilih.BackgroundColor = Color.FromArgb("#37474F");
            borderMejaTerpilih = null;
        }
        ID_MEJA = "0";
        Summary_ModePesanan.Text = "Takeaway";
    }

    private void RadioDine_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        
        if (!e.Value) return;
        DenahMejaContainer.IsVisible = true;      
        get_meja();
        Summary_ModePesanan.Text = "Dine-In";
    }

    private void OnMejaTapped(object sender, TappedEventArgs e)
    {
        // Pastikan parameter dan elemen yang di-tap valid
        if (e.Parameter is list_meja mejaYangDiTap && sender is Border borderBaru)
        {
            // 1. Jika meja sudah terpakai, tampilkan pesan dan hentikan proses
            if (mejaYangDiTap.in_used == 1)
            {
                DisplayAlert("Informasi", "Meja ini sudah terpakai.", "OK");
                return;
            }

            // 2. KEMBALIKAN WARNA MEJA LAMA:
            // Jika sebelumnya sudah ada meja yang dipilih (misal: meja 08)...
            if (borderMejaTerpilih != null)
            {
                // ...kembalikan warnanya ke warna "tersedia" (abu-abu/hijau tua)
                borderMejaTerpilih.BackgroundColor = Color.FromArgb("#37474F");
            }

            // 3. ATUR WARNA MEJA BARU:
            // Ubah warna meja yang baru di-tap (misal: meja 09) menjadi warna "dipesan"
            borderBaru.BackgroundColor = Color.FromArgb("#075E54");

            // 4. UPDATE MEMORI:
            // Sekarang, jadikan meja yang baru di-tap ini sebagai "meja terpilih"
            borderMejaTerpilih = borderBaru;

            // Simpan ID Meja yang baru dipilih
            ID_MEJA = mejaYangDiTap.id_meja.ToString();
            Summary_ModePesanan.Text = $"Dine In - Meja #{mejaYangDiTap.nomor_meja.PadLeft(2, '0')}";
        }
    }

    private async void get_meja()
    {
        string url = App.API_HOST + "meja/list_meja.php";
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            listMeja = JsonConvert.DeserializeObject<List<list_meja>>(json);

            int totalMeja = listMeja.Count;


            int mejaInUsed = listMeja.Count(m => m.in_used == 1);

            // ================= PENGEMBANGAN DIMULAI DI SINI =================

            // 1. Bersihkan denah sebelum menambahkan meja baru (untuk refresh)
            DenahMejaLayout.Children.Clear();

            // 2. Looping setiap data meja dari API
            foreach (var meja in listMeja)
            {
                // 3. Tentukan warna berdasarkan status 'in_used'
                Color warnaMeja = meja.in_used == 1 ? Color.FromArgb("#E53935") : Color.FromArgb("#37474F"); // Merah jika dipakai, hijau tua jika tidak

                // 4. Buat elemen UI untuk meja (Border + Label)
                var labelNomor = new Label
                {
                    Text = meja.nomor_meja.PadLeft(2, '0'), // Format jadi "01", "02", dst.
                    TextColor = Colors.White,
                    FontSize = 24,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                var borderMeja = new Border
                {
                    WidthRequest = 80,
                    HeightRequest = 80,
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    BackgroundColor = warnaMeja,
                    Content = labelNomor
                };

                // 5. Tambahkan gesture recognizer untuk event klik/tap
                var tapGesture = new TapGestureRecognizer();
                tapGesture.CommandParameter = meja; // Kirim seluruh objek 'meja' saat di-tap
                tapGesture.Tapped += OnMejaTapped;
                borderMeja.GestureRecognizers.Add(tapGesture);

                // 6. Konversi posisi X dan Y dari string ke double
                double x = double.Parse(meja.pos_x);
                double y = double.Parse(meja.pos_y);

                // 7. Atur posisi dan ukuran elemen di AbsoluteLayout
                AbsoluteLayout.SetLayoutBounds(borderMeja, new Rect(x, y, 60, 60));

                // 8. Tambahkan elemen meja ke layout
                DenahMejaLayout.Children.Add(borderMeja);
            }
            // ================= PENGEMBANGAN SELESAI =================
        }
        else
        {
            await DisplayAlert("Error", "Gagal memuat data meja", "OK");
        }
    }

    private void RadioTipePembayaran_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value == false)
        {
            return;
        }

        var radioButton = sender as RadioButton;
        if (radioButton == null)
        {
            return;
        }

        
        var selectedItem = radioButton.BindingContext as list_metodepembayaran;
        if (selectedItem == null)
        {
            return;
        }
       
        ID_BAYAR = selectedItem.id_bayar;
        string kategori = selectedItem.kategori;
        BIAYA_ADMIN = selectedItem.biaya_admin;

        Summary_MetodeBayar.Text = kategori;
        Summary_BiayaAdmin.Text = $"Rp {BIAYA_ADMIN.ToString("N0")}"; 
    }
}