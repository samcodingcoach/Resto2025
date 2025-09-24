using CommunityToolkit.Maui.Views;

namespace Resto2025.MetodePembayaran;

public partial class Tunai_Modal : Popup
{
    private double totalBelanja = 0;
  
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

  
    private void Keypad_Clicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        int digit = int.Parse(button.Text);

        // Menggabungkan angka secara matematis (misal: 123 menjadi 1234 saat 4 ditekan)
        uangKonsumenValue = (uangKonsumenValue * 10) + digit;
        UpdateDisplay();
    }

    private void Nominal_Clicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        // Hapus titik ribuan dan konversi ke double
        double nominalValue = double.Parse(button.Text.Replace(".", ""));

        // Tambahkan nilai nominal ke total uang konsumen
        uangKonsumenValue += nominalValue;
        UpdateDisplay();
    }

    private void Clear_Clicked(object sender, EventArgs e)
    {
        uangKonsumenValue = 0;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
       
        L_UangKonsumen.Text = $"Rp {uangKonsumenValue:N0}";

        double kembalian = uangKonsumenValue - this.totalBelanja;

        if (kembalian >= 0)
        {
            L_Kembalian.Text = $"Rp {kembalian:N0}";
        }
        else
        {
            L_Kembalian.Text = "Rp 0";
        }

        // Cek apakah uang konsumen sudah cukup untuk membayar total belanja
        if (uangKonsumenValue >= this.totalBelanja)
        {
            BBayar.IsEnabled = true;
            BBayar.Opacity = 1;
        }
        else
        {
            BBayar.IsEnabled = false;
            BBayar.Opacity = 0.2;
        }
    }

    private void CheckPrint_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {

    }
}