using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Views;

namespace Resto2025.Transaksi;

public partial class CekPesanan_Modal : ContentPage
{
	public int ID_MEJA = 4;
	private CekPesananResponse cekPesananData;
	private List<KeranjangItem> keranjangItems;

	public CekPesanan_Modal()
	{
		InitializeComponent();
		
		// Panggil fungsi untuk mengambil data dari API
		Task.Run(async () => await LoadCekPesananDataAsync());
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
					cekPesananData = JsonConvert.DeserializeObject<CekPesananResponse>(json);

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
					await DisplayAlert("Gagal Terhubung", $"Tidak dapat mengambil data dari server. Status: {response.StatusCode}", "OK");
				}
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Terjadi kesalahan saat mengambil data: {ex.Message}", "OK");
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
		public string TglCart { get; set; }

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
}