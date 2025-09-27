CekPesanan_Modal.xaml.cs, pada bagian_
public class PesananInfo
public string TglCart { get; set; } kenapa menjadi string?
contoh output dari json adalah "tgl_cart": "2025-09-27 09:35:18"
dari ouput itu saya ingin di transform kedalam bentuk informasi durasi yang sudah di lewati
misalnya 1:30 Menit 
artinya yang lalu (1 jam 30 menit yang lalu)
taruh dalam ui <Label Text="" x:Name="L_DURASI"  TextColor="#B3b3b3" FontFamily="FontBold" FontSize="Caption" />
