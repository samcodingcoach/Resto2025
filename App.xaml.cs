namespace Resto2025
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //MainPage = new AppShell();

            MainPage =  new NavigationPage(new Transaksi.ProdukMenu());
        }
    }
}
