pada login.xaml.cs

private async void B_Login_Clicked(object sender, EventArgs e)
    {


        if (sender is Button image)
        {
            await image.FadeTo(0.3, 100); // Turunkan opacity ke 0.3 dalam 100ms
            await image.FadeTo(1, 200);   // Kembalikan opacity ke 1 dalam 200ms
        }



    }

    buatkan validasi email dan password tidak boleh null/kosong,
    email harus format email, password minimal 4 digit 