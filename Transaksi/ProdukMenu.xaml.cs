using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using CommunityToolkit.Maui.Behaviors;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Microsoft.Maui.Controls.Compatibility.Grid;

namespace Resto2025.Transaksi;

public partial class ProdukMenu : ContentPage
{
    public string ID_USER= "4"; // sementara nanti ganti sama temp login
    private Border borderMejaTerpilih;
    private double currentScale = 1;
    private double startScale = 1;

    public string KODE_PAYMENT = string.Empty;
    public string ID_KATEGORI = string.Empty;
    public string ID_KONSUMEN = "1"; // Default ID_KONSUMEN GUEST
    public string ID_MEJA = "0";
    public string ID_BAYAR = "1"; // Default Tunai
    public double BIAYA_ADMIN = 0;
    public double PERSENTASE_PPN = 0;
    public string ID_PROMO = "0";
    public double NILAI_PROMO = 0;
    public double NILAI_PER_TAKEAWAY = 0;
    public string PILIHAN_PROMO = string.Empty;
    public bool MODE_PESANAN_DINE = false;
    public bool MODE_PESANAN_TA = true;


    public double PERSEN_PROMO = 0;
    public double NOMINAL_PROMO = 0;

    public double MIN_PEMBELIAN = 0;

    public double NILAI_POTONGAN = 0;

    public string NOMORHP = string.Empty;

    public int STATUS_BAYAR = 0; //0 = belum bayar, 1 = sudah bayar, 2 batal

    public double subtotal = 0;
    public double totalBiayaTakeaway = 0;
    public double nilaiPPN = 0;
    private double grandTotalFinal = 0;
    

    private list_metodepembayaran metodeBayarTerpilih;


    private List<list_produk> _listproduk;
    private List<list_meja> listMeja = new List<list_meja>();
    private List<list_metodepembayaran> _listmetodepembayaran = new List<list_metodepembayaran>();

    public ObservableCollection<KeranjangItem> keranjang = new ObservableCollection<KeranjangItem>();

    public ProdukMenu()
    {
        InitializeComponent();
        LV_Keranjang.ItemsSource = keranjang;
        get_data_kategori();
        get_data_promo();
        get_biaya_takeaway();

        _listproduk = new List<list_produk>();
        get_listproduk();
        get_metode_pembayaran();
        get_ppn();

        Task.Run(async () => await MuatPesananSementaraAsync());
    }

    // Konstruktor tambahan untuk mengisi keranjang dari data pesanan
    public ProdukMenu(List<CekPesanan_Modal.PesananDetailInfo> pesananDetail, string idMeja = "0", string kodePayment = "")
    {
        InitializeComponent();
        LV_Keranjang.ItemsSource = keranjang;
	
        _listproduk = new List<list_produk>();
        this.KODE_PAYMENT = kodePayment;
        System.Diagnostics.Debug.WriteLine($"ProdukMenu initialized with KODE_PAYMENT: {this.KODE_PAYMENT}");
        _ = InisialisasiDariPesananAsync(pesananDetail, idMeja);
    }

    // Model untuk setiap item di dalam 'pesanan_detail'
    public class PesananDetailPayload
    {
        [JsonProperty("id_produk_sell")]
        public string IdProdukSell { get; set; }

        [JsonProperty("qty")]
        public int Qty { get; set; }

        [JsonProperty("ta_dinein")]
        public string TaDinein { get; set; }
    }

    // Model utama untuk JSON yang akan dikirim ke API
    

    public class ProsesPembayaranDetailPayload
    {
        [JsonProperty("subtotal")]
        public double? Subtotal { get; set; }

        [JsonProperty("biaya_pengemasan")]
        public double? BiayaPengemasan { get; set; }

        [JsonProperty("service_charge")]
        public double? ServiceCharge { get; set; }

        [JsonProperty("promo_diskon")]
        public double? PromoDiskon { get; set; }

        [JsonProperty("ppn_resto")]
        public double? PpnResto { get; set; }
    }

    public class ProsesPembayaranPayload
    {
        [JsonProperty("id_bayar")]
        public string IdBayar { get; set; }

        [JsonProperty("id_user")]
        public string IdUser { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("jumlah_uang")]
        public double? JumlahUang { get; set; }

        [JsonProperty("jumlah_dibayarkan")]
        public double? JumlahDibayarkan { get; set; }

        [JsonProperty("kembalian")]
        public double? Kembalian { get; set; }

        [JsonProperty("model_diskon")]
        public string ModelDiskon { get; set; }

        [JsonProperty("nilai_nominal")]
        public double? NilaiNominal { get; set; }

        [JsonProperty("total_diskon")]
        public double? TotalDiskon { get; set; }

        [JsonProperty("pembayaran_detail")]
        public ProsesPembayaranDetailPayload PembayaranDetail { get; set; }
    }

    // Ubah kelas PesananPayload yang sudah ada menjadi seperti ini
    public class PesananPayload
    {
        [JsonProperty("id_user")]
        public string IdUser { get; set; }

        [JsonProperty("id_konsumen")]
        public string IdKonsumen { get; set; }

        [JsonProperty("total_cart")]
        public double TotalCart { get; set; }

        [JsonProperty("status_checkout")]
        public string StatusCheckout { get; set; }

        [JsonProperty("id_meja")]
        public string IdMeja { get; set; }

        [JsonProperty("deviceid")]
        public string DeviceId { get; set; }

        [JsonProperty("pesanan_detail")]
        public List<PesananDetailPayload> PesananDetail { get; set; }

        // TAMBAHKAN OBJEK PEMBAYARAN DI SINI
        [JsonProperty("proses_pembayaran")]
        public ProsesPembayaranPayload ProsesPembayaran { get; set; }
    }


    public class TransaksiSementara
    {
        public string IdKonsumen { get; set; }
        public string IdMeja { get; set; }
        public string IdBayar { get; set; }
        public string IdPromo { get; set; }
        public double NilaiPotongan { get; set; }
        public List<KeranjangItem> ItemDiKeranjang { get; set; }
    }

    private async void OnPopupClosed()
    {


    }

    public class KeranjangItem : INotifyPropertyChanged
    {
        // Properti asli
        public string IdProduk { get; set; }
        public string IdProdukSell { get; set; }
        public string NamaProduk { get; set; }
        public double HargaJual { get; set; }
        public string UrlGambar { get; set; }
        
        // Ubah 'IkonModePesanan' menjadi properti dengan backing field
        private string _ikonModePesanan;
        public string IkonModePesanan
        {
            get => _ikonModePesanan;
            set
            {
                if (_ikonModePesanan != value)
                {
                    _ikonModePesanan = value;
                    OnPropertyChanged(); // Memberitahu UI bahwa 'IkonModePesanan' berubah
                }
            }
        }
        
        public int StokTersedia { get; set; }

        // Ubah 'Jumlah' menjadi properti dengan backing field
        private int _jumlah;
        public int Jumlah
        {
            get => _jumlah;
            set
            {
                if (_jumlah != value)
                {
                    _jumlah = value;
                    OnPropertyChanged(); // Memberitahu UI bahwa 'Jumlah' berubah
                    OnPropertyChanged(nameof(Subtotal)); // Memberitahu UI bahwa 'Subtotal' juga berubah
                    OnPropertyChanged(nameof(FormattedSubtotal)); // Dan formatnya juga
                }
            }
        }

        // Properti kalkulasi
        public double Subtotal => HargaJual * Jumlah;
        public string FormattedHargaJual => $"Rp {HargaJual:N0}";
        public string FormattedSubtotal => $"Rp {Subtotal:N0}";

        // Implementasi INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private void Produk_Tapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is list_produk produkDipilih)
        {
            // Cari apakah produk ini sudah ada di keranjang
            var itemDiKeranjang = keranjang.FirstOrDefault(item => item.IdProduk == produkDipilih.id_produk);

            // KASUS 1: PRODUK SUDAH ADA DI KERANJANG
            if (itemDiKeranjang != null)
            {
                // Validasi: Hanya tambah jumlah jika jumlah saat ini di keranjang BELUM MENCAPAI batas stok
                if (itemDiKeranjang.Jumlah < itemDiKeranjang.StokTersedia)
                {
                    itemDiKeranjang.Jumlah++;
                    UpdateTotalBelanja(); // Update total setelah berhasil
                }
                else
                {
                    // Jika sudah sama dengan stok, tampilkan pesan dan jangan lakukan apa-apa
                    DisplayAlert("Stok Maksimal", "Jumlah pesanan untuk item ini sudah mencapai batas stok.", "OK");
                }
            }
            // KASUS 2: PRODUK BELUM ADA DI KERANJANG
            else
            {
                // Validasi: Hanya tambahkan produk baru jika stoknya lebih dari 0
                if (produkDipilih.stok > 0)
                {
                    var newItem = new KeranjangItem
                    {
                        IdProduk = produkDipilih.id_produk,
                        IdProdukSell = produkDipilih.id_produk_sell,
                        NamaProduk = produkDipilih.nama_produk,
                        HargaJual = produkDipilih.harga_jual,
                        Jumlah = 1,
                        UrlGambar = produkDipilih.url_gambar,
                        StokTersedia = produkDipilih.stok
                    };
                    // Atur IkonModePesanan menggunakan setter agar OnPropertyChanged dipicu
                    newItem.IkonModePesanan = (this.ID_MEJA == "0") ? "takeaway.png" : "dine.png";
                    keranjang.Add(newItem);
                    UpdateTotalBelanja(); // Update total setelah berhasil
                   
                }
                else
                {
                    // Jika stok dari awal sudah 0, jangan tambahkan ke keranjang
                    DisplayAlert("Stok Habis", "Produk ini sudah habis.", "OK");
                }
            }
        }
    }
    //kalkulasi
    public void UpdateTotalBelanja()
    {

        int totalItem = keranjang.Sum(item => item.Jumlah);
        Summary_TotalItem.Text = $"TOTAL ITEM ({totalItem})";

        double totalProduk = keranjang.Sum(item => item.Subtotal);
        Summary_TotalProduk.Text = $"Rp {totalProduk:N0}";

        int totalKuantitasTakeaway = keranjang
            .Where(item => item.IkonModePesanan == "takeaway.png")
            .Sum(item => item.Jumlah);


       totalBiayaTakeaway = totalKuantitasTakeaway * NILAI_PER_TAKEAWAY;


        Summary_BiayaTakeaway.Text = $"Rp {totalBiayaTakeaway:N0}";

        if (metodeBayarTerpilih != null)
        {
            // Cek jika metode pembayaran adalah QRIS (atau ID 3)
            if (metodeBayarTerpilih.id_bayar == "3") // Asumsi ID 3 adalah QRIS
            {
                // Hitung subtotal dasar untuk biaya admin
                double subtotalUntukFee = totalProduk + totalBiayaTakeaway - NILAI_PROMO;

               
                BIAYA_ADMIN = subtotalUntukFee * (metodeBayarTerpilih.biaya_admin / 100.0);
            }
            else
            {
                // Jika bukan QRIS, ambil nilainya langsung
                BIAYA_ADMIN = metodeBayarTerpilih.biaya_admin;
            }

            // Update UI Biaya Admin
            Summary_BiayaAdmin.Text = $"Rp {BIAYA_ADMIN:N0}";
        }

        NILAI_PROMO = 0; // Reset nilai promo setiap kali perhitungan ulang

        // Cek apakah total belanja memenuhi minimum pembelian promo
        if (ID_PROMO != "0" && totalProduk >= MIN_PEMBELIAN)
        {
            // Cek tipe promo: persen atau nominal
            if (PILIHAN_PROMO == "persen")
            {
                NILAI_PROMO = totalProduk * (PERSEN_PROMO / 100.0);
            }
            else if (PILIHAN_PROMO == "nominal")
            {
                NILAI_PROMO = NOMINAL_PROMO;
            }
        }

        // Update Label nilai promosi di UI
        Summary_NilaiPromosi.Text = $"Rp {NILAI_PROMO:N0}";

        Summary_Potongan.Text = $"Rp {NILAI_POTONGAN:N0}";


        subtotal = totalProduk + totalBiayaTakeaway + BIAYA_ADMIN - NILAI_PROMO - NILAI_POTONGAN;
        Summary_Subtotal.Text = $"Rp {subtotal:N0}";


        nilaiPPN = subtotal * (PERSENTASE_PPN / 100.0);
        Summary_PPN.Text = $"Rp {nilaiPPN:N0}";


        double grandTotal = subtotal + nilaiPPN;
        double grandTotalBulat = Math.Floor(grandTotal / 100.0) * 100;


        Summary_TotalCheckout.Text = $"Rp {grandTotalBulat:N0}";

        // nilai modal
        this.grandTotalFinal = grandTotalBulat;

        Task.Run(async () => await SimpanPesananSementaraAsync());
    }
    //keranjang tapped
    private async void ItemKeranjang_Tapped(object sender, EventArgs e)
    {
        // Dapatkan item yang di-tap dari BindingContext ViewCell
        if ((sender as ViewCell)?.BindingContext is KeranjangItem item)
        {
            // Tampilkan Action Sheet dengan pilihan untuk pengguna
            string action = await DisplayActionSheet(
                $"Pilih Aksi untuk {item.NamaProduk}", // Judul
                "Batal",                               // Tombol Batal
                null,                                  // Tombol Hapus (tidak ada di sini)
                "Edit Jumlah",                         // Pilihan 1
                "Ubah Mode Pesanan",                   // Pilihan 2
                "Hapus Item");                         // Pilihan 3

            // Gunakan switch untuk menentukan aksi berdasarkan pilihan pengguna
            switch (action)
            {
                case "Edit Jumlah":
                    await EditJumlahAsync(item);
                    break;

                case "Ubah Mode Pesanan":
                    await UbahModePesananAsync(item);
                    break;

                case "Hapus Item":
                    await HapusItemAsync(item);
                    break;
            }
        }
    }

    private async Task EditJumlahAsync(KeranjangItem item)
    {
        string hasil = await DisplayPromptAsync(
            title: "Ubah Jumlah",
            message: $"Stok tersedia: {item.StokTersedia}\n\nMasukkan jumlah baru:",
            accept: "Simpan",
            cancel: "Batal",
            placeholder: item.Jumlah.ToString(),
            maxLength: 3,
            keyboard: Keyboard.Numeric);

        if (hasil != null)
        {
            if (int.TryParse(hasil, out int jumlahBaru) && jumlahBaru >= 0) // Izinkan jumlah 0 untuk menghapus
            {
                if (jumlahBaru == 0)
                {
                    // Jika user memasukkan 0, anggap ingin menghapus
                    await HapusItemAsync(item);
                    return;
                }

                if (jumlahBaru > item.StokTersedia)
                {
                    await DisplayAlert("Stok Tidak Cukup", $"Jumlah melebihi stok yang tersedia ({item.StokTersedia}).", "OK");
                    return;
                }

                item.Jumlah = jumlahBaru;
                UpdateTotalBelanja();
            }
            else
            {
                await DisplayAlert("Input Tidak Valid", "Harap masukkan jumlah yang valid.", "OK");
            }
        }
    }

    private async Task HapusItemAsync(KeranjangItem itemDihapus)
    {
        bool jawaban = await DisplayAlert(
            "Konfirmasi Hapus",
            $"Anda yakin ingin menghapus '{itemDihapus.NamaProduk}' dari keranjang?",
            "Ya, Hapus",
            "Tidak");

        if (jawaban)
        {
            keranjang.Remove(itemDihapus);
            UpdateTotalBelanja();
        }
    }

    private async Task UbahModePesananAsync(KeranjangItem item)
    {
        // Dapatkan mode pesanan saat ini dari item
        string currentMode = item.IkonModePesanan == "takeaway.png" ? "Takeaway" : "Dine-in";
        string oppositeMode = item.IkonModePesanan == "takeaway.png" ? "Dine-in" : "Takeaway";
        
        // Jika jumlah item lebih dari 1, tawarkan opsi untuk memecah item
        if (item.Jumlah > 1)
        {
            string action = await DisplayActionSheet(
                $"Pilih Aksi untuk {item.NamaProduk}",
                "Batal",
                null,
                "Ubah Semua",
                "Pecah Item");

            if (action == "Ubah Semua")
            {
                string newMode = await DisplayActionSheet(
                    $"Ubah Mode Pesanan - Saat ini: {currentMode}",
                    "Batal",
                    null,
                    "Takeaway",
                    "Dine-in");

                if (newMode != null && newMode != currentMode)
                {
                    // Simpan mode pesanan lama untuk debugging
                    string oldMode = item.IkonModePesanan;
                    
                    // Ubah mode pesanan berdasarkan pilihan
                    if (newMode == "Takeaway")
                    {
                        item.IkonModePesanan = "takeaway.png";
                    }
                    else if (newMode == "Dine-in")
                    {
                        item.IkonModePesanan = "dine.png";
                    }
                    
                    // Debug output sesuai permintaan
                    System.Diagnostics.Debug.WriteLine($"Item {item.NamaProduk}: IkonModePesanan lama = {oldMode}, baru = {item.IkonModePesanan}");
                }
            }
            else if (action == "Pecah Item")
            {
                // Tawarkan jumlah baru untuk item yang akan diubah mode pesannya
                string jumlahInput = await DisplayPromptAsync(
                    title: "Pecah Item - Jumlah",
                    message: $"Item saat ini: {item.NamaProduk} - Jumlah: {item.Jumlah} - Mode: {currentMode}\n\nMasukkan jumlah {oppositeMode} (maks: {item.Jumlah}):",
                    accept: "Lanjutkan",
                    cancel: "Batal",
                    placeholder: "1",  // Defaultnya 1
                    maxLength: 3,
                    keyboard: Keyboard.Numeric);

                if (jumlahInput != null)
                {
                    if (int.TryParse(jumlahInput, out int jumlahBaru) && jumlahBaru > 0 && jumlahBaru <= item.Jumlah)
                    {
                        // Buat item baru dengan jumlah yang dipilih dan mode pesanan yang berlawanan
                        var newItem = new KeranjangItem
                        {
                            IdProduk = item.IdProduk,
                            IdProdukSell = item.IdProdukSell,
                            NamaProduk = item.NamaProduk,
                            HargaJual = item.HargaJual,
                            UrlGambar = item.UrlGambar,
                            StokTersedia = item.StokTersedia
                        };

                        // Tetapkan mode pesanan berlawanan ke item baru
                        if (item.IkonModePesanan == "takeaway.png")
                        {
                            newItem.IkonModePesanan = "dine.png";  // Jika sekarang takeaway, yang baru jadi dine-in
                        }
                        else
                        {
                            newItem.IkonModePesanan = "takeaway.png";  // Jika sekarang dine-in, yang baru jadi takeaway
                        }

                        newItem.Jumlah = jumlahBaru;

                        // Kurangi jumlah item lama
                        item.Jumlah -= jumlahBaru;

                        // Tambahkan item baru ke keranjang
                        keranjang.Add(newItem);

                        // Debug output
                        System.Diagnostics.Debug.WriteLine($"Item {item.NamaProduk}: {jumlahBaru} item dipecah dari {currentMode} ke {oppositeMode}");
                    }
                    else
                    {
                        await DisplayAlert("Input Tidak Valid", $"Jumlah harus antara 1 dan {item.Jumlah}.", "OK");
                    }
                }
            }
        }
        else
        {
            // Untuk item dengan jumlah 1, langsung tawarkan pengubahan mode
            string newMode = await DisplayActionSheet(
                $"Ubah Mode Pesanan - Saat ini: {currentMode}",
                "Batal",
                null,
                "Takeaway",
                "Dine-in");

            if (newMode != null && newMode != currentMode)
            {
                // Simpan mode pesanan lama untuk debugging
                string oldMode = item.IkonModePesanan;
                
                // Ubah mode pesanan berdasarkan pilihan
                if (newMode == "Takeaway")
                {
                    item.IkonModePesanan = "takeaway.png";
                }
                else if (newMode == "Dine-in")
                {
                    item.IkonModePesanan = "dine.png";
                }
                
                // Debug output sesuai permintaan
                System.Diagnostics.Debug.WriteLine($"Item {item.NamaProduk}: IkonModePesanan lama = {oldMode}, baru = {item.IkonModePesanan}");
            }
        }

        // Perbarui total belanja karena biaya takeaway mungkin berubah
        UpdateTotalBelanja();
    }


    public class data_biaya_takeaway
    {
        public double biaya_per_item { get; set; } = 0;
    }

    public class data_kategori()
    {
        public string id_kategori { get; set; } = string.Empty;
    }

    public class data_promo()
    {
        public string id_promo { get; set; } = string.Empty;
        public string nama_promo { get; set; } = string.Empty;
        public string pilihan_promo { get; set; } = string.Empty;
        public double nominal { get; set; } = 0;
        public double persen { get; set; } = 0;
        public double nilai_promo { get; set; } = 0;
        public double min_pembelian { get; set; } = 0;
        public string deskripsi { get; set; } = string.Empty;

    }

    public class list_produk
    {
        public string id_produk { get; set; } = string.Empty; 
        public string id_produk_sell { get; set; } = string.Empty;
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

    public class list_ppn
    {
        public string id_ppn { get; set; } = string.Empty;
        public double nilai_ppn { get; set; } = 0;
        public string keterangan { get; set; } = string.Empty;

    }

    private async void get_biaya_takeaway()
    {
        try
        {
            string url = App.API_HOST + "takeaway/biaya.php";
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    List<data_biaya_takeaway> rowData = JsonConvert.DeserializeObject<List<data_biaya_takeaway>>(json);

                    if (rowData != null && rowData.Count > 0)
                    {
                        data_biaya_takeaway row = rowData[0];
                        NILAI_PER_TAKEAWAY = row.biaya_per_item;
                    }
                    else
                    {

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

    private async Task get_ppn()
    {
        try
        {
            string url = App.API_HOST + "ppn/ppn_aktif.php";
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    List<list_ppn> rowData = JsonConvert.DeserializeObject<List<list_ppn>>(json);

                    if (rowData != null && rowData.Count > 0)
                    {
                        list_ppn row = rowData[0];
                        PPNText.Text = row.keterangan;
                        PERSENTASE_PPN = row.nilai_ppn;
                    }
                    else
                    {

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
                        SummaryNamaKonsumen.Text = row.nama_konsumen;
                        ID_KONSUMEN = row.id_konsumen;



                        image_info.Source = ImageSource.FromFile("check_green.png");
                        text_info.Text = $"Konsumen Ditemukan: {row.nama_konsumen}";

                    }
                    else
                    {
                        // Handle jumlah kosong dan grandtotal kosong
                        image_info.Source = ImageSource.FromFile("not_found.png");
                        text_info.Text = $"Konsumen tidak ditemuakan";
                        ID_KONSUMEN = "1";
                    }
                    InfoSearch.IsVisible = true;
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

            _listmetodepembayaran.Clear(); 


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
        // 1. Tampilkan loading indicator SEBELUM proses dimulai
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        CV_ListProduk.IsVisible = false; // Sembunyikan daftar produk lama

        try
        {
            string url = App.API_HOST + "produk/list_produk.php?id_kategori=" + ID_KATEGORI;
            using (HttpClient client = new HttpClient())
            {
                // Set timeout agar tidak menunggu terlalu lama
                client.Timeout = TimeSpan.FromSeconds(15);
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    List<list_produk> rowData = JsonConvert.DeserializeObject<List<list_produk>>(json);
                    _listproduk.Clear();

                    foreach (var produk in rowData)
                    {
                        var formated = "Rp " + ((int)produk.harga_jual).ToString("N0");
                        produk.new_harga_jual = formated;
                        if (produk.stok <= 0)
                        {
                            produk.opacity_produk = 0.3;
                            produk.enabled_produk = false;
                        }
                        _listproduk.Add(produk);
                    }

                    // Terapkan data baru ke CollectionView
                    CV_ListProduk.ItemsSource = null;
                    CV_ListProduk.ItemsSource = _listproduk;
                }
                else
                {
                    // Handle jika server tidak merespon dengan baik
                    await DisplayAlert("Gagal Terhubung", $"Server merespon dengan status: {response.StatusCode}", "OK");
                    CV_ListProduk.ItemsSource = null; // Kosongkan list jika gagal
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
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            CV_ListProduk.IsVisible = true; // Tampilkan kembali daftar produk (baik ada isinya maupun kosong)
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

    private async void get_data_promo()
    {
        string url = App.API_HOST + "promo/promo_aktif.php";
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            List<string> nama = JsonConvert.DeserializeObject<List<string>>(json);

            Picker_Promo.ItemsSource = nama;
            System.Diagnostics.Debug.WriteLine("Load Kategori Menu Berhasil");
        }
        else
        {
            await DisplayAlert("Error", "Gagal mendapatkan data kategori", "OK");
        }
    }

    private async void simpan_konsumen()
    {
        //staffID sementara nanti ganti sama temp login

        var data = new Dictionary<string, string>
                {
                    { "nama_konsumen", ENamaKonsumen.Text },
                    { "no_hp", ENomorHp.Text.Replace("-","") },
                    { "email", EEmail.Text },
                    { "alamat", EAlamat.Text }
                };

        var jsonData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var client = new HttpClient();
        string ip = App.API_HOST + "konsumen/new_konsumen.php";

        var response = await client.PostAsync(ip, jsonData);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

        if (responseObject["status"] == "duplikat")
        {
            InfoKonsumenRegister.IsVisible = true;
            text_infokonsumenregister.Text = "Nomor HP sudah terdaftar, silahkan gunakan nomor lain.";
            image_infokonsumenregister.Source = ImageSource.FromFile("not_found.png");
        }
        else if (responseObject["status"] == "success")
        {

            InfoKonsumenRegister.IsVisible = true;
            text_infokonsumenregister.Text = responseObject["message"];
            image_infokonsumenregister.Source = ImageSource.FromFile("check_green.png");
            ID_KONSUMEN = responseObject["id_konsumen"];

            //kosongkan form
            ENamaKonsumen.Text = string.Empty;
            ENomorHp.Text = string.Empty;
            EAlamat.Text = string.Empty;
            EEmail.Text = string.Empty;

        }
    }

    private async void Picker_Kategori_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        string nilaiTerpilih = picker.SelectedItem.ToString();

        string url = App.API_HOST + "kategori_menu/id_kategori.php?nama_kategori=" + nilaiTerpilih;
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

    private async void Picker_Promo_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;

        // Jika pengguna menghapus pilihan promo (kembali ke title "Pilih Promosi")
        if (picker.SelectedIndex == -1)
        {
            // Reset semua data & nilai promo
            ID_PROMO = "0";
            NILAI_PROMO = 0;
            PILIHAN_PROMO = string.Empty;
            MIN_PEMBELIAN = 0;
            PERSEN_PROMO = 0;
            NOMINAL_PROMO = 0;

            // Hitung ulang total tanpa promo
            UpdateTotalBelanja();
            return;
        }

        string nilaiTerpilih = picker.SelectedItem.ToString();
        string url = App.API_HOST + "promo/id_promo.php?nama_promo="+ nilaiTerpilih;

        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var promoData = JsonConvert.DeserializeObject<List<data_promo>>(json);

                    if (promoData != null && promoData.Any())
                    {
                        var promo = promoData[0];

                        // Simpan semua detail promo ke variabel kelas
                        ID_PROMO = promo.id_promo;
                        PILIHAN_PROMO = promo.pilihan_promo;
                        MIN_PEMBELIAN = promo.min_pembelian;
                        PERSEN_PROMO = promo.persen;
                        NOMINAL_PROMO = promo.nominal;


                        // Panggil method kalkulasi utama untuk menerapkan promo
                        UpdateTotalBelanja();
                    }
                    else
                    {
                        await DisplayAlert("Info", "Detail untuk promo yang dipilih tidak ditemukan.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Gagal mendapatkan data promo dari server.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error Promo", $"Terjadi kesalahan: {ex.Message}", "OK");
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
        GridModeKonsumen.IsVisible = false;

        LNavModePesanan.TextColor = Color.FromArgb("#FFFFFF");
        NavModePesanan.BackgroundColor = Color.FromArgb("#075E54");

        LNavProduk.TextColor = Color.FromArgb("#333");
        NavProduk.BackgroundColor = Color.FromArgb("#F2F2F2");

        LNavKonsumen.TextColor = Color.FromArgb("#333");
        NavKonsumen.BackgroundColor = Color.FromArgb("#F2F2F2");

        LNavPembayaran.TextColor = Color.FromArgb("#333");
        NavPembayaran.BackgroundColor = Color.FromRgba("#F2F2F2");

    }

    private async void Tap_Konsumen_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        GridModePesanan.IsVisible = false;
        GridProduk.IsVisible = false;
        lv_metodepembayaran.IsVisible = false;
        GridModeKonsumen.IsVisible = true;

        LNavModePesanan.TextColor = Color.FromArgb("#333");
        NavModePesanan.BackgroundColor = Color.FromArgb("#F2F2F2");

        LNavProduk.TextColor = Color.FromArgb("#333");
        NavProduk.BackgroundColor = Color.FromArgb("#F2F2F2");

        LNavKonsumen.TextColor = Color.FromArgb("#FFF");
        NavKonsumen.BackgroundColor = Color.FromArgb("#075E54");

        LNavPembayaran.TextColor = Color.FromArgb("#333");
        NavPembayaran.BackgroundColor = Color.FromRgba("#F2F2F2");
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
        GridModeKonsumen.IsVisible = false;

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
        lv_metodepembayaran.IsVisible = true; GridModeKonsumen.IsVisible = false;

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
        MODE_PESANAN_DINE = false;
        MODE_PESANAN_TA = true;
        // Ubah mode pesanan semua item di keranjang ke takeaway
        foreach (var item in keranjang)
        {
            item.IkonModePesanan = "takeaway.png";
        }
        
        // Update total belanja untuk menghitung ulang biaya takeaway
        UpdateTotalBelanja();
    }

    private void RadioDine_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {

        if (!e.Value) return;
        DenahMejaContainer.IsVisible = true;
        get_meja();
        Summary_ModePesanan.Text = "Dine-In";
        MODE_PESANAN_DINE = true;
        MODE_PESANAN_TA = false;

        // Ubah mode pesanan semua item di keranjang ke dine-in
        foreach (var item in keranjang)
        {
            item.IkonModePesanan = "dine.png";
        }
        
        // Update total belanja untuk menghitung ulang biaya takeaway
        UpdateTotalBelanja();
    }

    private void OnMejaTapped(object sender, TappedEventArgs e)
    {
        // Pastikan parameter dan elemen yang di-tap valid
        if (e.Parameter is list_meja mejaYangDiTap && sender is Border borderBaru)
        {
            // 1. Jika meja sudah terpakai, buka popup cekpesanan_modal
            if (mejaYangDiTap.in_used == 1)
            {
                // Buka popup CekPesanan_Modal dengan membawa nilai id_meja dan callback untuk refresh meja
                var cekPesananModal = new CekPesanan_Modal(mejaYangDiTap.id_meja, () => 
                {
                    // Panggil get_meja dari thread utama
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        get_meja(); // Panggil tanpa await karena ini async void
                    });
                });
                this.ShowPopup(cekPesananModal);
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

    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started)
        {
            // Simpan skala saat ini sebagai titik awal
            startScale = DenahMejaLayout.Scale;
        }
        if (e.Status == GestureStatus.Running)
        {
            // Hitung skala baru
            currentScale += (e.Scale - 1) * startScale;
            // Batasi agar tidak terlalu kecil atau terlalu besar
            currentScale = Math.Max(0.5, currentScale); // Minimal zoom 50%
            currentScale = Math.Min(3, currentScale);   // Maksimal zoom 300%

            // Terapkan skala baru ke AbsoluteLayout
            DenahMejaLayout.Scale = currentScale;
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
            var listMeja = JsonConvert.DeserializeObject<List<list_meja>>(json);

            // PENAMBAHAN 1: Handle jika tidak ada data meja dari API
            if (listMeja == null || !listMeja.Any())
            {
                DenahMejaLayout.Children.Clear(); // Bersihkan jika ada sisa
                await DisplayAlert("Informasi", "Tidak ada data meja yang tersedia.", "OK");
                return;
            }

            // PENAMBAHAN 2: Lakukan semua pembaruan UI di Main Thread untuk keamanan
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // 1. Bersihkan denah dan reset properti layout sebelum menambahkan yang baru
                DenahMejaLayout.Children.Clear();
                DenahMejaLayout.Scale = 1; // Reset zoom ke posisi normal
                currentScale = 1; // Reset variabel zoom
                startScale = 1;

                // Variabel untuk menghitung ukuran total denah yang dibutuhkan
                double maxPosX = 0;
                double maxPosY = 0;

                // 2. Looping setiap data meja dari API
                foreach (var meja in listMeja)
                {
                    // 3. Tentukan warna berdasarkan status 'in_used'
                    // Pastikan membandingkan dengan string "1", bukan integer 1
                    Color warnaMeja = meja.in_used == 1 ? Color.FromArgb("#E53935") : Color.FromArgb("#37474F");

                    // 4. Buat elemen UI untuk meja (Border + Label)
                    var labelNomor = new Label
                    {
                        Text = meja.nomor_meja.PadLeft(2, '0'),
                        TextColor = Colors.White,
                        FontSize = 24,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    };

                    var borderMeja = new Border
                    {
                        WidthRequest = 60,
                        HeightRequest = 60,
                        StrokeShape = new RoundRectangle { CornerRadius = 8 },
                        BackgroundColor = warnaMeja,
                        Content = labelNomor
                    };

                    // 5. Tambahkan gesture recognizer untuk event klik/tap
                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.CommandParameter = meja;
                    tapGesture.Tapped += OnMejaTapped;
                    borderMeja.GestureRecognizers.Add(tapGesture);

                    // 6. Konversi posisi X dan Y dari string ke double
                    double x = double.Parse(meja.pos_x);
                    double y = double.Parse(meja.pos_y);

                    // 7. Atur posisi dan ukuran elemen di AbsoluteLayout
                    AbsoluteLayout.SetLayoutBounds(borderMeja, new Rect(x, y, 60, 60));

                    // 8. Tambahkan elemen meja ke layout
                    DenahMejaLayout.Children.Add(borderMeja);

                    // Perbarui posisi maksimum untuk kalkulasi ukuran canvas
                    if (x + 60 > maxPosX) maxPosX = x + 60;
                    if (y + 60 > maxPosY) maxPosY = y + 60;
                }

                // SETELAH LOOP: Atur ukuran AbsoluteLayout agar ScrollView berfungsi
                DenahMejaLayout.WidthRequest = maxPosX + 20; // +20 untuk padding ekstra
                DenahMejaLayout.HeightRequest = maxPosY + 20;
            });
        }
        else
        {
            await DisplayAlert("Error", "Gagal memuat data meja", "OK");
        }
    }

    private async void RadioTipePembayaran_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value == false) return;
        var radioButton = sender as RadioButton;
        if (radioButton?.BindingContext is list_metodepembayaran selectedItem)
        {
            // 1. Simpan seluruh objek metode pembayaran yang dipilih
            metodeBayarTerpilih = selectedItem;

            // 2. Set state dasar
            ID_BAYAR = selectedItem.id_bayar;
            Summary_MetodeBayar.Text = selectedItem.kategori;

            // 3. Panggil UpdateTotalBelanja() untuk menghitung ulang semua biaya
            UpdateTotalBelanja();

            // 4. Logika untuk menampilkan pop-up (tetap sama)
            if (selectedItem.kategori == "Transfer")
            {
                this.ShowPopup(new MetodePembayaran.TransferBank_Modal(() =>
                {
                    OnPopupClosed();
                }));
            }
        }
    }

    private void RadioGuest_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        FormSearchKonsumen.IsVisible = false;
    }

    private void RadioMember_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        FormSearchKonsumen.IsVisible = true;

    }

    private void T_SearchNomor_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Ambil Entry yang sedang digunakan
        var entry = (Entry)sender;
        var text = e.NewTextValue ?? "";

        // 1. Mencegah infinite loop saat kita mengubah teks secara programatik
        entry.TextChanged -= T_SearchNomor_TextChanged;

        // 2. Hapus semua karakter non-digit (seperti tanda '-')
        string digitsOnly = new string(text.Where(char.IsDigit).ToArray());

        // 3. Terapkan format XXXX-XXXX-XXXX
        string formattedText = digitsOnly;
        if (digitsOnly.Length > 8)
        {
            // Format menjadi 0812-3456-7890
            formattedText = $"{digitsOnly.Substring(0, 4)}-{digitsOnly.Substring(4, 4)}-{digitsOnly.Substring(8)}";
        }
        else if (digitsOnly.Length > 4)
        {
            // Format menjadi 0812-3456
            formattedText = $"{digitsOnly.Substring(0, 4)}-{digitsOnly.Substring(4)}";
        }

        // 4. Update teks di Entry dan posisikan kursor di akhir
        entry.Text = formattedText;
        entry.CursorPosition = formattedText.Length;

        // 5. Daftarkan kembali event handler
        entry.TextChanged += T_SearchNomor_TextChanged;
    }

    private async void T_SearchNomor_Completed(object sender, EventArgs e)
    {
        var entry = (Entry)sender;
        string input = entry.Text ?? "";

        // Hilangkan karakter '-'
        string nomor = input.Replace("-", "");

        // Validasi: harus diawali '08' dan minimal 11 angka
        if (!nomor.StartsWith("08") || nomor.Length < 11)
        {
            await DisplayAlert("Validasi Nomor HP", "Nomor HP harus diawali '08' dan minimal 11 angka.", "OK");
            entry.Focus();
            return;
        }

        NOMORHP = nomor;
        get_konsumen();
    }

    private async void ENomorHp_TextChanged(object sender, TextChangedEventArgs e)
    {

        var entry = (Entry)sender;
        var text = e.NewTextValue ?? "";

        // 1. Mencegah infinite loop saat kita mengubah teks secara programatik
        entry.TextChanged -= ENomorHp_TextChanged;

        // 2. Hapus semua karakter non-digit (seperti tanda '-')
        string digitsOnly = new string(text.Where(char.IsDigit).ToArray());

        // 3. Terapkan format XXXX-XXXX-XXXX
        string formattedText = digitsOnly;
        if (digitsOnly.Length > 8)
        {
            // Format menjadi 0812-3456-7890
            formattedText = $"{digitsOnly.Substring(0, 4)}-{digitsOnly.Substring(4, 4)}-{digitsOnly.Substring(8)}";
        }
        else if (digitsOnly.Length > 4)
        {
            // Format menjadi 0812-3456
            formattedText = $"{digitsOnly.Substring(0, 4)}-{digitsOnly.Substring(4)}";
        }

        // 4. Update teks di Entry dan posisikan kursor di akhir
        entry.Text = formattedText;
        entry.CursorPosition = formattedText.Length;

        // 5. Daftarkan kembali event handler
        entry.TextChanged += ENomorHp_TextChanged;

    }

    private async void BDaftarMember_Clicked(object sender, EventArgs e)
    {
        // 1. Validasi Nama Konsumen
        if (string.IsNullOrWhiteSpace(ENamaKonsumen.Text))
        {
            await DisplayAlert("Validasi Gagal", "Nama konsumen tidak boleh kosong.", "OK");
            ENamaKonsumen.Focus();
            return; // Hentikan eksekusi jika kosong
        }

        // 2. Validasi Nomor HP
        if (string.IsNullOrWhiteSpace(ENomorHp.Text))
        {
            await DisplayAlert("Validasi Gagal", "Nomor HP tidak boleh kosong.", "OK");
            ENomorHp.Focus();
            return; // Hentikan eksekusi jika kosong
        }

        // 3. Validasi Email
        if (string.IsNullOrWhiteSpace(EEmail.Text))
        {
            await DisplayAlert("Validasi Gagal", "Email tidak boleh kosong.", "OK");
            EEmail.Focus();
            return; // Hentikan eksekusi jika kosong
        }

        // Jika semua validasi lolos, lanjutkan proses pendaftaran di sini
        simpan_konsumen();

    }

    private async void ENomorHp_Completed(object sender, EventArgs e)
    {
        var entry = (Entry)sender;
        string input = entry.Text ?? "";

        // Hilangkan karakter '-'
        string nomor = input.Replace("-", "");

        // Validasi: harus diawali '08' dan minimal 11 angka
        if (!nomor.StartsWith("08") || nomor.Length < 11)
        {
            await DisplayAlert("Validasi Nomor HP", "Nomor HP harus diawali '08' dan minimal 11 angka.", "OK");
            entry.Focus();
            return;
        }
    }

    private void T_Search_Completed(object sender, EventArgs e)
    {
       
        string searchText = T_Search.Text;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            CV_ListProduk.ItemsSource = _listproduk;
            return;
        }

        var produkYangDifilter = _listproduk
            .Where(produk => produk.nama_produk.ToLower().Contains(searchText.ToLower()))
            .ToList();

        CV_ListProduk.ItemsSource = produkYangDifilter;
    }

    private async void TapPotongan_Tapped(object sender, TappedEventArgs e)
    {
      
        string hasil = await DisplayPromptAsync(title: "Masukkan Potongan",message: "Masukkan jumlah potongan dalam nominal (Rp):", accept: "Terapkan", cancel: "Batal", placeholder: "0", keyboard: Keyboard.Numeric);

        if (hasil != null)
        {
            if (double.TryParse(hasil, out double potonganBaru) && potonganBaru >= 0)
            {
                NILAI_POTONGAN = potonganBaru;
                UpdateTotalBelanja();
            }
            else
            {
                await DisplayAlert("Input Tidak Valid", "Harap masukkan angka yang valid.", "OK");
            }
        }
    }
    // tombol checkout
    private async void Summary_TotalCheckout_Clicked(object sender, EventArgs e)
    {
        if (sender is Button image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        if (!keranjang.Any())
        {
            await DisplayAlert("Perhatian", "Keranjang belanja masih kosong.", "OK");
            return;
        }

        if(MODE_PESANAN_DINE && ID_MEJA == "0")
        {
            await DisplayAlert("Perhatian", "Silakan pilih nomor meja terlebih dahulu.", "OK");
            return;
        }

        if (SwitchInvoice.IsToggled)
        {
            switch (ID_BAYAR)
            {
                case "1":
                    await this.ShowPopupAsync(new MetodePembayaran.Tunai_Modal(this.grandTotalFinal, ProsesDanSimpanTransaksiAsync));
                    break;

                case "2":
                    await this.ShowPopupAsync(new MetodePembayaran.TransferBank_Modal(() =>
                    {
                        OnPopupClosed();
                    }));
                    break;

                case "3":
                    await this.ShowPopupAsync(new MetodePembayaran.Qris_Modal(() =>
                    {
                        OnPopupClosed();
                    }));
                    break;

                default:
                    await DisplayAlert("Perhatian", "Silakan pilih metode pembayaran terlebih dahulu.", "OK");
                    break;
            }
        }
        else
        {
            if(ID_MEJA == "0")
            {
                await DisplayAlert("Perhatian", "Bayar nanti hanya berlaku untuk Dine-in", "OK");
                return;
            }

            await SimpanSebagaiInvoiceAsync();
        }
    }

    private async Task SimpanPesananSementaraAsync()
    {
        try
        {
            // Kumpulkan data pesanan saat ini
            var transaksi = new TransaksiSementara
            {
                IdKonsumen = this.ID_KONSUMEN,
                IdMeja = this.ID_MEJA,
                IdBayar = this.ID_BAYAR,
                IdPromo = this.ID_PROMO,
                NilaiPotongan = this.NILAI_POTONGAN,
                ItemDiKeranjang = new List<KeranjangItem>(this.keranjang)
            };

            // Ubah menjadi format JSON
            string jsonTransaksi = JsonConvert.SerializeObject(transaksi, Formatting.Indented);

            // Tentukan lokasi dan nama file, lalu tulis isinya
            string targetFile = System.IO.Path.Combine(FileSystem.AppDataDirectory, "pesanan_sementara.json");
            await File.WriteAllTextAsync(targetFile, jsonTransaksi);

            System.Diagnostics.Debug.WriteLine("--> Pesanan sementara DISIMPAN/DIPERBARUI.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Gagal menyimpan pesanan sementara: {ex.Message}");
        }
    }

    private async Task MuatPesananSementaraAsync()
    {
        string targetFile = System.IO.Path.Combine(FileSystem.AppDataDirectory, "pesanan_sementara.json");

        if (File.Exists(targetFile))
        {
            try
            {
                string jsonTransaksi = await File.ReadAllTextAsync(targetFile);
                var transaksi = JsonConvert.DeserializeObject<TransaksiSementara>(jsonTransaksi);

                if (transaksi != null && transaksi.ItemDiKeranjang.Any())
                {
                    // Kembalikan semua state dari file ke halaman Anda
                    this.ID_KONSUMEN = transaksi.IdKonsumen;
                    this.ID_MEJA = transaksi.IdMeja;
                    this.ID_BAYAR = transaksi.IdBayar;
                    this.ID_PROMO = transaksi.IdPromo;
                    this.NILAI_POTONGAN = transaksi.NilaiPotongan;

                    this.keranjang.Clear();
                    foreach (var item in transaksi.ItemDiKeranjang)
                    {
                        this.keranjang.Add(item);
                    }

                    UpdateTotalBelanja(); // Refresh seluruh UI
                    System.Diagnostics.Debug.WriteLine("--> Pesanan sementara berhasil DIMUAT.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gagal memuat pesanan sementara: {ex.Message}");
            }
        }
    }

    private async Task ProsesDanSimpanTransaksiAsync(double uangDiterima)
    {
        // 1. Kumpulkan data untuk detail pembayaran
        var pembayaranDetail = new ProsesPembayaranDetailPayload
        {
            Subtotal = this.subtotal,
            BiayaPengemasan = this.totalBiayaTakeaway,
            ServiceCharge = this.BIAYA_ADMIN,
            PromoDiskon = this.NILAI_PROMO,
            PpnResto = this.nilaiPPN
        };

        // 2. Kumpulkan data untuk pembayaran utama
        var pembayaran = new ProsesPembayaranPayload
        {
            IdBayar = this.ID_BAYAR,
            IdUser = this.ID_USER,
            Status = this.STATUS_BAYAR.ToString(),
            JumlahDibayarkan = this.grandTotalFinal,       // Total tagihan
            JumlahUang = uangDiterima,                   // Uang dari konsumen
            Kembalian = uangDiterima - this.grandTotalFinal,
            ModelDiskon = "nominal",                     // Sesuai permintaan Anda
            NilaiNominal = this.NILAI_POTONGAN,            // Potongan manual
            TotalDiskon = this.NILAI_PROMO,                // Potongan dari promo
            PembayaranDetail = pembayaranDetail
        };

        // 3. Kumpulkan payload utama untuk dikirim
        var payload = new PesananPayload
        {
            IdUser = ID_USER,
            IdKonsumen = this.ID_KONSUMEN,
            TotalCart = this.grandTotalFinal,
            StatusCheckout = this.STATUS_BAYAR.ToString(), // status 0 atau 1 dari Switch
            IdMeja = this.ID_MEJA,
            DeviceId = "-",
            PesananDetail = new List<PesananDetailPayload>(),
            ProsesPembayaran = pembayaran // Masukkan objek pembayaran ke payload utama
        };

        // Isi detail pesanan (item keranjang)
        foreach (var item in keranjang)
        {
            payload.PesananDetail.Add(new PesananDetailPayload
            {
                IdProdukSell = item.IdProdukSell,
                Qty = item.Jumlah,
                TaDinein = (item.IkonModePesanan == "takeaway.png") ? "1" : "0"
            });
        }


        string endpoint;
        if (this.STATUS_BAYAR == 1) // Bayar Langsung
        {
            endpoint = "pesanan/simpan_pesanan.php";
        }
        else // Simpan sebagai Invoice (Belum Bayar)
        {
            endpoint = "pesanan/simpan_invoice.php";
        }
        string url = App.API_HOST + endpoint;

        // 4. Serialisasi dan kirim ke API
        string jsonPayload = JsonConvert.SerializeObject(payload);
        System.Diagnostics.Debug.WriteLine("================ JSON DIKIRIM KE SERVER ================");
        System.Diagnostics.Debug.WriteLine(jsonPayload);
        System.Diagnostics.Debug.WriteLine("==========================================================");

        try
        {
            using (HttpClient client = new HttpClient())
            {
               
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Sukses", "Transaksi berhasil disimpan!", "OK");
                    HapusPesananSementara();
                    ResetHalaman();
                    
                    // Perbarui data meja dan produk setelah transaksi selesai - harus di main thread
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await get_listproduk();
                        get_meja();
                    });
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Gagal", $"Gagal menyimpan transaksi ke server. Pesan: {errorContent}", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error Proses Dan Simpan", $"Terjadi kesalahan jaringan: {ex.Message}", "OK");
        }
    }

    private async Task SimpanSebagaiInvoiceAsync()
    {
        // 1. Buat payload pembayaran MINIMAL yang dibutuhkan oleh simpan_invoice.php
        var pembayaran = new ProsesPembayaranPayload
        {
            IdUser = this.ID_USER
            // Properti lain sengaja dibiarkan kosong/default (akan menjadi null)
        };

        // 2. Buat payload utama
        var payload = new PesananPayload
        {
            IdUser = ID_USER,
            IdKonsumen = this.ID_KONSUMEN,
            TotalCart = this.grandTotalFinal,
            StatusCheckout = this.STATUS_BAYAR.ToString(), // Akan bernilai "0"
            IdMeja = this.ID_MEJA,
            DeviceId = "-",
            PesananDetail = new List<PesananDetailPayload>(),
            ProsesPembayaran = pembayaran
        };

        // Isi detail pesanan (item keranjang)
        foreach (var item in keranjang)
        {
            payload.PesananDetail.Add(new PesananDetailPayload
            {
                IdProdukSell = item.IdProdukSell,
                Qty = item.Jumlah,
                TaDinein = (item.IkonModePesanan == "takeaway.png") ? "1" : "0"
            });
        }

        // 3. Serialisasi JSON dan PERINTAHKAN UNTUK MENGABAIKAN NILAI NULL
        string jsonPayload = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        });

        // 4. Kirim ke endpoint KHUSUS untuk invoice
        string url = App.API_HOST + "pesanan/simpan_invoice.php";

        System.Diagnostics.Debug.WriteLine("================ JSON INVOICE (BERSIH) DIKIRIM KE SERVER ================");
        System.Diagnostics.Debug.WriteLine(jsonPayload);
        System.Diagnostics.Debug.WriteLine("========================================================================");

        try
        {
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Sukses", "Invoice berhasil disimpan!", "OK");
                    HapusPesananSementara();
                    ResetHalaman();
                    
                    // Perbarui data meja dan produk setelah transaksi selesai - harus di main thread
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await get_listproduk();
                        get_meja();
                    });
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Gagal", $"Gagal menyimpan invoice. Pesan: {errorContent}", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error Invoice", $"Terjadi kesalahan jaringan: {ex.Message}", "OK");
        }
    }

    // Method bantuan untuk mereset halaman
    private void ResetHalaman()
    {
        keranjang.Clear();
        ID_KONSUMEN = "1";
        ID_MEJA = "0";
        ID_BAYAR = "1";
        ID_PROMO = "0";
        NILAI_POTONGAN = 0;
       
        // Tap_Konsumen_Tapped(NavKonsumen, null);

        // Update tampilan
        UpdateTotalBelanja();
    }

    public void HapusPesananSementara()
    {
        try
        {
            // 1. Tentukan lokasi file
            string targetFile = System.IO.Path.Combine(FileSystem.AppDataDirectory, "pesanan_sementara.json");

            // 2. Cek apakah file tersebut ada
            if (File.Exists(targetFile))
            {
                // 3. Jika ada, hapus file tersebut
                File.Delete(targetFile);
                System.Diagnostics.Debug.WriteLine("--> File pesanan sementara berhasil dihapus.");
            }
        }
        catch (Exception ex)
        {
            // Catat error jika gagal menghapus, tidak perlu mengganggu user
            System.Diagnostics.Debug.WriteLine($"Gagal menghapus file pesanan sementara: {ex.Message}");
        }
    }

    private void SwitchInvoice_Toggled(object sender, ToggledEventArgs e)
    {
        STATUS_BAYAR = e.Value ? 1 : 0;
        if(e.Value)
        {
            LabelSwitch.Text = "Bayar Langsung";
        }
        else
        {
            LabelSwitch.Text = "Pembayaran Nanti (Invoice)";
        }

    }


    private async Task InisialisasiDariPesananAsync(List<CekPesanan_Modal.PesananDetailInfo> pesananDetail, string idMeja)
    {
        // Panggil dan TUNGGU semua data yang dibutuhkan, terutama PPN
        await get_ppn();

        // Method lain bisa jalan paralel jika tidak ada ketergantungan
        get_data_kategori();
        get_data_promo();
        get_biaya_takeaway();
        get_listproduk();
        get_metode_pembayaran();

        // Isi keranjang dari data pesanan
        foreach (var item in pesananDetail)
        {
            var newItem = new KeranjangItem
            {
                IdProduk = item.IdProdukSell,
                IdProdukSell = item.IdProdukSell,
                NamaProduk = item.NamaProduk,
                HargaJual = item.HargaJual,
                Jumlah = item.Qty,
                UrlGambar = App.IMAGE_HOST + item.KodeProduk + ".jpg",
                IkonModePesanan = (item.TaDinein == "1") ? "takeaway.png" : "dine.png"
            };
            keranjang.Add(newItem);
        }

        // Update ID_MEJA jika diperlukan
        if (idMeja != "0")
        {
            this.ID_MEJA = idMeja;
        }

        // Panggil UpdateTotalBelanja SETELAH get_ppn() dijamin selesai
        UpdateTotalBelanja();
        
        System.Diagnostics.Debug.WriteLine($"ProdukMenu initialized from order data with KODE_PAYMENT: {this.KODE_PAYMENT}");
    }
}