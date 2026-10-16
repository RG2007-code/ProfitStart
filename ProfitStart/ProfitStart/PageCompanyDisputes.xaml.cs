using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProfitStart
{
    public partial class PageCompanyDisputes : Page
    {
        public PageCompanyDisputes()
        {
            InitializeComponent();
            Load();

            IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue)
                    Load();
            };
        }

        private void Load()
        {
            var list = new List<object>();

            var companies = Connection.connect.Companies
                .Where(c => c.DisputeRequested == true)
                .OrderByDescending(c => c.Id)
                .ToList();

            foreach (var c in companies)
            {
                byte[] avatar = null;
                if (c.UserId.HasValue)
                {
                    var owner = Connection.connect.Users
                        .AsNoTracking()
                        .FirstOrDefault(u => u.Id == c.UserId.Value);
                    if (owner != null) avatar = owner.AvatarData;
                }

                list.Add(new
                {
                    c.Id,
                    c.Name,
                    Info = (c.Direction ?? "—") + " · ИНН: " + (c.INN ?? "—"),
                    Message = "Оспаривание: " + (c.DisputeMessage ?? "—"),
                    AvatarData = avatar
                });
            }

            DisputesList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CompanyAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;

            var data = ellipse.Tag as byte[];
            if (data == null || data.Length == 0) return;

            try
            {
                using (var ms = new MemoryStream(data))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();

                    ellipse.Fill = new ImageBrush(bmp)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
            }
            catch { }
        }

        private void Card_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            int id = (int)border.Tag;
            NavigationService.Navigate(new PageCompanyDispute(id));
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            NavigationService.Navigate(new PageCompanyDispute(id));
        }
    }
}