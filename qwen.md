ProdukMenu.xaml.cs line 1837
try
        {
            using (HttpClient client = new HttpClient())
            {
               
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Sukses",, "OK"); <- pada bagian ini saya ingin ambil pesan sukses dari server

                  berikut formatnya
                  echo json_encode(['status' => 'success', 'message' => 'Pesanan berhasil diperbarui dan dibayar.']);