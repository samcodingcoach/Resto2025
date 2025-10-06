using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
namespace Resto2025.Akun;

public partial class Akun : ContentPage
{
    private List<list_promo> _listpromo;
    public Akun()
	{
		InitializeComponent();
        _listpromo = new List<list_promo>(); // taruh di public load 
        get_list();
    }



    
  

        

    public class list_promo
    {
        public string id_promo { get; set; } = string.Empty;
        public string nama_promo { get;set; } = string.Empty;
        public string kode_promo { get; set; }  = string.Empty ;
        public string pilihan_promo { get; set; } = string.Empty;
        public string tanggalmulai_promo { get; set; } = string.Empty;
        public string tanggalselesai_promo { get; set; } = string.Empty;
        public double nilai { get; set; } = 0;
        public int kuota { get; set; } = 0;
        public double min_pembelian { get; set; } = 0;
        public string nilai_string { get; set; } = string.Empty;


    }



    private async void get_list()
    {


        string url = App.API_HOST + "promo/list_promo.php";
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            List<list_promo> rowData = JsonConvert.DeserializeObject<List<list_promo>>(json);

            _listpromo.Clear(); 

           
            for (int i = 0; i < rowData.Count; i++)
            {
                if (rowData[i].pilihan_promo == "persen")
                {
                    rowData[i].nilai_string = $"{rowData[i].nilai}%";
                }
                else if(rowData[i].pilihan_promo == "nominal")
                {
                    rowData[i].nilai_string = FormatCurrency(rowData[i].nilai);
                }
                    _listpromo.Add(rowData[i]);
            }

            //total = rowData.Count;
            lv_promo.ItemsSource = _listpromo; 
            
        }
        else
        {
            
        }

    }

    private string FormatCurrency(double amount)
    {
        // Format the amount as Indonesian Rupiah with thousand separators
        return "Rp " + amount.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
    }



    private void B_Update_Clicked(object sender, EventArgs e)
    {

    }

    private void B_UbahPassword_Clicked(object sender, EventArgs e)
    {

    }

    private void B_Logout_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void TapPromo_Tapped(object sender, EventArgs e)
    {

    }
}