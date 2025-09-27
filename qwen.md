pada CekPesanan_Modal.xaml.cs_
		
		[JsonProperty("status")]
		public string Status { get; set; }
	Jika status == 1 itu artinya sudah dibayar diawal
	maka tombol bayar 
	<Button Text="BAYAR" TextColor="White" FontAttributes="Bold" x:Name="Submit_Bayar" Clicked="Submit_Bayar_Clicked"
                        FontSize="Title" BackgroundColor="#FF2D2D" CornerRadius="8" Padding="30,15" />
diganti dengan text RILIS MEJA back ground 075E54 text white