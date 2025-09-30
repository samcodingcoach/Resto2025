# Dokumentasi Arsitektur Codebase Resto2025

## Ringkasan Proyek
**Resto2025** adalah aplikasi Point of Sale (POS) untuk restoran yang dibangun menggunakan **.NET MAUI (Multi-platform App UI)**. Aplikasi ini mendukung platform Android, iOS, macOS Catalyst, dan Windows dengan target framework .NET 8.0.

## Teknologi & Framework

### Platform Target
- **Android** (min: API 21)
- **iOS** (min: 11.0)
- **macOS Catalyst** (min: 13.1)
- **Windows** (min: 10.0.17763.0)

### Dependencies Utama
```xml
- CommunityToolkit.Maui (v9.1.1) - UI toolkit tambahan
- Microsoft.Maui.Controls - Framework UI utama
- Newtonsoft.Json (v13.0.3) - Serialisasi/deserialisasi JSON
- SkiaSharp (v3.119.0) - Rendering grafis
- System.Net.Http (v4.3.4) - HTTP client untuk API calls
```

### Font Kustom
- **Lexend-Light.ttf** (FontRegular)
- **Lexend-SemiBold.ttf** (FontBold)

## Arsitektur Aplikasi

### 1. Entry Point & Konfigurasi

#### App.xaml.cs
File ini adalah entry point utama aplikasi dengan konfigurasi berikut:

**Konfigurasi API:**
```csharp
- API_HOST: https://resto.samdev.org/_resto007/api/
- IMAGE_HOST: https://resto.samdev.org/_resto007/public/images/
```

**Halaman Awal:**
- Aplikasi langsung membuka `ProdukMenu` sebagai halaman utama
- Menggunakan `NavigationPage` untuk navigasi

**Variabel Global:**
- `ID_USER = "4"` (ID pengguna yang sedang login - sementara hardcoded)

#### MauiProgram.cs
Konfigurasi builder aplikasi:
- Menggunakan `CommunityToolkit.Maui`
- Konfigurasi font kustom
- Debug logging untuk development

### 2. Struktur Folder & Modul

```
Resto2025/
├── Transaksi/              # Modul transaksi & pesanan
│   ├── ProdukMenu.xaml     # Halaman utama POS
│   ├── ProdukMenu.xaml.cs  # Logic halaman POS
│   ├── CekPesanan_Modal.xaml
│   └── CekPesanan_Modal.xaml.cs
├── MetodePembayaran/       # Modul pembayaran
│   ├── Tunai_Modal.xaml
│   ├── Tunai_Modal.xaml.cs
│   ├── TransferBank_Modal.xaml
│   ├── TransferBank_Modal.xaml.cs
│   ├── Qris_Modal.xaml
│   └── Qris_Modal.xaml.cs
├── Platforms/              # Kode spesifik platform
├── Resources/              # Asset (fonts, images, dll)
└── Properties/             # Properti proyek
```

## 3. Modul Inti Aplikasi

### A. Modul Transaksi - ProdukMenu

**File:** `Transaksi/ProdukMenu.xaml.cs`

**Fungsi Utama:**
Halaman POS lengkap dengan 4 bagian utama:

#### Navigation Tabs:
1. **KONSUMEN** - Manajemen pelanggan
2. **MODE PESANAN** - Pilih Takeaway atau Dine-in
3. **MENU PRODUK** - Katalog produk
4. **PEMBAYARAN** - Metode pembayaran

#### Fitur Detail:

**1. Manajemen Konsumen**
- Mode Guest atau Member
- Search member berdasarkan nomor HP (format: XXXX-XXXX-XXXX)
- Registrasi member baru (nama, HP, email, alamat)
- Validasi nomor HP (harus diawali "08", minimal 11 digit)

**2. Mode Pesanan**
- **Takeaway**: Pesanan dibawa pulang
- **Dine-in**: Makan di tempat dengan pilihan meja
- Denah meja interaktif:
  - Support zoom (pinch gesture)
  - Status meja: tersedia (#37474F) atau terpakai (#FF2D2D)
  - Posisi meja dinamis dari database (pos_x, pos_y)

**3. Katalog Produk**
- Filter berdasarkan kategori
- Search produk by nama
- Grid layout 5 kolom
- Info stok realtime
- Gambar produk dari server
- Validasi stok saat add to cart

**4. Keranjang Belanja**

**Model Data:**
```csharp
KeranjangItem {
    - IdProduk, IdProdukSell
    - NamaProduk, HargaJual
    - Jumlah (quantity)
    - IkonModePesanan (takeaway.png / dine.png)
    - IsFrozen (untuk pesanan existing)
    - StokTersedia
    - Subtotal (kalkulasi otomatis)
}
```

**Fitur Keranjang:**
- Edit jumlah item
- Ubah mode pesanan per item (Takeaway ↔ Dine-in)
- Pecah item (split qty dengan mode berbeda)
- Hapus item
- Item frozen tidak bisa diedit (dari pesanan sebelumnya)

**5. Kalkulasi Harga (UpdateTotalBelanja)**

**Formula:**
```
Total Produk = Σ (HargaJual × Qty)
Biaya Takeaway = Jumlah Item Takeaway × NILAI_PER_TAKEAWAY
Biaya Admin = 
  - QRIS: (subtotal dasar) × (persen biaya admin)
  - Lainnya: nilai tetap
Nilai Promo = 
  - Persen: Total Produk × (persen_promo / 100)
  - Nominal: nominal_promo
Subtotal = Total Produk + Biaya Takeaway + Biaya Admin - Nilai Promo - Nilai Potongan
PPN = Subtotal × (PERSENTASE_PPN / 100)
Grand Total = FLOOR(Subtotal + PPN, per ratusan)
```

**Validasi Promo:**
- Cek minimum pembelian
- Pilihan promo: persen atau nominal

**6. Proses Checkout**

**Validasi:**
- Keranjang tidak boleh kosong
- Harus pilih metode pembayaran
- Validasi meja untuk Dine-in

**Flow Pembayaran:**

**A. Tunai (via Tunai_Modal)**
- Keypad input uang konsumen
- Shortcut nominal (10.000, 20.000, 50.000, 100.000)
- Auto-calculate kembalian
- Button aktif jika uang >= total

**B. Transfer Bank (via TransferBank_Modal)**
- Pilih bank (BRI, BCA, Mandiri, dll)
- Input nama pengirim
- Input nomor referensi (optional)
- Upload bukti transfer (screenshot)
- Kirim ke API: `pembayaran/simpan_bank.php`

**C. QRIS (via Qris_Modal)**
- Scan QR code untuk pembayaran

**7. Payload API**

**Simpan Pesanan:**
```json
{
  "id_user": "4",
  "id_konsumen": "1",
  "total_cart": 50000,
  "status_checkout": "0",
  "id_meja": "3",
  "deviceid": "xxx",
  "pesanan_detail": [
    {
      "id_produk_sell": "12",
      "qty": 2,
      "ta_dinein": "0",
      "is_frozen": false
    }
  ],
  "proses_pembayaran": {
    "id_bayar": "1",
    "id_user": "4",
    "status": "1",
    "jumlah_uang": 100000,
    "jumlah_dibayarkan": 50000,
    "kembalian": 50000,
    "model_diskon": "nominal",
    "nilai_nominal": 5000,
    "total_diskon": 5000,
    "kode_payment": "PAY20250930001",
    "pembayaran_detail": {
      "subtotal": 45000,
      "biaya_pengemasan": 5000,
      "service_charge": 0,
      "promo_diskon": 5000,
      "ppn_resto": 5000
    }
  }
}
```

**8. Transaksi Sementara**

**Fitur Auto-save:**
- Menyimpan keranjang ke `Preferences` secara otomatis
- Format JSON dengan key: `transaksi_sementara_{deviceId}`
- Restore otomatis saat buka aplikasi

**Data yang Disimpan:**
```csharp
{
  IdKonsumen, IdMeja, IdBayar, IdPromo,
  NilaiPotongan,
  ItemDiKeranjang: List<KeranjangItem>
}
```

**9. Resume Pesanan**

**Flow untuk Lanjutkan Pesanan:**
- Dari `CekPesanan_Modal` → ProdukMenu
- Membawa `kode_payment` dan data pesanan
- Item dari pesanan lama di-freeze (tidak bisa edit/hapus)
- Bisa tambah item baru
- Update pesanan ke server dengan `kode_payment` yang sama

### B. Modul Cek Pesanan - CekPesanan_Modal

**File:** `Transaksi/CekPesanan_Modal.xaml.cs`

**Fungsi:**
Popup modal untuk melihat detail pesanan di suatu meja.

**Fitur:**
1. **Info Pesanan:**
   - ID Tagihan
   - Nomor Meja
   - Nama Konsumen
   - Total Invoice
   - Durasi sejak dipesan

2. **List Item Pesanan:**
   - Nama produk + harga
   - Qty & mode (Takeaway/Dine-in)
   - Subtotal per item

3. **Status Pembayaran:**
   - Belum Bayar: Tombol "BAYAR" (merah)
   - Sudah Bayar: Tombol "RILIS MEJA" (hijau)

4. **Aksi:**
   - **BAYAR**: Navigasi ke ProdukMenu dengan data pesanan
   - **RILIS MEJA**: Panggil API `meja/aktifkan.php` untuk free meja

**API Endpoint:**
```
GET: pesanan/cek_pesanan.php?id_meja={id}
POST: meja/aktifkan.php
```

### C. Modul Metode Pembayaran

#### 1. Tunai_Modal

**File:** `MetodePembayaran/Tunai_Modal.xaml.cs`

**Fitur:**
- Keypad angka 0-9
- Tombol nominal cepat (10rb, 20rb, 50rb, 100rb)
- Clear button
- Auto-calculate kembalian
- Callback ke parent page saat berhasil

**Logic:**
```csharp
uangKonsumen = (uangKonsumen * 10) + digit
kembalian = uangKonsumen - totalBelanja
enable button jika uangKonsumen >= totalBelanja
```

#### 2. TransferBank_Modal

**File:** `MetodePembayaran/TransferBank_Modal.xaml.cs`

**Fitur:**
- Pilih metode: Transfer atau EDC
- Dropdown pilih bank
- Input nama pengirim (required)
- Input nomor referensi (optional)
- Upload bukti transfer (foto)
- MultipartFormData upload ke server

**Validasi:**
- Kode payment harus ada
- Bank harus dipilih
- Nama pengirim wajib diisi
- Nominal transfer valid

**API Endpoint:**
```
POST: pembayaran/simpan_bank.php
```

**Payload:**
```
- kode_payment
- transfer_or_edc (0=transfer, 1=EDC)
- nama_bank
- nama_pengirim
- nominal_transfer
- no_referensi (optional)
- img_ss (file upload)
```

#### 3. Qris_Modal

**File:** `MetodePembayaran/Qris_Modal.xaml.cs`

**Fungsi:**
Modal untuk pembayaran via QRIS (scan QR code).

## 4. API Endpoints

### Produk
```
GET: produk/list_produk.php?id_kategori={id}
```

### Kategori
```
GET: kategori_menu/list_kategori.php
GET: kategori_menu/id_kategori.php?nama_kategori={nama}
```

### Promo
```
GET: promo/promo_aktif.php
GET: promo/id_promo.php?nama_promo={nama}
```

### Meja
```
GET: meja/list_meja.php
POST: meja/aktifkan.php
```

### Konsumen
```
GET: konsumen/search_konsumen.php?nomor={hp}
POST: konsumen/new_konsumen.php
```

### Pembayaran
```
GET: pembayaran/list_metodebayar.php
POST: pembayaran/simpan_bank.php
```

### Pesanan
```
POST: pesanan/new_pesanan.php
GET: pesanan/cek_pesanan.php?id_meja={id}
```

### Takeaway & PPN
```
GET: takeaway/biaya.php
GET: ppn/ppn_aktif.php
```

## 5. Model Data Utama

### list_produk
```csharp
{
  id_produk, id_produk_sell,
  kode_produk, nama_produk,
  stok, harga_jual,
  url_gambar (computed: IMAGE_HOST + kode_produk + ".jpg")
}
```

### list_meja
```csharp
{
  id_meja, nomor_meja,
  in_used (0=tersedia, 1=terpakai),
  pos_x, pos_y (koordinat denah),
  warna (computed berdasarkan status)
}
```

### list_metodepembayaran
```csharp
{
  id_bayar, kategori,
  no_rek, biaya_admin,
  keterangan, aktif
}
```

### list_konsumen
```csharp
{
  id_konsumen,
  nama_konsumen,
  no_hp, email, alamat
}
```

## 6. State Management

### Variabel Global (App.xaml.cs)
- `API_HOST` - Base URL untuk API
- `IMAGE_HOST` - Base URL untuk gambar produk

### Variabel State (ProdukMenu)
```csharp
// Identitas
ID_USER, ID_KONSUMEN, ID_MEJA, ID_BAYAR, ID_PROMO, ID_KATEGORI

// Mode
MODE_PESANAN_DINE, MODE_PESANAN_TA

// Nilai
BIAYA_ADMIN, PERSENTASE_PPN, NILAI_PROMO, NILAI_POTONGAN,
NILAI_PER_TAKEAWAY, MIN_PEMBELIAN

// Promo
PERSEN_PROMO, NOMINAL_PROMO, PILIHAN_PROMO

// Data
keranjang (ObservableCollection<KeranjangItem>)
metodeBayarTerpilih

// Payment
KODE_PAYMENT, STATUS_BAYAR (0=belum, 1=sudah, 2=batal)
```

### Local Storage
- **Preferences API** untuk transaksi sementara
- Key: `transaksi_sementara_{deviceId}`

## 7. UI/UX Pattern

### Layout
- **Grid-based layout** dengan kolom responsif
- **Frame + Border** untuk card/box styling
- **CollectionView** untuk list produk (5 kolom grid)
- **ListView** untuk keranjang & list detail

### Navigasi
- Tab-based navigation dengan Frame sebagai tab buttons
- State visibility control (`IsVisible`)
- Modal popup untuk pembayaran

### Gesture
- `TapGestureRecognizer` untuk klik
- `PinchGestureRecognizer` untuk zoom denah meja
- Fade animation pada tap (opacity 0.3 → 1)

### Color Scheme
```
Primary: #075E54 (hijau teal)
Danger: #FF2D2D (merah)
Background: #F9F9F9 (abu muda)
Border: #F2F2F2
Text: #333 (gelap), #B3B3B3 (abu)
Meja Tersedia: #37474F
Meja Terpakai: #E53935
```

## 8. Best Practices yang Diterapkan

1. **Async/Await** untuk semua network calls
2. **INotifyPropertyChanged** untuk binding UI
3. **ObservableCollection** untuk auto-update list
4. **Try-Catch** untuk error handling
5. **HttpClient** timeout 15 detik
6. **Main Thread** untuk update UI dari background
7. **JSON serialization** dengan Newtonsoft.Json
8. **Validasi input** sebelum kirim ke server
9. **Loading indicator** saat fetch data
10. **Callback pattern** untuk komunikasi modal ↔ parent

## 9. Flow Aplikasi Lengkap

```
1. Buka App → ProdukMenu
2. Pilih Konsumen (Guest/Member)
3. Pilih Mode Pesanan (Takeaway/Dine-in)
   └─ Jika Dine-in: Pilih Meja dari denah
4. Filter Kategori → Browse Produk
5. Tap Produk → Tambah ke Keranjang
6. Edit Keranjang (qty, mode pesanan)
7. Pilih Metode Pembayaran
8. Pilih Promo (optional)
9. Input Potongan (optional)
10. Tap Checkout → Modal Pembayaran
    ├─ Tunai: Input uang → Kembalian
    ├─ Transfer: Pilih bank → Upload bukti
    └─ QRIS: Scan QR
11. Konfirmasi → Kirim ke Server
12. Success → Clear Keranjang / Invoice
```

## 10. Catatan Development

### Hardcoded Values (Perlu Diganti)
- `ID_USER = "4"` → Ganti dengan session login
- `ID_KONSUMEN = "1"` (default Guest)
- `ID_BAYAR = "1"` (default Tunai)

### TODO / Future Improvements
1. Implementasi login system
2. Printer integration untuk struk
3. Report & analytics
4. Multi-language support
5. Offline mode dengan local database
6. Push notification untuk order status
7. Integrasi QRIS payment gateway
8. Split bill untuk Dine-in
9. Diskon per item (tidak hanya global)
10. History transaksi

### Known Issues
- Denah meja memerlukan koordinat manual dari database
- Tidak ada sinkronisasi realtime untuk status meja
- Image caching bergantung pada UriImageSource default behavior

## Kesimpulan

Resto2025 adalah aplikasi POS yang komprehensif dengan fitur lengkap untuk manajemen pesanan restoran. Arsitektur menggunakan MVVM pattern dengan **.NET MAUI**, komunikasi backend via **RESTful API**, dan UI responsif untuk berbagai platform. Kode terstruktur dengan baik dengan separation of concerns antara UI, business logic, dan data layer.