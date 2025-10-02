pada login.xaml.cs

public Login()
	{
		InitializeComponent();
        L_NamaApp.Text = App.NAMA_APLIKASI;
        System.Diagnostics.Debug.WriteLine($"Nama APP: {L_NamaApp.Text}");
        
	}

	kenama pada login() L_NamaApp.Text kosong padahal di App.xaml.cs ada nilainya