private async void aktifkan_meja()
	{
		
		var data = new Dictionary<string, string>
				{
					{ "id_meja", ID_MEJA.ToString() }
				};

		var jsonData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
		var client = new HttpClient();
		string ip = App.API_HOST + "meja/aktifkan.php";

		var response = await client.PostAsync(ip, jsonData);
		var responseContent = await response.Content.ReadAsStringAsync();
		var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

		if (responseObject["status"] == "success")
		{

			
			Close();
		
		}
	}

	harusnya pada success anda cari tau kodenya ketika ditutup langsung merefresh meja. karna sepertinya di ProdukMenu.xaml.cs ada methodnya
	coba anda cari methodnya di ProdukMenu.xaml.cs dan contoh di Tunai_Modal.xaml.cs_