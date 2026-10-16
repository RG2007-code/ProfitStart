using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProfitStart
{
    public partial class PageMyPractices : Page, IRefreshablePage
    {
        public PageMyPractices()
        {
            InitializeComponent();
            if (SearchPlaceholder != null)
                SearchPlaceholder.Visibility = Visibility.Visible;
            Loaded += (s, e) => Load();
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
            if (PracticesList == null) return;

            if (!Session.IsStudent || Session.StudentId == 0)
            {
                PracticesList.ItemsSource = null;
                EmptyText.Visibility = Visibility.Visible;
                return;
            }

            var text = SearchBox?.Text?.Trim() ?? "";

            var deletedCompanyIds = Connection.connect.Companies
                .Where(c => c.IsDeleted == true)
                .Select(c => c.Id)
                .ToList();

            var apps = Connection.connect.PracticeApplications
                .Where(a => a.StudentId == Session.StudentId
                         && a.Status == "Одобрен")
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            var list = new List<object>();

            foreach (var a in apps)
            {
                var p = Connection.connect.Practices
                    .FirstOrDefault(x => x.Id == a.PracticeId);
                if (p == null) continue;

                if (p.CompanyId.HasValue
                    && deletedCompanyIds.Contains(p.CompanyId.Value))
                    continue;

                string title = p.Title ?? "—";
                string dir = p.Direction ?? "—";

                if (!string.IsNullOrEmpty(text) &&
                    !title.Contains(text) &&
                    !dir.Contains(text))
                    continue;

                string companyName = "—";
                if (p.CompanyId.HasValue)
                {
                    var c = Connection.connect.Companies
                        .FirstOrDefault(x => x.Id == p.CompanyId.Value);
                    if (c != null) companyName = c.Name;
                }

                byte[] avatar = p.AvatarData;
                if ((avatar == null || avatar.Length == 0)
                    && p.CompanyId.HasValue)
                {
                    var c = Connection.connect.Companies
                        .FirstOrDefault(x => x.Id == p.CompanyId.Value);
                    if (c != null) avatar = c.AvatarData;
                }

                list.Add(new
                {
                    Id = p.Id,
                    Title = title,
                    Subtitle = dir + " · " + (p.Duration ?? "—"),
                    CompanyName = "Компания: " + companyName,
                    AvatarData = avatar
                });
            }

            PracticesList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void PracticeAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void Card_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;
            int id = (int)border.Tag;
            NavigationService.Navigate(new PagePracticeCard(id));
        }
    }
}