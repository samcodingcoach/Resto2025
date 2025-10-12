using CommunityToolkit.Maui.Views;

namespace Resto2025.Beranda;

public partial class DetailBatal : Popup
{	
	public DetailBatal(string nm_produk, string alasan, string waktu, string harga, string mode_pesanan, string kode_payment)
	{
		InitializeComponent();
		
		// Set data to UI elements
		L_KodePayment.Text = kode_payment;
		L_NamaProduk.Text = nm_produk;
		L_HargaJual.Text = $"- {harga}";
		L_Waktu.Text = waktu;
		L_ModePesanan.Text = mode_pesanan;
		L_Alasan.Text = alasan;
	}

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        this.Close();
    }
}