using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProfitStart
{
    public partial class PageVacancies : Page, IRefreshablePage
    {
        public PageVacancies()
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
            if (VacancyList == null || SearchBox == null) return;

            var text = SearchBox.Text?.Trim() ?? "";
            var type = (TypeFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все";
            var salarySort = (SalaryFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString()
                             ?? "Без сортировки";

            var query = Connection.connect.Vacancies
                .Where(v => v.IsApproved == true
                         && (v.Companies == null || v.Companies.IsDeleted != true))
                .AsQueryable();

            if (!string.IsNullOrEmpty(text))
            {
                query = query.Where(v =>
                    v.Title.Contains(text) ||
                    v.Description.Contains(text) ||
                    v.Requirements.Contains(text) ||
                    v.Companies.Name.Contains(text));
            }

            if (type != "Все")
                query = query.Where(v => v.Type == type);

            var vacancies = query.ToList();

            if (salarySort == "Сначала дорогие")
                vacancies = vacancies.OrderByDescending(v => Payment.ParseSalary(v.Salary)).ToList();
            else if (salarySort == "Сначала дешёвые")
                vacancies = vacancies.OrderBy(v => Payment.ParseSalary(v.Salary)).ToList();
            else
                vacancies = vacancies.OrderByDescending(v => v.CreatedAt).ToList();

            var list = vacancies.Select(v =>
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

                int match = 0;
                if (Session.IsStudent && Session.StudentId != 0)
                    match = Skill.CalculateMatch(Session.StudentId, v.Id);

                string salaryFormatted = Payment.Format(v.Salary, v.PaymentUnit);

                return new
                {
                    v.Id,
                    v.Title,
                    Subtitle = (v.Companies != null ? v.Companies.Name : "—")
                               + " · " + v.Type,
                    SalaryText = string.IsNullOrEmpty(salaryFormatted)
                        ? "" : "💰 " + salaryFormatted,
                    Match = match > 0 ? "Подходит вам · " + match + "%" : "",
                    MatchVisibility = (Session.IsStudent && match > 0)
                        ? Visibility.Visible
                        : Visibility.Collapsed,
                    AvatarData = avatar
                };
            }).ToList();

            VacancyList.ItemsSource = list;
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