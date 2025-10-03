saya rasa masih, dengan masalah sebelumnya
Mode 1
1. Invoice
2. Bayar Langsung
3. Invoice <- ini tidak tersimpan,


coba cari tau kenapa , setiap ingin melakukan pembayaran nanti jika sebelumnya bayar langsung tidak tersimpan untuk invoice. kecuali saya tutup aplikasi lalu mulai ulang.
adalah salah ketika reset

coba anda cek kenapa malah memanggil menjalankan update_invoice.php;

 if (string.IsNullOrEmpty(this.KODE_PAYMENT)) masalahnya kenapa menjalan kode ini pastikan KODE_PAYMENT variablenya dihapus_
        {
            // KODE_PAYMENT kosong = Transaksi baru
            endpoint = (this.STATUS_BAYAR == 1) ? "pesanan/simpan_pesanan.php" : "pesanan/simpan_invoice.php";
            System.Diagnostics.Debug.WriteLine("=== kode payment kosong ====");
        }
        else
        {
            // KODE_PAYMENT ada = Update pesanan/invoice yang sudah ada
            
            if(STATUS_BAYAR == 0)
            {
                endpoint = "pesanan/update_invoice.php";
                System.Diagnostics.Debug.WriteLine("=== UPDATE INVOICE ====");

            }
            else
            {
                endpoint = "pesanan/update_pesanan.php";
                System.Diagnostics.Debug.WriteLine("=== UPDATE PESANAN ====");
            }

        }