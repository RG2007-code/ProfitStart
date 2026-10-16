using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageModerationVacancies : Page
    {
        public PageModerationVacancies()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            var list = new List<object>();

            var pending = Connection.connect.Vacancies
                .Where(v => v.IsApproved == false && v.IsModerated == true)
                .OrderByDescending(v => v.CreatedAt)
                .ToList();

            foreach (var v in pending)
            {
                var company = Connection.connect.Companies
                    .FirstOrDefault(c => c.Id == v.CompanyId);

                byte[] avatar = v.AvatarData;
                if ((avatar == null || avatar.Length == 0) && company != null)
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

                list.Add(new
                {
                    v.Id,
                    v.Title,
                    Subtitle = (company != null ? company.Name : "—")
                               + " · " + (v.Type ?? ""),
                    Status = "На модерации",
                    AvatarData = avatar
                });
            }

            PendingList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;

            RefreshParentBadge();
        }

        private void VacancyAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void RefreshParentBadge()
        {
            var main = Window.GetWindow(this) as MainWindow;
            main?.LoadModVacBadge();
        }

        
        private void Card_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            
            var element = e.OriginalSource as DependencyObject;
            while (element != null && element != border)
            {
                if (element is Button) return;   
                element = VisualTreeHelper.GetParent(element);
            }

            int id = (int)border.Tag;
            NavigationService.Navigate(new PageVacancyCard(id));
        }

        
        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var v = Connection.connect.Vacancies
                .FirstOrDefault(x => x.Id == id);
            if (v == null) return;

            v.IsApproved = true;
            v.IsModerated = false;
            v.RejectionReason = null;
            Connection.connect.SaveChanges();

            if (v.CompanyId.HasValue)
            {
                Notify.ToCompany(v.CompanyId.Value,
                    "Вакансия одобрена",
                    "Ваша вакансия «" + v.Title + "» одобрена",
                    "myvacancies", v.Id);
            }

            MarkAdminNotifRead("modvacancies", v.Id);

            AppMessage.Success(Window.GetWindow(this), "Вакансия одобрена");
            Load();
        }

        
        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var v = Connection.connect.Vacancies
                .FirstOrDefault(x => x.Id == id);
            if (v == null) return;

            var wnd = new RejectReasonWindow(
                "Укажите причину отклонения вакансии:");
            wnd.Owner = Window.GetWindow(this);

            if (wnd.ShowDialog() == true)
            {
                v.RejectionReason = wnd.Reason;
                v.IsApproved = false;
                v.IsModerated = false;
                Connection.connect.SaveChanges();

                if (v.CompanyId.HasValue)
                {
                    Notify.ToCompany(v.CompanyId.Value,
                        "Вакансия отклонена",
                        "Ваша вакансия «" + v.Title + "» отклонена. Причина: "
                        + wnd.Reason,
                        "myvacancies", v.Id);
                }

                MarkAdminNotifRead("modvacancies", v.Id);

                AppMessage.Info(Window.GetWindow(this),
                    "Вакансия отклонена");
                Load();
            }
        }

        private void MarkAdminNotifRead(string page, int linkId)
        {
            var notifs = Connection.connect.Notifications
                .Where(n => n.RecipientType == "Admin"
                         && n.LinkPage == page
                         && n.LinkId == linkId
                         && n.IsRead == false)
                .ToList();

            foreach (var n in notifs)
                n.IsRead = true;

            Connection.connect.SaveChanges();
        }
    }
}