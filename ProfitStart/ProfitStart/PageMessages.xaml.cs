using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageMessages : Page, IRefreshablePage
    {
        public PageMessages()
        {
            InitializeComponent();
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

        private void Load()
        {
            if (DialogsList == null) return;

            var list = new List<object>();

            if (Session.IsStudent && Session.StudentId != 0)
            {
                var messages = Connection.connect.Messages
                    .Where(m => m.StudentId == Session.StudentId)
                    .ToList();

                var groups = messages
                    .GroupBy(m => m.CompanyId)
                    .ToList();

                foreach (var g in groups)
                {
                    var company = Connection.connect.Companies
                        .FirstOrDefault(c => c.Id == g.Key);

                    var last = g.OrderByDescending(m => m.CreatedAt).First();
                    int unread = g.Count(m => m.IsRead == false && m.SenderType == "Company");

                    byte[] avatar = company != null ? company.AvatarData : null;
                    if ((avatar == null || avatar.Length == 0)
                        && company != null && company.UserId.HasValue)
                    {
                        var u = Connection.connect.Users
                            .FirstOrDefault(x => x.Id == company.UserId.Value);
                        if (u != null) avatar = u.AvatarData;
                    }

                    list.Add(new
                    {
                        Id = g.Key,
                        Name = company != null ? company.Name : "Компания",
                        LastMessage = last.Text,
                        AvatarData = avatar,
                        UnreadVisibility = unread > 0
                            ? Visibility.Visible : Visibility.Collapsed,
                        UnreadCount = unread > 99 ? "99+" : unread.ToString()
                    });
                }
            }
            else if (Session.IsEmployer && Session.CompanyId != 0)
            {
                var messages = Connection.connect.Messages
                    .Where(m => m.CompanyId == Session.CompanyId)
                    .ToList();

                var groups = messages
                    .GroupBy(m => m.StudentId)
                    .ToList();

                foreach (var g in groups)
                {
                    var student = Connection.connect.Students
                        .FirstOrDefault(s => s.Id == g.Key);

                    var last = g.OrderByDescending(m => m.CreatedAt).First();
                    int unread = g.Count(m => m.IsRead == false && m.SenderType == "Student");

                    byte[] avatar = null;
                    if (student != null && student.UserId.HasValue)
                    {
                        var u = Connection.connect.Users
                            .FirstOrDefault(x => x.Id == student.UserId.Value);
                        if (u != null) avatar = u.AvatarData;
                    }

                    list.Add(new
                    {
                        Id = g.Key,
                        Name = student != null ? student.FullName : "Студент",
                        LastMessage = last.Text,
                        AvatarData = avatar,
                        UnreadVisibility = unread > 0
                            ? Visibility.Visible : Visibility.Collapsed,
                        UnreadCount = unread > 99 ? "99+" : unread.ToString()
                    });
                }
            }

            DialogsList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Avatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void Dialog_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;
            int id = (int)border.Tag;
            NavigationService.Navigate(new PageChat(id));
        }
    }
}