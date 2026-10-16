using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageMain : Page, IRefreshablePage
    {
        private string _selectedDirection = "";

        public PageMain()
        {
            InitializeComponent();
            SearchPlaceholder.Visibility = Visibility.Visible;

            bool auth = Session.IsAuthenticated;
            DirectionsBlock.Visibility = auth ? Visibility.Visible : Visibility.Collapsed;
            ForYouBlock.Visibility = auth ? Visibility.Visible : Visibility.Collapsed;

            if (auth)
            {
                LoadDirections();
                LoadForYou();
            }

            LoadRecent();
        }

        public void RefreshAfterThemeChange()
        {
            if (Session.IsAuthenticated)
            {
                LoadDirections();
                LoadForYou();
            }
            LoadRecent();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchPlaceholder != null)
                SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                    ? Visibility.Visible : Visibility.Collapsed;

            LoadForYou();
            LoadRecent();
        }

        private void LoadDirections()
        {
            DirectionsPanel.Children.Clear();

            var directions = Connection.connect.Companies
                .Where(c => c.Direction != null && c.Direction != ""
                         && c.IsDeleted != true)
                .Select(c => c.Direction)
                .Distinct()
                .Take(6)
                .ToList();

            AddDirectionChip("Все", isReset: true);

            foreach (var name in directions)
                AddDirectionChip(name, isReset: false);
        }

        private void AddDirectionChip(string name, bool isReset)
        {
            bool selected = isReset
                ? string.IsNullOrEmpty(_selectedDirection)
                : _selectedDirection == name;

            var border = new Border
            {
                Background = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(
                        selected ? "#5B5BF5" : "#EEF2FF")),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(18, 10, 18, 10),
                Margin = new Thickness(0, 0, 10, 10),
                Cursor = Cursors.Hand,
                Tag = isReset ? "" : name
            };
            border.Child = new TextBlock
            {
                Text = name,
                Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(
                        selected ? "#FFFFFF" : "#5B5BF5")),
                FontSize = 13
            };
            border.MouseLeftButtonUp += Direction_Click;
            DirectionsPanel.Children.Add(border);
        }

        private void Direction_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            _selectedDirection = border.Tag as string ?? "";

            LoadDirections();
            LoadForYou();
            LoadRecent();
        }

        private void LoadForYou()
        {
            if (ForYouList == null) return;

            var search = SearchBox?.Text?.Trim() ?? "";

            var query = Connection.connect.Vacancies
                .Where(v => v.IsApproved == true
                         && (v.Companies == null || v.Companies.IsDeleted != true));

            if (!string.IsNullOrEmpty(_selectedDirection))
            {
                query = query.Where(v =>
                    v.Companies != null &&
                    v.Companies.Direction == _selectedDirection);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(v =>
                    v.Title.Contains(search) ||
                    v.Description.Contains(search) ||
                    v.Requirements.Contains(search) ||
                    (v.Companies != null && v.Companies.Name.Contains(search)));
            }

            var vacancies = query
                .OrderByDescending(v => v.CreatedAt)
                .Take(50)
                .ToList();

            var list = vacancies
                .Select(v =>
                {
                    int match = 0;
                    if (Session.IsStudent && Session.StudentId != 0)
                        match = Skill.CalculateMatch(Session.StudentId, v.Id);

                    return new
                    {
                        v.Id,
                        v.Title,
                        Subtitle = (v.Companies != null ? v.Companies.Name : "—")
                                   + " · " + v.Type,
                        Match = "Подходит вам · " + match + "%",
                        MatchVisibility = Session.IsStudent
                            ? Visibility.Visible
                            : Visibility.Collapsed,
                        MatchPercentValue = match,
                        AvatarData = GetAvatar(v)
                    };
                })
                .OrderByDescending(x => x.MatchPercentValue)
                .Take(3)
                .ToList();

            ForYouList.ItemsSource = list;
        }

        private void LoadRecent()
        {
            if (RecentList == null) return;

            var search = SearchBox?.Text?.Trim() ?? "";

            var query = Connection.connect.Vacancies
                .Where(v => v.IsApproved == true
                         && (v.Companies == null || v.Companies.IsDeleted != true));

            if (!string.IsNullOrEmpty(_selectedDirection))
            {
                query = query.Where(v =>
                    v.Companies != null &&
                    v.Companies.Direction == _selectedDirection);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(v =>
                    v.Title.Contains(search) ||
                    v.Description.Contains(search) ||
                    v.Requirements.Contains(search) ||
                    (v.Companies != null && v.Companies.Name.Contains(search)));
            }

            var vacancies = query
                .OrderByDescending(v => v.CreatedAt)
                .Take(5).ToList();

            var list = vacancies.Select(v => new
            {
                v.Id,
                v.Title,
                Subtitle = (v.Companies != null ? v.Companies.Name : "—")
                           + " · " + v.Type,
                DateText = v.CreatedAt != null
                    ? v.CreatedAt.Value.ToString("dd.MM.yyyy") : "",
                AvatarData = GetAvatar(v)
            }).ToList();

            RecentList.ItemsSource = list;

            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private byte[] GetAvatar(Vacancies v)
        {
            byte[] avatar = v.AvatarData;

            if ((avatar == null || avatar.Length == 0) && v.CompanyId.HasValue)
            {
                var company = Connection.connect.Companies
                    .FirstOrDefault(c => c.Id == v.CompanyId.Value);

                if (company != null)
                {
                    avatar = company.AvatarData;

                    if ((avatar == null || avatar.Length == 0)
                        && company.UserId.HasValue)
                    {
                        var owner = Connection.connect.Users
                            .FirstOrDefault(u => u.Id == company.UserId.Value);
                        if (owner != null) avatar = owner.AvatarData;
                    }
                }
            }

            return avatar;
        }

        private void VacancyAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void Vacancy_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;
            int id = (int)border.Tag;
            NavigationService.Navigate(new PageVacancyCard(id));
        }
    }
}