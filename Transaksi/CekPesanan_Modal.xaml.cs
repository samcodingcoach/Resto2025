using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Views;
using System.Text;

namespace Resto2025.Transaksi;

public partial class CekPesanan_Modal : Popup
{
	public int ID_MEJA;
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
						// Update UI di main thread
						MainThread.BeginInvokeOnMainThread(() =>
						{
							UpdateUI();
						});
					}
				}
				else
				{
					// Dalam konteks popup, kita bisa menangani error dengan cara lain
					// Misalnya menampilkan pesan error di komponen UI di popup atau hanya mencatatnya
					System.Diagnostics.Debug.WriteLine($"Gagal Terhubung: Tidak dapat mengambil data dari server. Status: {response.StatusCode}");
				}
			}
		}
		catch (Exception ex)
		{
			// Dalam konteks popup, kita bisa menangani error dengan cara lain
			// Misalnya menampilkan pesan error di komponen UI di popup atau hanya mencatatnya
			System.Diagnostics.Debug.WriteLine($"Error: Terjadi kesalahan saat mengambil data: {ex.Message}");
		}
	}

	private void UpdateUI()
	{
		if (cekPesananData?.Pesanan != null)
		{
			// Update label-label UI sesuai permintaan
			if (L_TotalInvoice != null)
				L_TotalInvoice.Text = cekPesananData.Pesanan.FormattedTotalCart;
				
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
				
				// Navigasi kembali ke ProdukMenu dan isi keranjang dengan data dari pesanan
				var produkMenu = new ProdukMenu(cekPesananData.Pesanan.PesananDetail, cekPesananData.Pesanan.IdMeja, kodePayment);
				
				// Pindah ke halaman ProdukMenu
				Application.Current.MainPage = new NavigationPage(produkMenu);
				
				// Tutup popup ini
				Close();
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

}