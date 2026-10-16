using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageApplications : Page, IRefreshablePage
    {
        public PageApplications()
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

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            Load();
        }

        private void Load()
        {
            if (AppsList == null) return;
            if (SearchBox == null) return;

            var text = SearchBox?.Text?.Trim() ?? "";
            var filter = (StatusFilter?.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "Все";

            var list = new List<object>();

            if (Session.IsStudent && Session.StudentId != 0)
            {
                TitleText.Text = "Мои отклики";
                StatusFilterBorder.Visibility = Visibility.Collapsed;

                var apps = Connection.connect.CompanyApplications
                    .Where(a => a.StudentId == Session.StudentId)
                    .ToList();

                foreach (var a in apps)
                {
                    var v = Connection.connect.Vacancies
                        .FirstOrDefault(x => x.Id == a.VacancyId);

                    string title = v != null ? v.Title : "—";
                    string company = (v != null && v.Companies != null)
                        ? v.Companies.Name : "—";

                    if (!string.IsNullOrEmpty(text) &&
                        !title.Contains(text) && !company.Contains(text))
                        continue;

                    list.Add(new
                    {
                        a.Id,
                        a.StudentId,
                        Title = title,
                        Subtitle = company + " · " +
                                   (a.CreatedAt != null
                                       ? a.CreatedAt.ToString("dd.MM.yyyy") : ""),
                        Message = a.Message,
                        Status = GetStatusLabel(a.Status),
                        StatusColor = GetStatusColor(a.Status),
                        StatusBg = GetStatusBg(a.Status),
                        RejectionReason = !string.IsNullOrEmpty(a.CompanyComment)
                            ? "Причина: " + a.CompanyComment : "",
                        ReasonVisibility = string.IsNullOrEmpty(a.CompanyComment)
                            ? Visibility.Collapsed : Visibility.Visible,
                        CompanyButtonsVisibility = Visibility.Collapsed,
                        ApproveButtonsVisibility = Visibility.Collapsed,
                        AvatarData = v != null ? v.AvatarData : null
                    });
                }
            }
            else if (Session.IsEmployer && Session.CompanyId != 0)
            {
                TitleText.Text = "Отклики на мои вакансии";
                StatusFilterBorder.Visibility = Visibility.Visible;

                var vacancyIds = Connection.connect.Vacancies
                    .Where(v => v.CompanyId == Session.CompanyId)
                    .Select(v => v.Id).ToList();

                var apps = Connection.connect.CompanyApplications
                    .Where(a => vacancyIds.Contains(a.VacancyId))
                    .ToList();

                foreach (var a in apps)
                {
                    if (filter == "Ожидают" && a.Status != "Отправлен") continue;
                    if (filter == "Одобрены" && a.Status != "Одобрен") continue;
                    if (filter == "Отклонены" && a.Status != "Отказ") continue;

                    var s = Connection.connect.Students
                        .FirstOrDefault(x => x.Id == a.StudentId);
                    var v = Connection.connect.Vacancies
                        .FirstOrDefault(x => x.Id == a.VacancyId);

                    string name = s != null ? s.FullName : "—";
                    string vacancyTitle = v != null ? v.Title : "—";

                    if (!string.IsNullOrEmpty(text) &&
                        !name.Contains(text) && !vacancyTitle.Contains(text))
                        continue;

                    byte[] avatar = null;
                    if (s != null && s.UserId.HasValue)
                    {
                        var u = Connection.connect.Users
                            .FirstOrDefault(x => x.Id == s.UserId.Value);
                        if (u != null) avatar = u.AvatarData;
                    }

                    list.Add(new
                    {
                        a.Id,
                        a.StudentId,
                        Title = name,
                        Subtitle = (s != null
                            ? s.Course + " курс · " + s.Direction : "")
                            + " → " + vacancyTitle,
                        Message = a.Message,
                        Status = GetStatusLabel(a.Status),
                        StatusColor = GetStatusColor(a.Status),
                        StatusBg = GetStatusBg(a.Status),
                        RejectionReason = !string.IsNullOrEmpty(a.CompanyComment)
                            ? "Причина: " + a.CompanyComment : "",
                        ReasonVisibility = string.IsNullOrEmpty(a.CompanyComment)
                            ? Visibility.Collapsed : Visibility.Visible,
                        CompanyButtonsVisibility = Visibility.Visible,
                        ApproveButtonsVisibility = (a.Status == "Отправлен")
                            ? Visibility.Visible
                            : Visibility.Collapsed,
                        AvatarData = avatar
                    });
                }
            }

            AppsList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private string GetStatusLabel(string status)
        {
            switch (status)
            {
                case "Отправлен": return "Отправлен";
                case "Одобрен": return "Одобрен";
                case "Интервью": return "Интервью";
                case "Оффер": return "Оффер";
                case "Отказ": return "Отказ";
                default: return status ?? "—";
            }
        }

        private Brush GetStatusColor(string status)
        {
            switch (status)
            {
                case "Одобрен":
                case "Интервью":
                case "Оффер":
                    return new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#16A34A"));
                case "Отказ":
                    return new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#DC2626"));
                default:
                    return new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#5B5BF5"));
            }
        }

        private Brush GetStatusBg(string status)
        {
            switch (status)
            {
                case "Одобрен":
                case "Интервью":
                case "Оффер":
                    return new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#DCFCE7"));
                case "Отказ":
                    return new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#FEE2E2"));
                default:
                    return new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#EEF2FF"));
            }
        }

        private void Avatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void StudentProfile_Click(object sender, RoutedEventArgs e)
        {
            int studentId = (int)((Button)sender).Tag;
            NavigationService.Navigate(new PageStudentProfile(studentId));
        }

       
        private void WriteMessage_Click(object sender, RoutedEventArgs e)
        {
            int studentId = (int)((Button)sender).Tag;

            if (!Session.IsEmployer || Session.CompanyId == 0)
            {
                AppMessage.Warning(Window.GetWindow(this),
                    "Написать студенту может только компания");
                return;
            }

            
            NavigationService.Navigate(new PageChat(studentId, Session.CompanyId));
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var a = Connection.connect.CompanyApplications
                .FirstOrDefault(x => x.Id == id);
            if (a == null) return;

            var wnd = new MessageDialogWindow("Сообщение студенту (необязательно):");
            wnd.Owner = Window.GetWindow(this);

            string msg = "";
            if (wnd.ShowDialog() == true)
                msg = wnd.Message;

            a.Status = "Одобрен";
            a.CompanyComment = null;
            Connection.connect.SaveChanges();

            var v = Connection.connect.Vacancies
                .FirstOrDefault(x => x.Id == a.VacancyId);
            string title = v != null ? v.Title : "вакансия";

            Notify.ToStudent(a.StudentId,
                "Отклик одобрен",
                "Ваш отклик на «" + title + "» одобрен",
                "applications", a.VacancyId);

            if (!string.IsNullOrEmpty(msg))
            {
                Notify.SendMessage(a.StudentId, Session.CompanyId,
                    a.VacancyId, "Company", msg);
            }

            var main = Window.GetWindow(this) as MainWindow;
            main?.LoadMailBadge();

            AppMessage.Success(Window.GetWindow(this), "Отклик одобрен");
            Load();
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var a = Connection.connect.CompanyApplications
                .FirstOrDefault(x => x.Id == id);
            if (a == null) return;

            var wnd = new RejectReasonWindow("Укажите причину отказа:");
            wnd.Owner = Window.GetWindow(this);

            if (wnd.ShowDialog() == true)
            {
                a.Status = "Отказ";
                a.CompanyComment = wnd.Reason;
                Connection.connect.SaveChanges();

                var v = Connection.connect.Vacancies
                    .FirstOrDefault(x => x.Id == a.VacancyId);
                string title = v != null ? v.Title : "вакансия";

                Notify.ToStudent(a.StudentId,
                    "Отклик отклонён",
                    "Ваш отклик на «" + title + "» отклонён. Причина: " + wnd.Reason,
                    "applications", a.VacancyId);

                Notify.SendMessage(a.StudentId, Session.CompanyId,
                    a.VacancyId, "Company",
                    "Ваш отклик на «" + title + "» отклонён. Причина: " + wnd.Reason);

                var main = Window.GetWindow(this) as MainWindow;
                main?.LoadMailBadge();

                AppMessage.Info(Window.GetWindow(this), "Отклик отклонён");
                Load();
            }
        }
    }
}