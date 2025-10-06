using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Diagnostics;
namespace Resto2025.History;

public partial class Riwayat : ContentPage
{
    private string? TGL_PAYMENT;

	public Riwayat()
	{
		InitializeComponent();
		DatePicker_tgl.MaximumDate = DateTime.Today;
		DatePicker_tgl.Date = DateTime.Today;
		TGL_PAYMENT = DatePicker_tgl.Date.ToString("yyyy-MM-dd");
	}

	private void OnDatePickerSelected(object? sender, DateChangedEventArgs e)
	{
		TGL_PAYMENT = e.NewDate.ToString("yyyy-MM-dd");
		System.Diagnostics.Debug.WriteLine($"tanggal:{TGL_PAYMENT}");

	}
}