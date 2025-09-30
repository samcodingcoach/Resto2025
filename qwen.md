Qris_Modal.xaml.cs

pada bagian kode dibawah ini line 47
private async void BGenerateQR_Clicked(object sender, EventArgs e)
    {


        if (sender is Image image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }

        // Panggil ProsesDanSimpanTransaksiAsync dari ProdukMenu
        if (_onGenerateQR != null)
        {
            await _onGenerateQR();
        }

    }

    setelah panggil proses dan simpan transaksi, tambahkan kode untuk menampilkan QR Code yang didapat dari
    kode dibawah ini ProdukMenu.xaml.cs line 1966 (URL_QRIS))
    if(ID_BAYAR == "3")
    {
        URL_QRIS = responseObject["qris_url"];
        System.Diagnostics.Debug.WriteLine($"URLQRIS = {URL_QRIS}");
    }

    lalu ditampilkan di Image QRCode di  Qris_Modal.xaml bagian 
    <Image x:Name="QrisWebView" HeightRequest="200" WidthRequest="200"></Image> di code behindnya pakai  QrisWebView.Source = ImageSource.FromUri(new Uri(...));


