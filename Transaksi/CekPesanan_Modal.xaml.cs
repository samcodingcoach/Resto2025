using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Views;
using System.Text;
using System.Threading.Tasks;

namespace Resto2025.Transaksi;

public class MejaData
{
	public string id_meja { get; set; }
	public string nomor_meja { get; set; }
}



public partial class CekPesanan_Modal : Popup
{
	public int ID_MEJA;
	string selectedMeja = string.Empty;
	string id_pesanan_terpilih = string.Empty;
	string id_pesanan_existing = string.Empty;
    private CekPesananResponse cekPesananData;
	private List<KeranjangItem> keranjangItems;
	private readonly Action _onMejaReleasedCallback;

	public CekPesanan_Modal(int idMeja, Action onMejaReleasedCallback = null)
	{
		ID_MEJA = idMeja;
		_onMejaReleasedCallback = onMejaReleasedCallback;
		InitializeComponent();
		
		// Panggil fungsi untuk mengambil data dari API
		Task.Run(async () => await LoadCekPesananDataAsync());
	}

	private async void CloseModal_Tapped(object sender, TappedEventArgs e)
	{
		if (sender is Image image)
		{
			await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
			await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
		}

		
		Close();
		
	}

	private async Task LoadCekPesananDataAsync()
	{
		try
		{
			string url = App.API_HOST + $"pesanan/cek_pesanan.php?id_meja={ID_MEJA}";
			using (HttpClient client = new HttpClient())
			{
				client.Timeout = TimeSpan.FromSeconds(15);
				HttpResponseMessage response = await client.GetAsync(url);

				if (response.IsSuccessStatusCode)
				{
					string json = await response.Content.ReadAsStringAsync();
					// Gunakan JsonSerializerSettings untuk mengkonversi tanggal dari string ke DateTime
					JsonSerializerSettings settings = new JsonSerializerSettings();
					settings.DateParseHandling = DateParseHandling.DateTime;
					cekPesananData = JsonConvert.DeserializeObject<CekPesananResponse>(json, settings);

					if (cekPesananData?.Pesanan != null)
					{
						// Ambil id_pesanan yang existing
						id_pesanan_existing = cekPesananData.Pesanan.IdPesanan;
						System.Diagnostics.Debug.WriteLine("ID Pesanan Existing: " + id_pesanan_existing);
						
						// Update UI di main thread
						MainThread.BeginInvokeOnMainThread(() =>
						{
							UpdateUI();
						});
					}
				}
				else
				{
				
					System.Diagnostics.Debug.WriteLine($"Gagal Terhubung: Tidak dapat mengambil data dari server. Status: {response.StatusCode}");
				}
			}
		}
		catch (Exception ex)
		{
		
			System.Diagnostics.Debug.WriteLine($"Error: Terjadi kesalahan saat mengambil data: {ex.Message}");
		}
	}

	private void UpdateUI()
	{
		if (cekPesananData?.Pesanan != null)
		{
			// Hitung total invoice dari SUM (qty * harga_jual) di pesanan_detail
			double totalInvoice = 0;
			if (cekPesananData.Pesanan.PesananDetail != null && cekPesananData.Pesanan.PesananDetail.Any())
			{
				totalInvoice = cekPesananData.Pesanan.PesananDetail.Sum(item => item.Qty * item.HargaJual);
			}
			
			// Update label-label UI sesuai permintaan
			if (L_TotalInvoice != null)
				L_TotalInvoice.Text = $"Rp {totalInvoice:N0}";
				
			if (L_NamaMember != null)
				L_NamaMember.Text = cekPesananData.Pesanan.NamaKonsumen;
				
			if (L_IDTAGIHAN != null)
				L_IDTAGIHAN.Text = cekPesananData.Pesanan.IdTagihan;
				
			if (L_IDMEJA != null && cekPesananData.Meja != null)
				L_IDMEJA.Text = cekPesananData.Meja.NomorMeja;

			// Tampilkan durasi sejak pesanan dibuat
			if (L_DURASI != null)
				L_DURASI.Text = cekPesananData.Pesanan.DurasiSejakDipesan;

			// Cek status pembayaran dan update tombol
			if (Submit_Bayar != null)
			{
				// Cek apakah ada pembayaran dan statusnya
				bool sudahDibayar = false;
				if (cekPesananData.Pesanan.Pembayaran != null && cekPesananData.Pesanan.Pembayaran.Count > 0)
				{
					// Cek apakah salah satu pembayaran memiliki status 1
					sudahDibayar = cekPesananData.Pesanan.Pembayaran.Any(p => p.Status == "1");
				}
				
				if (sudahDibayar)
				{
					// Jika sudah dibayar, ubah tombol menjadi "RILIS MEJA"
					Submit_Bayar.Text = "RILIS MEJA";
					Submit_Bayar.BackgroundColor = Color.FromArgb("#075E54");
				}
				else
				{
					// Jika belum dibayar, biarkan sebagai "BAYAR"
					Submit_Bayar.Text = "BAYAR";
					Submit_Bayar.BackgroundColor = Color.FromArgb("#FF2D2D");
				}
			}

			// Isi ListView LV_Keranjang dengan data dari pesanan_detail
			if (LV_Keranjang != null && cekPesananData.Pesanan.PesananDetail != null)
			{
				keranjangItems = new List<KeranjangItem>();
				
				foreach (var item in cekPesananData.Pesanan.PesananDetail)
				{
					keranjangItems.Add(new KeranjangItem
					{
						IdPesanan = item.IdPesanan,
						IdProdukSell = item.IdProdukSell,
						KodeProduk = item.KodeProduk,
						NamaProduk = item.NamaProduk,
						HargaJual = item.HargaJual,
						Qty = item.Qty,
						TaDinein = item.TaDinein,
						Ket = item.Ket,
						NamaKategori = item.NamaKategori
					});
				}
				
				LV_Keranjang.ItemsSource = keranjangItems;
				
				// Update total item berdasarkan sum dari Qty
				if (L_TotalItem != null)
				{
					int totalItem = keranjangItems.Sum(item => item.Qty);
					L_TotalItem.Text = $"Total Item: {totalItem}";
				}
			}
		}
	}

	// Model untuk struktur data dari API cek_pesanan.php
	public class MejaInfo
	{
		[JsonProperty("id_meja")]
		public string IdMeja { get; set; }

		[JsonProperty("nomor_meja")]
		public string NomorMeja { get; set; }

		[JsonProperty("in_used")]
		public string InUsed { get; set; }
	}

	public class PesananDetailInfo
	{
		[JsonProperty("id_pesanan")]
		public string IdPesanan { get; set; }

		[JsonProperty("id_produk_sell")]
		public string IdProdukSell { get; set; }

		[JsonProperty("kode_produk")]
		public string KodeProduk { get; set; }

		[JsonProperty("nama_produk")]
		public string NamaProduk { get; set; }

		[JsonProperty("harga_jual")]
		public double HargaJual { get; set; }

		[JsonProperty("qty")]
		public int Qty { get; set; }

		[JsonProperty("ta_dinein")]
		public string TaDinein { get; set; }

		[JsonProperty("ket")]
		public string Ket { get; set; }

		[JsonProperty("nama_kategori")]
		public string NamaKategori { get; set; }

		public string FormattedHargaJual => $"Rp {HargaJual:N0}";
	}

	public class PembayaranInfo
	{
		[JsonProperty("kode_payment")]
		public string KodePayment { get; set; }

		[JsonProperty("tanggal_payment")]
		public string TanggalPayment { get; set; }

		[JsonProperty("id_pesanan")]
		public string IdPesanan { get; set; }

		[JsonProperty("kategori")]
		public string Kategori { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("jumlah_uang")]
		public double JumlahUang { get; set; }

		[JsonProperty("jumlah_dibayarkan")]
		public double JumlahDibayarkan { get; set; }

		[JsonProperty("kembalian")]
		public double Kembalian { get; set; }

		[JsonProperty("model_diskon")]
		public string ModelDiskon { get; set; }

		[JsonProperty("nilai_persen")]
		public double NilaiPersen { get; set; }

		[JsonProperty("nilai_nominal")]
		public double NilaiNominal { get; set; }

		[JsonProperty("total_diskon")]
		public double TotalDiskon { get; set; }
	}

	public class PesananInfo
	{
		[JsonProperty("id_pesanan")]
		public string IdPesanan { get; set; }

		[JsonProperty("nama_konsumen")]
		public string NamaKonsumen { get; set; }

		[JsonProperty("tgl_cart")]
		public DateTime TglCart { get; set; }

		[JsonProperty("total_cart")]
		public double TotalCart { get; set; }

		[JsonProperty("id_meja")]
		public string IdMeja { get; set; }

		[JsonProperty("id_tagihan")]
		public string IdTagihan { get; set; }

		[JsonProperty("nomor_antri")]
		public string NomorAntri { get; set; }

		[JsonProperty("pesanan_detail")]
		public List<PesananDetailInfo> PesananDetail { get; set; }

		[JsonProperty("pembayaran")]
		public List<PembayaranInfo> Pembayaran { get; set; }

		public string FormattedTotalCart => $"Rp {TotalCart:N0}";
		
		public string DurasiSejakDipesan
		{
			get
			{
				var sekarang = DateTime.Now;
				var selisih = sekarang - TglCart;
				
				int jam = (int)selisih.TotalHours;
				int menit = (int)selisih.Minutes;
				
				return $"{jam}:{menit} Menit";
			}
		}
	}

	public class CekPesananResponse
	{
		[JsonProperty("meja")]
		public MejaInfo Meja { get; set; }

		[JsonProperty("pesanan")]
		public PesananInfo Pesanan { get; set; }
	}

	// Class untuk digunakan di ListView keranjang (mengikuti struktur dari ProdukMenu.xaml.cs)
	public class KeranjangItem : INotifyPropertyChanged
	{
		public string IdPesanan { get; set; }
		public string IdProdukSell { get; set; }
		public string KodeProduk { get; set; }
		public string NamaProduk { get; set; }
		public double HargaJual { get; set; }
		public int Qty { get; set; }
		public string TaDinein { get; set; }
		public string Ket { get; set; }
		public string NamaKategori { get; set; }

		public string UrlGambar => App.IMAGE_HOST + KodeProduk + ".jpg";
		public string FormattedHargaJual => $"Rp {HargaJual:N0}";
		
		public string ModePesananSource => (TaDinein == "1") ? "takeaway.png" : "dine.png";
		
		public double Subtotal => HargaJual * Qty;
		public string FormattedSubtotal => $"Rp {Subtotal:N0}";

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

       
    }

    private async void Submit_Bayar_Clicked(object sender, EventArgs e)
	{


		if (sender is Button image)
		{
			await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
			await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
		}


		if(Submit_Bayar.Text == "BAYAR")
		{
			if (cekPesananData?.Pesanan != null && cekPesananData.Pesanan.PesananDetail != null)
			{
				// Ambil kode payment dari pembayaran yang sudah dilakukan
				string kodePayment = string.Empty;
				if (cekPesananData.Pesanan.Pembayaran != null && cekPesananData.Pesanan.Pembayaran.Count > 0)
				{
					// Ambil kode payment dari pembayaran pertama
					kodePayment = cekPesananData.Pesanan.Pembayaran[0].KodePayment;
				}
				
				// Buat data untuk dikirim ke ProdukMenu
				var pesananData = new Dictionary<string, object>
				{
					{ "PesananDetail", cekPesananData.Pesanan.PesananDetail },
					{ "IdMeja", cekPesananData.Pesanan.IdMeja },
					{ "KodePayment", kodePayment }
				};
				
				Console.WriteLine($"Debug: Sending data with kode_payment: {kodePayment}");
				
				// Kirim message ke ProdukMenu dengan data pesanan
				MessagingCenter.Send<object, Dictionary<string, object>>(this, "LoadPesananData", pesananData);
				
				// Tutup popup ini
				Close();
				
				// Navigate ke tab Order (ProdukMenu) menggunakan Shell
				Shell.Current.GoToAsync("//Order");
			}
		}
		else if(Submit_Bayar.Text == "RILIS MEJA")
		{
			aktifkan_meja();
        }
    }

	private async void aktifkan_meja()
	{
		
		var data = new Dictionary<string, string>
				{
					{ "id_meja", ID_MEJA.ToString() }
				};

		var jsonData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
		var client = new HttpClient();
		string ip = App.API_HOST + "meja/aktifkan.php";

		var response = await client.PostAsync(ip, jsonData);
		var responseContent = await response.Content.ReadAsStringAsync();
		var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

		if (responseObject["status"] == "success")
		{
			
			//await DisplayAlert("Berhasil", responseObject["message"], "OK");
			
			Close();
			
			// Panggil callback untuk merefresh meja di parent page jika tersedia
			if (_onMejaReleasedCallback != null)
			{
				_onMejaReleasedCallback();
			}
		}
	}

    private async void B_ImportMeja_Clicked(object sender, EventArgs e)
    {

		if (sender is Button image)
		{
			await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
			await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
		}



        bool confirm = await Application.Current.MainPage.DisplayAlert("Konfirmasi", "Yakin untuk melakukan?", "Yes", "No");


        if (confirm)
		{
			update_import();
		}



	}

	private async void get_data_meja()
	{
		string url = App.API_HOST + "impor_meja/listmeja_aktif.php?id_meja=" + ID_MEJA;
		HttpClient client = new HttpClient();
		HttpResponseMessage response = await client.GetAsync(url);

		if (response.IsSuccessStatusCode)
		{
			string jsonContent = await response.Content.ReadAsStringAsync();
			var mejaList = System.Text.Json.JsonSerializer.Deserialize<List<MejaData>>(jsonContent);
			
			PickerMejaAktif.Items.Clear();
			foreach (var meja in mejaList)
			{
				PickerMejaAktif.Items.Add(meja.nomor_meja);
			}
		}
		else
		{
			
		}
	}

    private void CB_Import_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value == true)
        {
            Form_PickerMeja.IsVisible = true;
            //load data meja
            get_data_meja();
        }
        else
        {
            Form_PickerMeja.IsVisible = false;
        }
    }

    private void PickerMejaAktif_SelectedIndexChanged(object sender, EventArgs e)
    {
        //ambil id_pesanan dari meja yang dipilih
		selectedMeja = PickerMejaAktif.SelectedItem as string;
		
		System.Diagnostics.Debug.WriteLine("Selected Meja: " + selectedMeja);

		cek_id();
    }

	private async void cek_id()
	{

		var data = new Dictionary<string, string>
		{
			{ "id_meja", selectedMeja },

		};

		var jsonData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
		var client = new HttpClient();
		string ip = App.API_HOST + "impor_meja/ambil_id.php";

		var response = await client.PostAsync(ip, jsonData);
		var responseContent = await response.Content.ReadAsStringAsync();
		var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

		if (responseObject?["status"] == "success")
		{

			id_pesanan_terpilih = responseObject["id_pesanan"].ToString();
			System.Diagnostics.Debug.WriteLine("ID Pesanan Terpilih: " + id_pesanan_terpilih);
			
		}
		else
		{
			System.Diagnostics.Debug.WriteLine($"{responseObject?["message"]}");
        }
	}

    private async void update_import()
    {


        var data = new Dictionary<string, string>
        {
            { "id_pesanan", id_pesanan_existing },
            { "id_importer", id_pesanan_terpilih }

        };

        var jsonData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var client = new HttpClient();
        string ip = App.API_HOST + "impor_meja/import.php";

        var response = await client.PostAsync(ip, jsonData);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

        if (responseObject?["status"] == "success")
        {

            System.Diagnostics.Debug.WriteLine($"Info: {responseObject?["message"]} ");
			//refresh data listview
			LV_Keranjang.ItemsSource = null;
			UpdateUI();


        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"{responseObject?["message"]}");
        }
    }

}