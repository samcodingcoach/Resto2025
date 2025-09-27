CekPesanan_Modal.xaml.cs
saat klik Submit_Bayar_Clicked
if(Submit_Bayar.Text == "BAYAR")
		{
			if (cekPesananData?.Pesanan != null && cekPesananData.Pesanan.PesananDetail != null)
			{
				// Navigasi kembali ke ProdukMenu dan isi keranjang dengan data dari pesanan
				var produkMenu = new ProdukMenu(cekPesananData.Pesanan.PesananDetail, cekPesananData.Pesanan.IdMeja);
				
				// Pindah ke halaman ProdukMenu
				Application.Current.MainPage = new NavigationPage(produkMenu);
				
				// Tutup popup ini
				Close();
			}
		}


		var produk menu buatkan debug consolenya.WriteLine
	    bawa juga kode_payment, dan simpan juga di public string KODE_PAYMENT (ProdukMenu.xaml.cs)