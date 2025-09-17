namespace Resto2025
{
    public partial class App : Application
    {
        public static string API_HOST { get; set; }
        public App()
        {
            InitializeComponent();

            //MainPage = new AppShell();
            API_HOST = "http://192.168.77.8/_resto007/api/";

            MainPage =  new NavigationPage(new Transaksi.ProdukMenu());
        }
    }
}
