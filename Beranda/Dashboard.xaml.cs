using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
namespace Resto2025.Beranda;

public partial class Dashboard : ContentPage
{
    string ID_USER = "4"; //Preferences.Get("ID_USER", string.Empty);
    public Dashboard()
	{
		InitializeComponent();
        get_summary();
	}

   


    public class list_summary
    {
        public string tanggal_open { get; set; } = string.Empty;
        public double qris { get; set; } = 0;
        public double transfer { get; set; } = 0;
        public double total_transaksi { get; set; } = 0;
        public double kas_awal { get; set; } = 0;
        public double tunai { get; set; } = 0;
        public int jumlah_transaksi { get; set; } = 0;

    }


   
    private async void get_summary()
    {
        try
        {
            string url = App.API_HOST + "kasir/summary.php?id_user=" + ID_USER;

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    List<list_summary> rowData = JsonConvert.DeserializeObject<List<list_summary>>(json);

                    if (rowData != null && rowData.Count > 0)
                    {
                        list_summary row = rowData[0];
                        System.Diagnostics.Debug.WriteLine(row.kas_awal.ToString());
                        L_JumlahTransaksi.Text = row.jumlah_transaksi.ToString(); // Jumlah transaksi doesn't need "Rp"
                        L_Qris.Text = FormatCurrency(row.qris);
                        L_TunaiAwal.Text = FormatCurrency(row.kas_awal);
                        L_Transfer.Text = FormatCurrency(row.transfer);
                        L_Tunai.Text = FormatCurrency(row.tunai);
                        L_Total.Text = FormatCurrency(row.total_transaksi);

                        // Format all monetary values as Rp with thousand separators, e.g., Rp 1.000.000
                    }
                    else
                    {
                        // Handle jumlah kosong dan grandtotal kosong
                        System.Diagnostics.Debug.WriteLine("Api Data Error");

                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Api Data 0");
                }
            }
        }

        catch (Exception ex)
        {

        }
    }

    private string FormatCurrency(double amount)
    {
        // Format the amount as Indonesian Rupiah with thousand separators
        return "Rp " + amount.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
    }



}