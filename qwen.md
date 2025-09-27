buatkan public class pada CekPesanan_Modal.xaml.cs untuk menampung informasi dari API berikut
_pesanan/cek_pesanan.php?id_meja=X
contoh output json
{
    "meja": {
        "id_meja": "4",
        "nomor_meja": "4",
        "in_used": "1"
    },
    "pesanan": {
        "id_pesanan": "849",
        "nama_konsumen": "Guest",
        "tgl_cart": "2025-09-27 09:35:18",
        "total_cart": "227900",
        "id_meja": "4",
        "id_tagihan": "INV-4-2509270001",
        "nomor_antri": "1",
        "pesanan_detail": [
            {
                "id_pesanan": "849",
                "id_produk_sell": "520",
                "kode_produk": "NGP",
                "nama_produk": "Nasi Goreng Premium",
                "harga_jual": "68900",
                "qty": "2",
                "ta_dinein": "0",
                "ket": "0",
                "nama_kategori": "Makanan"
            },
            {
                "id_pesanan": "849",
                "id_produk_sell": "519",
                "kode_produk": "CDL",
                "nama_produk": "Cendol 77 Samarinda",
                "harga_jual": "21600",
                "qty": "2",
                "ta_dinein": "0",
                "ket": "0",
                "nama_kategori": "Minuman"
            },
            {
                "id_pesanan": "849",
                "id_produk_sell": "518",
                "kode_produk": "BKS",
                "nama_produk": "Bakso Samarinda",
                "harga_jual": "26250",
                "qty": "1",
                "ta_dinein": "0",
                "ket": "0",
                "nama_kategori": "Makanan"
            }
        ],
        "pembayaran": [
            {
                "kode_payment": "INV-4-2509270001",
                "tanggal_payment": "2025-09-27 09:35:18",
                "id_pesanan": "849",
                "kategori": "Tunai",
                "status": "0",
                "jumlah_uang": "0",
                "jumlah_dibayarkan": "0",
                "kembalian": "0",
                "model_diskon": null,
                "nilai_persen": "0",
                "nilai_nominal": "0",
                "total_diskon": "0"
            }
        ]
    }
}

isi value
L_TotalInvoice.Text = total_cart
L_NamaMember.Text = nama_konsumen
L_IDTAGIHAN.Text = id_tagihan
L_IDMEJA.Text = nomor_meja

kemudian pada listview LV_Keranjang, tampilkan data dari pesanan_detail
berdasarkan binding nama_produk, harga_jual, qty, nama_kategori, untuk ket tidak usah ditampilkan
gunakan format harga jual dengan format "N0" contoh 227900 menjadi 227,900
"ta_dinein": "0" / "1" mengacu pada Take Away / Dine In

anda bisa lihat contoh pada ProdukMenu.xaml.cs dan ProdukMenu.xaml
