using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageModerationReviews : Page, IRefreshablePage
    {
        public PageModerationReviews()
        {
            InitializeComponent();
            Loaded += (s, e) => Load();
        }

        public void RefreshAfterThemeChange()
        {
            Load();
        }

        private void Load()
        {
            if (ReviewsList == null) return;

            
            var pending = Connection.connect.CompanyRatings
                .Where(r => r.Comment != null && r.Comment != ""
                         && r.IsModerated == true)
                .OrderByDescending(r => r.CommentedAt)
                .ToList();

            var list = new List<object>();

            foreach (var r in pending)
            {
                var company = Connection.connect.Companies
                    .FirstOrDefault(c => c.Id == r.CompanyId);

                var user = Connection.connect.Users
                    .FirstOrDefault(u => u.Id == r.UserId);

                string authorName = "Пользователь";
                if (user != null)
                {
                    var s = Connection.connect.Students
                        .FirstOrDefault(x => x.UserId == user.Id);
                    authorName = s?.FullName ?? user.Email;
                }

                list.Add(new
                {
                    r.Id,
                    CompanyName = company != null ? company.Name : "—",
                    AuthorName = authorName,
                    Comment = r.Comment,
                    Stars = new string('★', r.Value)
                          + new string('☆', 5 - r.Value)
                });
            }

            ReviewsList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            CompanyRating.Approve(id);
            AppMessage.Success(Window.GetWindow(this), "Отзыв одобрен");
            Load();
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;

            var wnd = new RejectReasonWindow("Укажите причину отклонения отзыва:");
            wnd.Owner = Window.GetWindow(this);

            if (wnd.ShowDialog() == true)
            {
                CompanyRating.Reject(id, wnd.Reason);
                AppMessage.Info(Window.GetWindow(this), "Отзыв отклонён");
                Load();
            }
        }
    }
}