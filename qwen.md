pada CekPesanan_Modal.xaml.cs_
		
 private async void Submit_Bayar_Clicked(object sender, EventArgs e)
    {


		if (sender is Button image)
		{
			await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
			await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
		}


		if(Submit_Bayar.Text == "BAYAR")
		{

		}
		else if(Submit_Bayar.Text == "RILIS MEJA")
		{
			aktifkan_meja();
        }
    }

	fokus terhadap rilis meja, anda cek di aktif_meja pada bagian ini
	if (responseObject["status"] == "success")
		{
			
			//await DisplayAlert("Berhasil", responseObject["message"], "OK");
			

		}_

		tambahkan tutup modal dan perbarui/ refresh meja