using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PagePracticeApplications : Page, IRefreshablePage
    {
        private int _filterPracticeId = 0;

        public PagePracticeApplications()
        {
            InitializeComponent();
            Loaded += (s, e) => Load();
        }

        public PagePracticeApplications(int practiceId)
        {
            InitializeComponent();
            _filterPracticeId = practiceId;
            Loaded += (s, e) => Load();
        }

        public void RefreshAfterThemeChange()
        {
            Load();
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) Load();
        }

        private Brush GetBrush(string hex) =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

        private void Load()
        {
            if (AppsList == null) return;

            var query = Connection.connect.PracticeApplications.AsQueryable();

            if (_filterPracticeId > 0)
                query = query.Where(a => a.PracticeId == _filterPracticeId);

            if (Session.IsEmployer && Session.CompanyId != 0)
            {
                var myPracticeIds = Connection.connect.Practices
                    .Where(p => p.CompanyId == Session.CompanyId)
                    .Select(p => p.Id)
                    .ToList();

                query = query.Where(a => myPracticeIds.Contains(a.PracticeId));
            }

            var filter = (StatusFilter?.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "Все";

            var list = new List<object>();

            foreach (var a in query.OrderByDescending(x => x.CreatedAt).ToList())
            {
                if (filter == "Ожидают" && a.Status != "Отправлен") continue;
                if (filter == "Одобрены" && a.Status != "Одобрен") continue;
                if (filter == "Отклонены" && a.Status != "Отклонён") continue;

                var student = Connection.connect.Students
                    .FirstOrDefault(s => s.Id == a.StudentId);
                var practice = Connection.connect.Practices
                    .FirstOrDefault(p => p.Id == a.PracticeId);

                string st;
                Brush col;
                Visibility btns;

                if (a.Status == "Одобрен")
                {
                    st = "✓ Одобрен";
                    col = GetBrush("#16A34A");
                    btns = Visibility.Collapsed;
                }
                else if (a.Status == "Отклонён")
                {
                    st = "✕ Отклонён";
                    col = GetBrush("#DC2626");
                    btns = Visibility.Collapsed;
                }
                else
                {
                    st = "⏳ Ожидает";
                    col = GetBrush("#D97706");
                    btns = Visibility.Visible;
                }

                byte[] avatar = null;
                if (student != null && student.UserId.HasValue)
                {
                    var u = Connection.connect.Users
                        .FirstOrDefault(x => x.Id == student.UserId.Value);
                    if (u != null) avatar = u.AvatarData;
                }

                list.Add(new
                {
                    a.Id,
                    StudentId = a.StudentId,
                    StudentName = student != null ? student.FullName : "—",
                    PracticeTitle = practice != null ? practice.Title : "—",
                    Message = a.Message ?? "",
                    StatusText = st,
                    StatusColor = col,
                    ButtonsVisibility = btns,
                    AvatarData = avatar,

                    ProfileVisibility = (student != null)
                        ? Visibility.Visible
                        : Visibility.Collapsed,

                    CanMessage = (Session.IsEmployer && Session.CompanyId != 0)
                        ? Visibility.Visible
                        : Visibility.Collapsed
                });
            }

            AppsList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void StudentAvatar_Loaded(object sender, RoutedEventArgs e)
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
            var a = Connection.connect.PracticeApplications
                .FirstOrDefault(x => x.Id == id);
            if (a == null) return;

            a.Status = "Одобрен";
            Connection.connect.SaveChanges();

            var practice = Connection.connect.Practices
                .FirstOrDefault(p => p.Id == a.PracticeId);

            Notify.ToStudent(
                a.StudentId,
                "Отклик одобрен",
                practice != null
                    ? "Ваш отклик на практику «" + practice.Title + "» одобрен"
                    : "Ваш отклик на практику одобрен",
                "mypractices",
                a.PracticeId);

            AppMessage.Success(Window.GetWindow(this), "Отклик одобрен");
            Load();
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var a = Connection.connect.PracticeApplications
                .FirstOrDefault(x => x.Id == id);
            if (a == null) return;

            a.Status = "Отклонён";
            Connection.connect.SaveChanges();

            var practice = Connection.connect.Practices
                .FirstOrDefault(p => p.Id == a.PracticeId);

            Notify.ToStudent(
                a.StudentId,
                "Отклик отклонён",
                practice != null
                    ? "Ваш отклик на практику «" + practice.Title + "» отклонён"
                    : "Ваш отклик на практику отклонён",
                "mypractices",
                a.PracticeId);

            AppMessage.Info(Window.GetWindow(this), "Отклик отклонён");
            Load();
        }
    }
}