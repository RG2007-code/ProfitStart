using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProfitStart
{
    public partial class PageModerationCompanies : Page
    {
        public PageModerationCompanies()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            var list = new List<object>();

            var companies = Connection.connect.Companies
                .Where(c => c.IsApproved == false
                         && c.IsDeleted != true
                         && c.IsModerated == true)
                .OrderByDescending(c => c.Id)
                .ToList();

            foreach (var c in companies)
            {
                byte[] avatar = null;
                if (c.UserId.HasValue)
                {
                    var owner = Connection.connect.Users
                        .AsNoTracking()
                        .FirstOrDefault(u => u.Id == c.UserId.Value);
                    if (owner != null) avatar = owner.AvatarData;
                }

                list.Add(new
                {
                    c.Id,
                    c.Name,
                    Info = (c.Direction ?? "—") + " · ОГРН: " + (c.OGRN ?? "—"),
                    Status = "Новая компания — на модерации",
                    AvatarData = avatar
                });
            }

            CompaniesList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CompanyAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;

            var data = ellipse.Tag as byte[];
            if (data == null || data.Length == 0) return;

            try
            {
                using (var ms = new MemoryStream(data))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();

                    ellipse.Fill = new ImageBrush(bmp)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
            }
            catch { }
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var c = Connection.connect.Companies.FirstOrDefault(x => x.Id == id);
            if (c == null) return;

            c.IsApproved = true;
            c.IsModerated = false;
            Connection.connect.SaveChanges();

           
            Notify.ToCompany(c.Id, "Компания одобрена",
                "Ваша компания «" + c.Name + "» одобрена",
                "myvacancies", c.Id);

            
            MarkAdminNotifRead("modcompanies", c.Id);

            AppMessage.Success(Window.GetWindow(this), "Компания одобрена");
            Load();

            var main = Window.GetWindow(this) as MainWindow;
            main?.LoadBellBadge();
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var c = Connection.connect.Companies.FirstOrDefault(x => x.Id == id);
            if (c == null) return;

            var wnd = new RejectReasonWindow("Укажите причину отклонения компании:");
            wnd.Owner = Window.GetWindow(this);

            if (wnd.ShowDialog() == true)
            {
                c.RejectionReason = wnd.Reason;
                c.IsApproved = false;
                c.IsModerated = false;
                Connection.connect.SaveChanges();

                Notify.ToCompany(c.Id, "Компания отклонена",
                    "Ваша компания «" + c.Name + "» отклонена. Причина: "
                    + wnd.Reason,
                    "myvacancies", c.Id);

                
                MarkAdminNotifRead("modcompanies", c.Id);

                AppMessage.Info(Window.GetWindow(this), "Компания отклонена");
                Load();

                var main = Window.GetWindow(this) as MainWindow;
                main?.LoadBellBadge();
            }
        }

        
        private void MarkAdminNotifRead(string linkPage, int linkId)
        {
            var notifs = Connection.connect.Notifications
                .Where(n => n.RecipientType == "Admin"
                         && n.LinkPage == linkPage
                         && n.LinkId == linkId
                         && n.IsRead == false)
                .ToList();

            foreach (var n in notifs)
                n.IsRead = true;

            Connection.connect.SaveChanges();
        }
    }
}