using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProfitStart
{
    public partial class PagePractice : Page, IRefreshablePage
    {
        public PagePractice()
        {
            InitializeComponent();
            SearchPlaceholder.Visibility = Visibility.Visible;
            Load();
        }

        public void RefreshAfterThemeChange()
        {
            Load();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchPlaceholder != null)
                SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                    ? Visibility.Visible : Visibility.Collapsed;

            Load();
        }

        private void Load()
        {
            var text = SearchBox?.Text?.Trim() ?? "";

            var deletedCompanyIds = Connection.connect.Companies
                .Where(c => c.IsDeleted == true)
                .Select(c => c.Id)
                .ToList();

            var query = Connection.connect.Practices
                .Where(p => p.IsApproved == true
                         && (p.CompanyId == null
                             || !deletedCompanyIds.Contains(p.CompanyId.Value)))
                .AsQueryable();

            if (!string.IsNullOrEmpty(text))
            {
                query = query.Where(p =>
                    p.Title.Contains(text) ||
                    p.Direction.Contains(text));
            }

            var list = query.ToList().Select(p => new
            {
                p.Id,
                p.Title,
                Subtitle = (p.Direction ?? "") + " · " + (p.Duration ?? ""),
                AvatarData = p.AvatarData
            }).ToList();

            PracticeList.ItemsSource = list;
        }

        private void PracticeAvatar_Loaded(object sender, RoutedEventArgs e)
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

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            int id = (int)btn.Tag;
            NavigationService.Navigate(new PagePracticeCard(id));
        }
    }
}