Perubahan yang telah dilakukan untuk memastikan nilai nominal_transfer yang benar dikirim ke server:

1. Dalam ProdukMenu.xaml.cs, case "2":
   - Sebelumnya: Mengirim this.grandTotalFinal langsung ke ProsesDanSimpanTransaksiAsync dan modal
   - Sekarang: Menyimpan nilai this.grandTotalFinal dalam variabel lokal sebelum diproses, 
             agar memastikan nilainya tetap konsisten saat dibuka modal TransferBank
   
   Kode yang diubah:
   ```csharp
   // Simpan nilai grandTotalFinal sebelum diproses
   double nominalTransfer = this.grandTotalFinal;
   await ProsesDanSimpanTransaksiAsync(nominalTransfer);
   
   // Buka modal TransferBank setelah kode pembayaran telah diperbarui dalam fungsi
   if (!string.IsNullOrEmpty(this.KODE_PAYMENT))
   {
       System.Diagnostics.Debug.WriteLine($"Mengirim nilai nominal transfer: {nominalTransfer} ke modal");
       await this.ShowPopupAsync(new MetodePembayaran.TransferBank_Modal(this.KODE_PAYMENT, nominalTransfer, (isSuccess, message) =>
   ```

2. Dalam TransferBank_Modal.xaml.cs:
   - Ditambahkan log tambahan untuk melihat nilai sebelum dikirim
   - Validasi hanya memastikan nilai bukan NaN atau Infinity, bukan memastikan nilai > 0

   Kode yang ditambahkan:
   ```csharp
   // Tambahkan log untuk debugging nilai yang akan dikirim
   Debug.WriteLine($"Nominal Transfer sebelum dikirim: {nominal_transfer}");
   ```

Perubahan ini akan membantu kita melacak dari mana nilai 0 berasal - apakah dari sisi MAUI atau dari sisi server.