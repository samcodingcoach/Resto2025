using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using static Resto2025.Login;

namespace Resto2025;

public partial class InfoDetailPopup : Popup
{
    public list_info InfoItem { get; set; }
    public ObservableCollection<string> Links { get; set; }

    public InfoDetailPopup(list_info info)
    {
        InitializeComponent();
        
        InfoItem = info;
        Links = new ObservableCollection<string>();
        
        // Parse links if any
        if (!string.IsNullOrEmpty(info.link))
        {
            // Split links by comma or semicolon if multiple links exist
            var linkList = info.link.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var link in linkList)
            {
                Links.Add(link.Trim());
            }
        }
        
        BindingContext = this;
    }

    private void CloseButton_Clicked(object sender, EventArgs e)
    {
        Close();
    }
}
