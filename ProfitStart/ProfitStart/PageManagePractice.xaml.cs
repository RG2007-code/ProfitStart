using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageManagePractice : Page
    {
        public PageManagePractice()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            var query = Connection.connect.Practices.AsQueryable();

            if (Session.IsEmployer && Session.CompanyId != 0)
                query = query.Where(p => p.CompanyId == Session.CompanyId);

            var list = new List<object>();

            foreach (var p in query.OrderByDescending(p => p.Id).ToList())
            {
                byte[] avatar = p.AvatarData;
                if ((avatar == null || avatar.Length == 0) && p.CompanyId.HasValue)
                {
                    var c = Connection.connect.Companies
                        .FirstOrDefault(x => x.Id == p.CompanyId.Value);
                    if (c != null)
                    {
                        avatar = c.AvatarData;
                        if ((avatar == null || avatar.Length == 0) && c.UserId.HasValue)
                        {
                            var u = Connection.connect.Users
                                .FirstOrDefault(x => x.Id == c.UserId.Value);
                            if (u != null) avatar = u.AvatarData;
                        }
                    }
                }

                string status;
                Brush statusColor;
                if (p.IsApproved == true)
                {
                    status = "✓ Одобрена";
                    statusColor = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#16A34A"));
                }
                else if (p.IsModerated == true)
                {
                    status = "⏳ На модерации";
                    statusColor = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#D97706"));
                }
                else if (!string.IsNullOrEmpty(p.RejectionReason))
                {
                    status = "✕ Отклонена";
                    statusColor = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#DC2626"));
                }
                else
                {
                    status = "● Черновик";
                    statusColor = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#6B7280"));
                }

                string reason = !string.IsNullOrEmpty(p.RejectionReason)
                    ? "Причина: " + p.RejectionReason
                    : "";

                list.Add(new
                {
                    p.Id,
                    p.Title,
                    Subtitle = (p.Direction ?? "") + " · " + (p.Duration ?? ""),
                    Status = status,
                    StatusColor = statusColor,
                    Reason = reason,
                    AvatarData = avatar
                });
            }

            PracticeList.ItemsSource = list;
        }

        private void PracticeAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageAddPractice());
        }

        private void Card_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            var el = e.OriginalSource as DependencyObject;
            while (el != null && el != border)
            {
                if (el is Button) return;
                el = VisualTreeHelper.GetParent(el);
            }

            int id = (int)border.Tag;
            NavigationService.Navigate(new PagePracticeCard(id));
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            NavigationService.Navigate(new PageEditPractice(id));
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var p = Connection.connect.Practices
                .FirstOrDefault(x => x.Id == id);
            if (p == null) return;

            if (Session.IsAdmin)
            {
                var wnd = new RejectReasonWindow("Укажите причину удаления практики:");
                wnd.Owner = Window.GetWindow(this);
                if (wnd.ShowDialog() != true) return;

                if (p.CompanyId.HasValue)
                    Notify.ToCompany(p.CompanyId.Value, "Практика удалена",
                        "Ваша практика «" + p.Title + "» удалена. Причина: " + wnd.Reason,
                        "practice", p.Id);
            }
            else
            {
                if (!AppMessage.Confirm(Window.GetWindow(this),
                    "Удалить практику?", "Удаление"))
                    return;
            }

            var notifs = Connection.connect.Notifications
                .Where(n => n.LinkId == id &&
                       (n.LinkPage == "modpractice"
                     || n.LinkPage == "practice"
                     || n.LinkPage == "practiceapps"
                     || n.LinkPage == "mypractices"))
                .ToList();
            foreach (var n in notifs) Connection.connect.Notifications.Remove(n);

            var apps = Connection.connect.PracticeApplications
                .Where(a => a.PracticeId == id)
                .ToList();
            foreach (var a in apps) Connection.connect.PracticeApplications.Remove(a);

            try
            {
                Connection.connect.Database.ExecuteSqlCommand(
                    "DELETE FROM PracticeSkills WHERE PracticeId = {0}", id);
            }
            catch { }

            Connection.connect.SaveChanges();

            Connection.connect.Practices.Remove(p);
            Connection.connect.SaveChanges();

            AppMessage.Success(Window.GetWindow(this), "Практика удалена");
            Load();
        }
    }
}