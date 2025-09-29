TransferBank_Modal.xaml & TransferBank_Modal.xaml.cs, pada bagian

baca dengan seksama dan pahami permintaan saya
sepertinya berhasil namun perlu koreksi
 
 pada Summary_TotalCheckout_Clicked bagian line 1686
 case "2":

await ProsesDanSimpanTransaksiAsync(this.grandTotalFinal);
break;

step yang benar
await ProsesDanSimpanTransaksiAsync(this.grandTotalFinal); <-- ini sudah benar namun jika success dapatkan  kode_payment dari output jsonnya

kemudian isi ke variable KODE_PAYMENT,
kemudian membuka transferbank_modal.xaml.cs
 await this.ShowPopupAsync(new MetodePembayaran.TransferBank_Modal(this.KODE_PAYMENT, (isSuccess, message) =>
{
    OnPopupClosed();
    // Tambahkan logika untuk menangani hasil dari modal di sini
    if (isSuccess)
    {
        // Penanganan jika berhasil
        System.Diagnostics.Debug.WriteLine($"Transfer berhasil: {message}");
    }
    else
    {
        // Penanganan jika gagal
        System.Diagnostics.Debug.WriteLine($"Transfer gagal: {message}");
    }
}));




