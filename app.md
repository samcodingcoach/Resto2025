# Aplikasi Resto2025 - Sistem POS Restoran

## Deskripsi Umum
Resto2025 adalah aplikasi Point of Sale (POS) untuk restoran yang dibangun menggunakan .NET MAUI. Aplikasi ini dirancang untuk mengelola seluruh proses pemesanan dan pembayaran di restoran, termasuk pemesanan takeaway dan makan di tempat.

## Struktur Proyek
- **Transaksi/**: Berisi antarmuka utama sistem pemesanan (ProdukMenu)
- **MetodePembayaran/**: Berisi antarmuka modal untuk berbagai metode pembayaran
- **Resources/**: Berisi gambar, font, dan file sumber daya lainnya
- **Platforms/**: Berisi konfigurasi platform spesifik

## Fitur Utama

### 1. Manajemen Pelanggan
- Pemilihan pelanggan antara Guest (Tamu) atau Member
- Pencarian member berdasarkan nomor telepon
- Formulir pendaftaran member baru dengan validasi data

### 2. Jenis Pesanan
- **Takeaway**: Pesanan dibawa pulang dengan biaya tambahan per item
- **Dine-in**: Pemesanan makan di tempat dengan pemilihan meja
- Denah meja interaktif dengan kemampuan zoom

### 3. Manajemen Produk
- Katalog produk berdasarkan kategori
- Pencarian produk
- Validasi stok untuk mencegah penjualan berlebih
- Tampilan visual produk dengan gambar dan harga

### 4. Keranjang Belanja
- Fungsi tambah, edit jumlah, dan hapus item
- Validasi stok saat mengedit jumlah
- Perhitungan subtotal real-time

### 5. Sistem Pembayaran
- Bayar Langsung (Cash, Bank Transfer, QRIS)
- Bayar Nanti (Invoice) hanya berlaku untuk Dine-in
- Modal pembayaran yang berbeda untuk setiap metode
- Perhitungan kembalian untuk pembayaran tunai

### 6. Fitur Keuangan
- Perhitungan biaya admin yang bervariasi berdasarkan metode pembayaran
- Diskon promosi dengan syarat minimum pembelian
- Diskon manual yang dapat dimasukkan oleh pengguna
- Perhitungan PPN (Pajak Pertambahan Nilai)
- Biaya takeaway per item

## Arsitektur Aplikasi

### File Utama
1. **App.xaml & App.xaml.cs**
   - Konfigurasi sumber daya aplikasi dan gaya
   - Inisialisasi API_HOST dan IMAGE_HOST untuk koneksi backend
   - Pemilihan halaman utama sebagai Transaksi.ProdukMenu

2. **ProdukMenu.xaml & ProdukMenu.xaml.cs**
   - Antarmuka utama sistem POS
   - Layout dua kolom: navigasi dan ringkasan pesanan
   - Logika bisnis untuk seluruh proses pemesanan
   - Integrasi API untuk pengambilan data

3. **File Modal Pembayaran**
   - Tunai_Modal: Layar pembayaran tunai dengan perhitungan kembalian
   - TransferBank_Modal: Layar konfirmasi pembayaran transfer
   - Qris_Modal: Layar pembayaran QRIS

### Teknologi yang Digunakan
- .NET MAUI untuk pengembangan lintas platform
- CommunityToolkit.Maui untuk komponen UI lanjutan
- Newtonsoft.Json untuk serialisasi/deserialisasi data
- HttpClient untuk koneksi API
- SQLite (implisit) untuk penyimpanan data lokal

## Alur Penggunaan Aplikasi

1. **Pemilihan Pelanggan**
   - Pilih antara Guest atau Member
   - Jika Member, cari atau daftarkan member baru

2. **Pemilihan Mode Pesanan**
   - Pilih Takeaway atau Dine-in
   - Jika Dine-in, pilih nomor meja yang tersedia

3. **Pemilihan Produk**
   - Jelajahi produk berdasarkan kategori
   - Cari produk spesifik
   - Tap produk untuk menambahkan ke keranjang

4. **Manajemen Keranjang**
   - Lihat ringkasan pesanan
   - Edit jumlah atau hapus item
   - Lihat perhitungan biaya secara real-time

5. **Pemilihan Pembayaran**
   - Pilih metode pembayaran
   - Aplikasikan promosi atau diskon jika ada

6. **Checkout**
   - Pilih Bayar Langsung atau Bayar Nanti
   - Selesaikan proses pembayaran sesuai metode
   - Dapatkan bukti transaksi

## Integrasi API
Aplikasi terintegrasi dengan backend untuk:
- Pengambilan data produk dan kategori
- Pengambilan data member dan meja
- Pengambilan informasi promosi dan PPN
- Pengiriman data transaksi
- Pengambilan data metode pembayaran

## Fitur Keamanan dan Validasi
- Validasi input nomor telepon (format dan panjang)
- Validasi stok produk sebelum penambahan ke keranjang
- Validasi minimum pembelian untuk promosi
- Validasi nomor meja yang telah digunakan

## Penyimpanan Data Lokal
- Pesanan sementara disimpan secara lokal jika aplikasi ditutup
- Data dipulihkan saat aplikasi dibuka kembali
- File pesanan sementara dihapus setelah transaksi selesai

## Desain UI/UX
- Warna utama hijau (#075E54) untuk tampilan profesional
- Navigasi intuitif antara bagian pelanggan, pesanan, produk, dan pembayaran
- Responsif dan mendukung interaksi sentuh
- Umpan balik visual pada setiap interaksi pengguna