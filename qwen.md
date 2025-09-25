setelah menyelesaikan tugas SimpanSebagaiInvoiceAsync(); atau ProsesDanSimpanTransaksiAsync();
lakukan refresh / reload terhadap get_meja() dan get_listproduk()
tujuan memperbarui data meja dan produk yang mungkin berubah setelah transaksi

================ JSON DIKIRIM KE SERVER ================
{"id_user":"4","id_konsumen":"1","total_cart":128400.0,"status_checkout":"1","id_meja":"21","deviceid":"-","pesanan_detail":[{"id_produk_sell":"512","qty":1,"ta_dinein":"0"},{"id_produk_sell":"514","qty":1,"ta_dinein":"0"},{"id_produk_sell":"513","qty":1,"ta_dinein":"0"}],"proses_pembayaran":{"id_bayar":"1","id_user":"4","status":"1","jumlah_uang":150000.0,"jumlah_dibayarkan":128400.0,"kembalian":21600.0,"model_diskon":"nominal","nilai_nominal":0.0,"total_diskon":0.0,"pembayaran_detail":{"subtotal":116750.0,"biaya_pengemasan":0.0,"service_charge":0.0,"promo_diskon":0.0,"ppn_resto":11675.0}}}
==========================================================
--> File pesanan sementara berhasil dihapus.
Exception thrown: 'System.Runtime.InteropServices.COMException' in WinRT.Runtime.dll
--> Pesanan sementara DISIMPAN/DIPERBARUI.
Exception thrown: 'System.Runtime.InteropServices.COMException' in System.Private.CoreLib.dll
Exception thrown: 'System.Runtime.InteropServices.COMException' in System.Private.CoreLib.dll

hasil debug, saya cek di database tetap tersimpan
error mengarah ke line 1798
tetapi get_meja() dan get_listproduk() tetap bekerja setelah proses selesai transaksi