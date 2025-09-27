namespace Resto2025
{
    public partial class App : Application
    {
        public static string API_HOST { get; set; }
        public static string IMAGE_HOST { get; set; }
        public App()
        {
            InitializeComponent();

            //MainPage = new AppShell();
            string lokal = "http://192.168.77.8/";
            string publik = "https://resto.samdev.org/";
            API_HOST = publik + "_resto007/api/";
            IMAGE_HOST = publik + "_resto007/public/images/";



          //MainPage =  new NavigationPage(new Transaksi.ProdukMenu());
          MainPage = new NavigationPage(new Transaksi.CekPesanan_Modal());
        }
    }
}
