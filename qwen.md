TransferBank_Modal.xaml & TransferBank_Modal.xaml.cs, pada bagian

baca dengan seksama dan pahami permintaan saya
pada halaman ini saya ingin membuat sebuah modal untuk melakukan penyimpanan ke sebuah table dengan api berikut

pembayaran/simpan_bank.php dengan post, berikut outputnya
{
    "status": "success",
    "message": "Data pembayaran berhasil disimpan.",
    "id": 32,
    "img_file_name": "",
    "img_url": ""
}
untuk parameternya saya ambil dari postman saja silahkan andaterjemahkan 
var client = new HttpClient();
var request = new HttpRequestMessage(HttpMethod.Post, "https://resto.samdev.org/_resto007/api/pembayaran/simpan_bank.php");
var content = new MultipartFormDataContent();
content.Add(new StringContent("T0015"), "kode_payment");
content.Add(new StringContent("0"), "transfer_or_edc");
content.Add(new StringContent("BCA"), "nama_bank");
content.Add(new StringContent("Budi"), "nama_pengirim");
content.Add(new StringContent("00192"), "no_referensi");
content.Add(new StreamContent(File.OpenRead("/C:/Users/USER/Desktop/tes.jpg")), "img_ss", "/C:/Users/USER/Desktop/tes.jpg");
request.Content = content;
var response = await client.SendAsync(request);
response.EnsureSuccessStatusCode();
Console.WriteLine(await response.Content.ReadAsStringAsync());

upload gambarnya optional, jika tidak ada gambar maka tidak perlu diikutkan pada form data content




