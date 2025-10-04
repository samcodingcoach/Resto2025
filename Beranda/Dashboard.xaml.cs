using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
namespace Resto2025.Beranda;

public partial class Dashboard : ContentPage
{
	public Dashboard()
	{
		InitializeComponent();
	}



    public class list_summary
    {
        public string tanggal_open { get; set; } = string.Empty;
        public double qris { get; set; } = 0;
        public double transfer { get; set; } = 0;
        public double total_transaksi { get; set; } = 0;
        public double kas_awal { get; set; } = 0;
        public int jumlah_transaksi { get; set; } = 0;

    }

        
}