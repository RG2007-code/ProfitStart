using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageCompanies : Page, IRefreshablePage
    {
        public PageCompanies()
        {
            InitializeComponent();
            SearchPlaceholder.Visibility = Visibility.Visible;

            Loaded += (s, e) => Load();

            IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue) Load();
            };
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

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            Load();
        }

        private void Load()
        {
            if (CompanyList == null || SearchBox == null) return;

            var text = SearchBox.Text?.Trim() ?? "";
            var ratingFilter = (RatingFilter?.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "Все";

            var query = Connection.connect.Companies
                .AsNoTracking()
                .Where(c => c.IsPartner == true
                         && c.IsApproved == true
                         && c.IsDeleted != true);

            if (!string.IsNullOrEmpty(text))
            {
                query = query.Where(c =>
                    c.Name.Contains(text) ||
                    c.Direction.Contains(text) ||
                    c.Description.Contains(text));
            }

            var companies = query.ToList();

            var ownerIds = companies
                .Where(c => c.UserId.HasValue)
                .Select(c => c.UserId.Value)
                .Distinct()
                .ToList();

            var owners = Connection.connect.Users
                .AsNoTracking()
                .Where(u => ownerIds.Contains(u.Id))
                .Select(u => new { u.Id, u.AvatarData })
                .ToDictionary(u => u.Id, u => u.AvatarData);

            var list = companies.Select(c =>
            {
                byte[] avatar = c.AvatarData;
                if ((avatar == null || avatar.Length == 0) && c.UserId.HasValue)
                {
                    if (owners.TryGetValue(c.UserId.Value, out var ownerAvatar))
                        avatar = ownerAvatar;
                }

                double rating = CompanyRating.GetAverage(c.Id);
                int ratingCount = CompanyRating.GetCount(c.Id);

                int fullStars = (int)Math.Round(rating);
                if (fullStars > 5) fullStars = 5;
                if (fullStars < 0) fullStars = 0;

                string stars = new string('★', fullStars)
                             + new string('☆', 5 - fullStars);

                string ratingText;
                if (ratingCount == 0)
                    ratingText = "нет оценок";
                else
                    ratingText = rating.ToString("0.0", CultureInfo.CurrentCulture)
                               + " · " + ratingCount + " " + Plural(ratingCount);

                return new
                {
                    c.Id,
                    c.Name,
                    Direction = c.Direction,
                    Stars = stars,
                    RatingText = ratingText,
                    AvatarData = avatar,
                    Rating = rating
                };
            })
            .Where(x =>
            {
                if (ratingFilter == "5 звёзд") return x.Rating >= 5;
                if (ratingFilter == "4+ звезды") return x.Rating >= 4;
                if (ratingFilter == "3+ звезды") return x.Rating >= 3;
                return true;
            })
            .OrderByDescending(x => x.Rating)
            .ToList();

            CompanyList.ItemsSource = list;
        }

        private static string Plural(int n)
        {
            int mod10 = n % 10;
            int mod100 = n % 100;
            if (mod10 == 1 && mod100 != 11) return "оценка";
            if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20))
                return "оценки";
            return "оценок";
        }

        private void CompanyAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void Company_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;
            int id = (int)border.Tag;
            NavigationService.Navigate(new PageCompanyCard(id));
        }
    }
}