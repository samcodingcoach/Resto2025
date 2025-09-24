using CommunityToolkit.Maui.Views;

namespace Resto2025.MetodePembayaran;

public partial class Tunai_Modal : Popup
{
    private double totalBelanja = 0;
    //private string uangKonsumenString = "";
    private double uangKonsumenValue = 0;
    public Tunai_Modal(double totalBelanja)
    {
        InitializeComponent();
        this.totalBelanja = totalBelanja;
        L_NilaiGrandTotal.Text = $"Rp {totalBelanja:N0}";
    }

    private void TapClose_Tapped(object sender, TappedEventArgs e)
    {
        Close();
    }

    private void CheckPrint_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {

    }

    // Method untuk tombol angka (0-9) - Logika diubah menjadi numerik
    private void Keypad_Clicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        int digit = int.Parse(button.Text);

        // Menggabungkan angka secara matematis (misal: 123 menjadi 1234 saat 4 ditekan)
        uangKonsumenValue = (uangKonsumenValue * 10) + digit;
        UpdateDisplay();
    }

    // Method untuk tombol nominal cepat - Logika diubah menjadi PENJUMLAHAN
    private void Nominal_Clicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        // Hapus titik ribuan dan konversi ke double
        double nominalValue = double.Parse(button.Text.Replace(".", ""));

        // Tambahkan nilai nominal ke total uang konsumen
        uangKonsumenValue += nominalValue;
        UpdateDisplay();
    }

    // Method untuk tombol Clear (C) - Logika diubah menjadi reset angka
    private void Clear_Clicked(object sender, EventArgs e)
    {
        uangKonsumenValue = 0;
        UpdateDisplay();
    }

    // Method utama untuk update tampilan - Sekarang lebih sederhana
    private void UpdateDisplay()
    {
        // Update Label Uang Konsumen
        L_UangKonsumen.Text = $"Rp {uangKonsumenValue:N0}";

        // Hitung kembalian
        double kembalian = uangKonsumenValue - this.totalBelanja;

        // Tampilkan kembalian jika uangnya cukup
        if (kembalian >= 0)
        {
            L_Kembalian.Text = $"Rp {kembalian:N0}";
        }
        else
        {
            L_Kembalian.Text = "Rp 0";
        }
    }
}