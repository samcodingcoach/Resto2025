CekPesanan_Modal.xaml.cs
pada bagian ini
private async void CloseModal_Tapped(object sender, TappedEventArgs e)
    {


		if (sender is Image image)
		{
			await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
			await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
		}

		// tutup modal
	}

	pada <Label Text="Total Item: " x:Name="L_TotalItem" FontSize="Caption" HorizontalOptions="End"></Label> beri informasi berapa item yang dipesan
	berdasarkan sum dari binding Qty