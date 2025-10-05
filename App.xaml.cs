using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
namespace Resto2025
{
    public partial class App : Application
    {
        public static string API_HOST { get; set; }
        public static string IMAGE_HOST { get; set; }
        public static string NAMA_APLIKASI { get; set; }
        public static string API_LOGIN { get; set; }
        public App()
        {
            InitializeComponent();
            //combobox windows 
            



            //MainPage = new AppShell();
            string lokal = "http://192.168.77.8/_resto007/";
            string publik = "https://resto.samdev.org/_resto007/";
            API_HOST = lokal + "api/";
            IMAGE_HOST = lokal + "public/images/";
            API_LOGIN = lokal + "config/login.php";
            // Start loading the app data in background since it's async
            _ = Task.Run(async () => await LoadAppDataAsync());



        MainPage =  new MainPage();
            //MainPage = new NavigationPage(new Beranda.DetailOrder());
        }
        
        private async Task LoadAppDataAsync()
        {
            await get_perusahaan();
        }

        public class list_perusahaan
        {
            public string nama_aplikasi { get; set; } = string.Empty;
            public string alamat { get; set; } = string.Empty;
            public string no_hp { get;set; } = string.Empty;
            public string id_app { get; set; } = string.Empty;
        }

        private async Task get_perusahaan()
        {
            try
            {


                string url = API_HOST + "perusahaan/list.php";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        List<list_perusahaan> rowData = JsonConvert.DeserializeObject<List<list_perusahaan>>(json);

                        if (rowData != null && rowData.Count > 0)
                        {
                            list_perusahaan row = rowData[0];
                            NAMA_APLIKASI = row.nama_aplikasi;
                            System.Diagnostics.Debug.WriteLine($"{NAMA_APLIKASI}-{row.nama_aplikasi}");

                        }
                        else
                        {
                          
                        }
                    }
                    else
                    {

                    }
                }
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

    }
}
