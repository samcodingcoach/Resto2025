using CommunityToolkit.Maui.Views;

namespace Resto2025.MetodePembayaran;

public partial class Tunai_Modal : Popup
{
	public Tunai_Modal(double totalBelanja)
    {
        InitializeComponent();
        L_NilaiGrandTotal.Text = $"Rp {totalBelanja:N0}";
    }

    private void TapClose_Tapped(object sender, TappedEventArgs e)
    {
        Close();
    }

    private void CheckPrint_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {

    }
}