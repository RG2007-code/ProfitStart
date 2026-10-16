using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProfitStart
{
    public partial class PageModerationPractice : Page
    {
        public PageModerationPractice()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            var list = new List<object>();

            var pending = Connection.connect.Practices
                .Where(p => p.IsApproved == false && p.IsModerated == true)
                .OrderByDescending(p => p.CreatedAt).ToList();

            foreach (var p in pending)
            {
                list.Add(new
                {
                    p.Id,
                    p.Title,
                    Subtitle = (p.Direction ?? "") + " · " + (p.Duration ?? ""),
                    Status = "На модерации",
                    p.AvatarData
                });
            }

            PendingList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void PracticeAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var p = Connection.connect.Practices
                .FirstOrDefault(x => x.Id == id);
            if (p == null) return;

            p.IsApproved = true;
            p.IsModerated = false;
            p.RejectionReason = null;
            Connection.connect.SaveChanges();

            if (p.CompanyId.HasValue)
                Notify.ToCompany(p.CompanyId.Value, "Практика одобрена",
                    "Ваша практика «" + p.Title + "» одобрена",
                    "practice", p.Id);

            AppMessage.Success(Window.GetWindow(this), "Практика одобрена");
            Load();
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var p = Connection.connect.Practices
                .FirstOrDefault(x => x.Id == id);
            if (p == null) return;

            var wnd = new RejectReasonWindow("Укажите причину отклонения практики:");
            wnd.Owner = Window.GetWindow(this);

            if (wnd.ShowDialog() == true)
            {
                p.RejectionReason = wnd.Reason;
                p.IsApproved = false;
                p.IsModerated = false;
                Connection.connect.SaveChanges();

                if (p.CompanyId.HasValue)
                    Notify.ToCompany(p.CompanyId.Value, "Практика отклонена",
                        "Ваша практика «" + p.Title + "» отклонена. Причина: " + wnd.Reason,
                        "practice", p.Id);

                AppMessage.Info(Window.GetWindow(this), "Практика отклонена");
                Load();
            }
        }
    }
}