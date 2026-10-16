using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageNotifications : Page
    {
        public PageNotifications()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            var query = Connection.connect.Notifications.AsQueryable();

            if (Session.IsStudent && Session.StudentId != 0)
                query = query.Where(n => n.StudentId == Session.StudentId);
            else if (Session.IsEmployer && Session.CompanyId != 0)
                query = query.Where(n => n.CompanyId == Session.CompanyId);
            else if (Session.IsAdmin || Session.IsCareer)
                query = query.Where(n => n.RecipientType == "Admin");
            else
            {
                NotifList.ItemsSource = null;
                EmptyText.Visibility = Visibility.Visible;
                return;
            }

            var notifs = query.OrderByDescending(n => n.CreatedAt).ToList();

            var list = new List<object>();
            foreach (var n in notifs)
            {
                var avatar = GetAvatar(n);
                bool isReview = IsReviewNotification(n);
                bool showWarning = isReview && avatar == null;

                list.Add(new
                {
                    n.Id,
                    Title = n.Title ?? "Уведомление",
                    Subtitle = n.Message ?? "",
                    n.LinkPage,
                    n.LinkId,
                    AvatarData = avatar,
                    ShowAvatar = avatar != null
                        ? Visibility.Visible : Visibility.Collapsed,
                    ShowWarning = showWarning
                        ? Visibility.Visible : Visibility.Collapsed,
                    UnreadMark = n.IsRead == true
                        ? Visibility.Collapsed : Visibility.Visible
                });
            }

            NotifList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private bool IsReviewNotification(Notifications n)
        {
            string page = n.LinkPage ?? "";
            string title = n.Title ?? "";

            if (page == "reviews"
                || page == "companyreviews"
                || page == "modreviews"
                || page == "myreviews")
                return true;

            if (title.Contains("Отзыв")
                || title.Contains("отзыв"))
                return true;

            return false;
        }

        private byte[] GetAvatar(Notifications n)
        {
            string pageCheck = n.LinkPage ?? "";
            string titleCheck = n.Title ?? "";

            if (pageCheck == "reviews"
                || pageCheck == "companyreviews"
                || pageCheck == "modreviews"
                || pageCheck == "myreviews"
                || titleCheck.Contains("Отзыв")
                || titleCheck.Contains("отзыв"))
            {
                return null;
            }

            if (n.SenderId.HasValue)
            {
                var a1 = AvatarFromStudent(n.SenderId.Value)
                      ?? AvatarFromCompany(n.SenderId.Value);
                if (a1 != null) return a1;
            }

            string page = n.LinkPage ?? "";

            if (page == "myvacancies" && n.LinkId.HasValue)
            {
                var a = AvatarFromVacancy(n.LinkId.Value)
                     ?? AvatarFromCompany(n.LinkId.Value);
                if (a != null) return a;
            }

            if ((page == "practice" || page == "practiceapps"
                 || page == "mypractices")
                && n.LinkId.HasValue)
            {
                var a = AvatarFromPractice(n.LinkId.Value);
                if (a != null) return a;
            }

            if (page == "managecompanies" && n.LinkId.HasValue)
            {
                var a = AvatarFromCompany(n.LinkId.Value);
                if (a != null) return a;
            }

            if (page == "profile" && n.LinkId.HasValue)
            {
                var a = AvatarFromCompany(n.LinkId.Value);
                if (a != null) return a;
            }

            if ((page == "companydispute"
                 || page == "companysdispute"
                 || page == "companydisputes")
                && n.LinkId.HasValue)
            {
                var a = AvatarFromCompany(n.LinkId.Value);
                if (a != null) return a;
            }

            if (page == "modcompanies" && n.LinkId.HasValue)
            {
                var a = AvatarFromCompany(n.LinkId.Value);
                if (a != null) return a;
            }

            if (page == "modvacancies" && n.LinkId.HasValue)
            {
                var a = AvatarFromVacancy(n.LinkId.Value);
                if (a != null) return a;
            }

            if (page == "modpractice" && n.LinkId.HasValue)
            {
                var a = AvatarFromPractice(n.LinkId.Value);
                if (a != null) return a;
            }

            if (page == "applications")
            {
                if (n.StudentId.HasValue)
                {
                    var a = AvatarFromStudent(n.StudentId.Value);
                    if (a != null) return a;
                }
                if (n.LinkId.HasValue)
                {
                    var a = AvatarFromStudent(n.LinkId.Value);
                    if (a != null) return a;
                }
            }

            if (n.VacancyId.HasValue)
            {
                var a = AvatarFromVacancy(n.VacancyId.Value);
                if (a != null) return a;
            }

            if (n.CompanyId.HasValue)
            {
                var a = AvatarFromCompany(n.CompanyId.Value);
                if (a != null) return a;
            }

            if (n.LinkId.HasValue)
            {
                var a = AvatarFromCompany(n.LinkId.Value)
                     ?? AvatarFromStudent(n.LinkId.Value)
                     ?? AvatarFromVacancy(n.LinkId.Value)
                     ?? AvatarFromPractice(n.LinkId.Value);
                if (a != null) return a;
            }

            if (n.StudentId.HasValue)
            {
                var a = AvatarFromStudent(n.StudentId.Value);
                if (a != null) return a;
            }

            return null;
        }

        private byte[] AvatarFromUsers(int? userId)
        {
            if (!userId.HasValue) return null;

            var u = Connection.connect.Users
                .FirstOrDefault(x => x.Id == userId.Value);
            if (u == null) return null;

            if (u.AvatarData != null && u.AvatarData.Length > 0)
                return u.AvatarData;

            return null;
        }

        private byte[] AvatarFromCompany(int companyId)
        {
            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == companyId);
            if (c == null) return null;

            if (c.AvatarData != null && c.AvatarData.Length > 0)
                return c.AvatarData;

            return AvatarFromUsers(c.UserId);
        }

        private byte[] AvatarFromStudent(int studentId)
        {
            var s = Connection.connect.Students
                .FirstOrDefault(x => x.Id == studentId);
            if (s == null) return null;

            return AvatarFromUsers(s.UserId);
        }

        private byte[] AvatarFromVacancy(int vacancyId)
        {
            var v = Connection.connect.Vacancies
                .FirstOrDefault(x => x.Id == vacancyId);
            if (v == null) return null;

            if (v.AvatarData != null && v.AvatarData.Length > 0)
                return v.AvatarData;

            if (v.CompanyId.HasValue)
                return AvatarFromCompany(v.CompanyId.Value);

            return null;
        }

        private byte[] AvatarFromPractice(int practiceId)
        {
            var p = Connection.connect.Practices
                .FirstOrDefault(x => x.Id == practiceId);
            if (p == null) return null;

            if (p.AvatarData != null && p.AvatarData.Length > 0)
                return p.AvatarData;

            if (p.CompanyId.HasValue)
                return AvatarFromCompany(p.CompanyId.Value);

            return null;
        }

        private void NotifAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void Notif_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null)
                border = FindParent<Border>(e.OriginalSource as DependencyObject);
            if (border == null) return;
            if (!(border.Tag is int notifId)) return;

            e.Handled = true;

            var n = Connection.connect.Notifications
                .FirstOrDefault(x => x.Id == notifId);
            if (n == null) return;

            n.IsRead = true;
            Connection.connect.SaveChanges();

            var main = Window.GetWindow(this) as MainWindow;
            main?.LoadBellBadge();

            string page = n.LinkPage ?? "";
            int linkId = n.LinkId ?? 0;

            if (string.IsNullOrEmpty(page)) { Load(); return; }

            try
            {
                switch (page)
                {
                    case "companydispute":
                    case "companysdispute":
                    case "companydisputes":
                        if (Session.IsAdmin)
                            NavigationService.Navigate(new PageCompanyDisputes());
                        else
                            NavigationService.Navigate(new PageProfile());
                        break;

                    case "profile":
                    case "managecompanies":
                        if (Session.IsAdmin)
                            NavigationService.Navigate(new PageManageCompanies());
                        else
                            NavigationService.Navigate(new PageProfile());
                        break;

                    case "vacancy":
                    case "vacancies":
                        NavigationService.Navigate(new PageVacancyCard(linkId));
                        break;

                    case "myvacancies":
                        NavigationService.Navigate(new PageMyVacancies());
                        break;

                    case "modvacancies":
                        if (Session.IsAdmin)
                            NavigationService.Navigate(new PageModerationVacancies());
                        else
                            NavigationService.Navigate(new PageMyVacancies());
                        break;

                    case "modcompanies":
                        if (Session.IsAdmin)
                            NavigationService.Navigate(new PageModerationCompanies());
                        else
                            NavigationService.Navigate(new PageProfile());
                        break;

                    case "modpractice":
                        if (Session.IsAdmin)
                            NavigationService.Navigate(new PageModerationPractice());
                        else
                            NavigationService.Navigate(new PageManagePractice());
                        break;

                    case "practiceapps":
                        if (Session.IsEmployer || Session.IsAdmin)
                            NavigationService.Navigate(new PagePracticeApplications(linkId));
                        else
                            NavigationService.Navigate(new PagePractice());
                        break;

                    case "mypractices":
                        if (Session.IsEmployer || Session.IsAdmin)
                            NavigationService.Navigate(new PagePracticeApplications(linkId));
                        else
                            NavigationService.Navigate(new PageMyPractices());
                        break;

                    case "applications":
                        NavigationService.Navigate(new PageApplications());
                        break;

                    case "practice":
                        NavigationService.Navigate(new PagePractice());
                        break;

                    case "messages":
                        NavigationService.Navigate(new PageMessages());
                        break;

                    case "modreviews":
                    case "reviews":
                    case "companyreviews":
                    case "myreviews":
                        if (Session.IsAdmin)
                            NavigationService.Navigate(new PageModerationReviews());
                        else
                            NavigationService.Navigate(new PageProfile());
                        break;

                    default:
                        AppMessage.Info(Window.GetWindow(this),
                            $"Неизвестная страница уведомления: [{page}]");
                        Load();
                        break;
                }
            }
            catch (Exception ex)
            {
                AppMessage.Error(Window.GetWindow(this),
                    "Ошибка перехода: " + ex.Message);
            }
        }

        private static T FindParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T t) return t;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (!AppMessage.Confirm(Window.GetWindow(this),
                "Удалить все уведомления?", "Очистка"))
                return;

            var query = Connection.connect.Notifications.AsQueryable();

            if (Session.IsStudent && Session.StudentId != 0)
                query = query.Where(n => n.StudentId == Session.StudentId);
            else if (Session.IsEmployer && Session.CompanyId != 0)
                query = query.Where(n => n.CompanyId == Session.CompanyId);
            else if (Session.IsAdmin || Session.IsCareer)
                query = query.Where(n => n.RecipientType == "Admin");
            else return;

            var list = query.ToList();
            foreach (var n in list) Connection.connect.Notifications.Remove(n);
            Connection.connect.SaveChanges();

            var main = Window.GetWindow(this) as MainWindow;
            main?.LoadBellBadge();

            AppMessage.Success(Window.GetWindow(this), "Уведомления очищены");
            Load();
        }
    }
}