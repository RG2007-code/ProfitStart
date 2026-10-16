using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageMyVacancies : Page
    {
        public PageMyVacancies()
        {
            InitializeComponent();
            SearchPlaceholder.Visibility = Visibility.Visible;
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
            if (Session.CompanyId == 0)
            {
                AppMessage.Warning(Window.GetWindow(this),
                    "Вы не привязаны к компании");
                return;
            }

            var text = SearchBox?.Text?.Trim() ?? "";

            var query = Connection.connect.Vacancies
                .Where(v => v.CompanyId == Session.CompanyId);

            if (!string.IsNullOrEmpty(text))
            {
                query = query.Where(v =>
                    v.Title.Contains(text) ||
                    v.Description.Contains(text) ||
                    v.Type.Contains(text));
            }

            var vacancies = query.OrderByDescending(v => v.CreatedAt).ToList();

            var list = new List<object>();
            foreach (var v in vacancies)
            {
                string status;
                Brush color;

                if (v.IsApproved == true)
                { status = "✓ Одобрено"; color = GetBrush("#16A34A"); }
                else if (v.IsModerated == true)
                { status = "⏳ На модерации"; color = GetBrush("#D97706"); }
                else if (!string.IsNullOrEmpty(v.RejectionReason))
                { status = "✕ Отклонено"; color = GetBrush("#DC2626"); }
                else
                { status = "● Черновик"; color = GetBrush("#6B7280"); }

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

                list.Add(new
                {
                    v.Id,
                    v.Title,
                    AvatarData = avatar,
                    Subtitle = (v.Type ?? "")
                             + " · " + Payment.Format(v.Salary, v.PaymentUnit)
                             + " · " + (v.Description ?? ""),
                    StatusText = status,
                    StatusColor = color,
                    Reason = !string.IsNullOrEmpty(v.RejectionReason)
                             && v.IsApproved == false && v.IsModerated == false
                        ? "Причина: " + v.RejectionReason : ""
                });
            }

            VacanciesList.ItemsSource = list;
        }

        private Brush GetBrush(string hex)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }

        private void VacancyAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new PageAddVacancy());

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            NavigationService.Navigate(new PageEditVacancy(id));
        }

        private void Applications_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            NavigationService.Navigate(new PageVacancyApplications(id));
        }

        
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;

            if (!AppMessage.Confirm(Window.GetWindow(this),
                "Удалить вакансию?\nВсе отклики и уведомления, связанные с ней, "
                + "будут удалены.", "Удаление"))
                return;

            var v = Connection.connect.Vacancies
                .FirstOrDefault(x => x.Id == id);
            if (v == null) return;

            
            var apps = Connection.connect.CompanyApplications
                .Where(a => a.VacancyId == id).ToList();
            foreach (var a in apps)
                Connection.connect.CompanyApplications.Remove(a);

            
            var notifs = Connection.connect.Notifications
                .Where(n => n.LinkId == id &&
                       (n.LinkPage == "modvacancies"
                     || n.LinkPage == "myvacancies"
                     || n.LinkPage == "vacancies"
                     || n.LinkPage == "applications"))
                .ToList();
            foreach (var n in notifs)
                Connection.connect.Notifications.Remove(n);

            
            Connection.connect.SaveChanges();

            
            Connection.connect.Vacancies.Remove(v);
            Connection.connect.SaveChanges();

            AppMessage.Success(Window.GetWindow(this), "Вакансия удалена");
            Load();
        }
    }
}