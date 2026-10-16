using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageManageUsers : Page, IRefreshablePage
    {
        public PageManageUsers()
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
            if (UsersList == null) return;

            var text = SearchBox?.Text?.Trim() ?? "";

            var users = Connection.connect.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToList();

            var list = new List<object>();

            foreach (var u in users)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    var email = u.Email ?? "";
                    var phone = u.Phone ?? "";

                    if (email.IndexOf(text, StringComparison.OrdinalIgnoreCase) < 0
                        && phone.IndexOf(text, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                string roleName = u.Roles != null ? u.Roles.Name : "—";

                string info = "Телефон: " + (u.Phone ?? "—")
                            + " · Создан: "
                            + (u.CreatedAt.HasValue
                                ? u.CreatedAt.Value.ToString("dd.MM.yyyy")
                                : "—");

                bool isSelf = (u.Id == Session.UserId);
                bool isBanned = Connection.connect.Bans
                    .Any(b => b.UserId == u.Id);

                Visibility canManage = isSelf
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                list.Add(new
                {
                    u.Id,
                    u.Email,
                    Info = info,
                    Role = roleName,
                    AvatarData = u.AvatarData,
                    CanMessage = canManage,
                    CanBan = canManage,
                    CanDelete = canManage,
                    BanButtonText = isBanned ? "Разбанить" : "Забанить"
                });
            }

            UsersList.ItemsSource = list;
        }

        private void UserAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void Message_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;

            if (userId == Session.UserId) return;

            var student = Connection.connect.Students
                .FirstOrDefault(s => s.UserId == userId);

            if (student != null)
            {
                NavigationService.Navigate(new PageChat(student.Id));
                return;
            }

            var company = Connection.connect.Companies
                .FirstOrDefault(c => c.UserId == userId);

            if (company != null)
            {
                NavigationService.Navigate(new PageChat(company.Id));
                return;
            }

            AppMessage.Warning(Window.GetWindow(this),
                "Не удалось открыть чат");
        }

        private void BanOrUnban_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;

            if (userId == Session.UserId) return;

            var ban = Connection.connect.Bans
                .FirstOrDefault(b => b.UserId == userId);

            if (ban != null)
            {
                if (!AppMessage.Confirm(Window.GetWindow(this),
                    "Разбанить пользователя?", "Разблокировка"))
                    return;

                BannedUsers.Unban(ban.Id);
                AppMessage.Success(Window.GetWindow(this),
                    "Пользователь разбанен");
                Load();
                return;
            }

            var wnd = new BanWindow(userId);
            wnd.Owner = Window.GetWindow(this);

            if (wnd.ShowDialog() == true)
            {
                Load();
            }
        }

        
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;

            if (userId == Session.UserId) return;

            if (!AppMessage.Confirm(Window.GetWindow(this),
                "Удалить пользователя окончательно?\n"
                + "Все его данные будут удалены безвозвратно:\n"
                + "• профиль (студент / компания)\n"
                + "• отклики\n"
                + "• сообщения\n"
                + "• уведомления\n"
                + "• отзывы и оценки\n"
                + "• резюме PDF\n"
                + "• запись о бане",
                "Окончательное удаление"))
                return;

            try
            {
                CascadeDeleteUser(userId);
                AppMessage.Success(Window.GetWindow(this),
                    "Пользователь удалён со всеми данными");
                Load();
            }
            catch (Exception ex)
            {
                AppMessage.Error(Window.GetWindow(this),
                    "Ошибка удаления: " + ex.Message);
            }
        }

        
        private void CascadeDeleteUser(int userId)
        {
            
            var bans = Connection.connect.Bans
                .Where(b => b.UserId == userId).ToList();
            foreach (var b in bans)
                Connection.connect.Bans.Remove(b);

            
            var resumes = Connection.connect.Resumes
                .Where(r => r.UserId == userId).ToList();
            foreach (var r in resumes)
                Connection.connect.Resumes.Remove(r);

            
            var student = Connection.connect.Students
                .FirstOrDefault(s => s.UserId == userId);

            int? studentId = student != null ? (int?)student.Id : null;

            if (studentId.HasValue)
            {
                
                var practiceApps = Connection.connect.PracticeApplications
                    .Where(a => a.StudentId == studentId.Value).ToList();
                foreach (var a in practiceApps)
                    Connection.connect.PracticeApplications.Remove(a);

                
                var companyApps = Connection.connect.CompanyApplications
                    .Where(a => a.StudentId == studentId.Value).ToList();
                foreach (var a in companyApps)
                    Connection.connect.CompanyApplications.Remove(a);

                
                var messages = Connection.connect.Messages
                    .Where(m => m.StudentId == studentId.Value).ToList();
                foreach (var m in messages)
                    Connection.connect.Messages.Remove(m);

                
                var notifs = Connection.connect.Notifications
                    .Where(n => n.StudentId == studentId.Value).ToList();
                foreach (var n in notifs)
                    Connection.connect.Notifications.Remove(n);

                
                var reviews = Connection.connect.CompanyRatings
                    .Where(r => r.UserId == userId).ToList();
                foreach (var r in reviews)
                    Connection.connect.CompanyRatings.Remove(r);

                
                var studentSkills = student.Skills.ToList();
                foreach (var sk in studentSkills)
                    student.Skills.Remove(sk);

               
                Connection.connect.Students.Remove(student);
            }

            
            var company = Connection.connect.Companies
                .FirstOrDefault(c => c.UserId == userId);

            if (company != null)
            {
                int companyId = company.Id;

                
                var practices = Connection.connect.Practices
                    .Where(p => p.CompanyId == companyId).ToList();

                var practiceIds = practices.Select(p => p.Id).ToList();

                foreach (var pa in Connection.connect.PracticeApplications
                    .Where(a => practiceIds.Contains(a.PracticeId)).ToList())
                    Connection.connect.PracticeApplications.Remove(pa);

                foreach (var p in practices)
                    Connection.connect.Practices.Remove(p);

               
                var vacancies = Connection.connect.Vacancies
                    .Where(v => v.CompanyId == companyId).ToList();

                var vacancyIds = vacancies.Select(v => v.Id).ToList();

                foreach (var ca in Connection.connect.CompanyApplications
                    .Where(a => vacancyIds.Contains(a.VacancyId)).ToList())
                    Connection.connect.CompanyApplications.Remove(ca);

                foreach (var v in vacancies)
                    Connection.connect.Vacancies.Remove(v);

               
                foreach (var m in Connection.connect.Messages
                    .Where(m => m.CompanyId == companyId).ToList())
                    Connection.connect.Messages.Remove(m);

                
                foreach (var n in Connection.connect.Notifications
                    .Where(n => n.CompanyId == companyId).ToList())
                    Connection.connect.Notifications.Remove(n);

                
                foreach (var r in Connection.connect.CompanyRatings
                    .Where(r => r.CompanyId == companyId).ToList())
                    Connection.connect.CompanyRatings.Remove(r);

               
                Connection.connect.Companies.Remove(company);
            }

           
            Connection.connect.SaveChanges();

            
            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                Connection.connect.Users.Remove(user);
                Connection.connect.SaveChanges();
            }
        }

        private void ShowBans_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageBannedUsers());
        }
    }
}