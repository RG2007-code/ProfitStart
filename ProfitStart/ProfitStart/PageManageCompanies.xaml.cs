using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageManageCompanies : Page, IRefreshablePage
    {
        public PageManageCompanies()
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
            if (CompaniesList == null) return;

            var companies = Connection.connect.Companies
                .OrderByDescending(c => c.Id)
                .ToList();

            var list = new List<object>();

            foreach (var c in companies)
            {
                string status;
                Brush color;

                if (c.IsDeleted == true)
                {
                    status = "✕ Удалена";
                    color = GetBrush("#DC2626");
                }
                else if (c.IsApproved == true && c.IsModerated != true)
                {
                    status = "✓ Одобрена";
                    color = GetBrush("#16A34A");
                }
                else if (c.IsModerated == true)
                {
                    status = "⏳ На модерации";
                    color = GetBrush("#D97706");
                }
                else
                {
                    status = "● Черновик";
                    color = GetBrush("#6B7280");
                }

                
                byte[] avatar = c.AvatarData;

                if ((avatar == null || avatar.Length == 0) && c.UserId.HasValue)
                {
                    var owner = Connection.connect.Users
                        .FirstOrDefault(u => u.Id == c.UserId.Value);

                    if (owner != null)
                        avatar = owner.AvatarData;
                }

                list.Add(new
                {
                    c.Id,
                    c.Name,
                    Info = (c.Direction ?? "—")
                           + " · ОГРН: "
                           + (c.OGRN ?? "—"),
                    Status = status,
                    StatusColor = color,
                    Reason = !string.IsNullOrEmpty(c.RejectionReason)
                        ? "Причина: " + c.RejectionReason : "",
                    AvatarData = avatar,
                    CanDelete = Session.IsAdmin
                        ? Visibility.Visible
                        : Visibility.Collapsed,

                    
                    ApproveVisibility = (Session.IsAdmin
                                         && c.IsDeleted != true
                                         && (c.IsApproved != true
                                             || c.IsModerated == true))
                        ? Visibility.Visible
                        : Visibility.Collapsed
                });
            }

            CompaniesList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private Brush GetBrush(string hex)
        {
            return new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(hex));
        }

        private void CompanyAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

       
        private void Card_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            int id = (int)border.Tag;
            NavigationService.Navigate(new PageCompanyCard(id));
        }

        
        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;

            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == id);
            if (c == null) return;

            c.IsApproved = true;
            c.IsModerated = false;
            c.RejectionReason = null;
            Connection.connect.SaveChanges();

            if (c.UserId.HasValue)
            {
                var user = Connection.connect.Users
                    .FirstOrDefault(u => u.Id == c.UserId.Value);
                if (user != null)
                {
                    Notify.ToCompany(c.Id,
                        "Компания одобрена",
                        "Ваша компания одобрена администратором",
                        "myvacancies", c.Id);
                }
            }

            AppMessage.Success(Window.GetWindow(this), "Компания одобрена");
            Load();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            NavigationService.Navigate(new PageCompanyCard(id));
        }

        private void Rating_Click(object sender, RoutedEventArgs e)
        {
            if (Session.IsAdmin)
            {
                AppMessage.Warning(Window.GetWindow(this),
                    "Рейтинг может ставить только студент",
                    "Недоступно");
                return;
            }

            int id = (int)((Button)sender).Tag;
            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == id);
            if (c == null) return;

            decimal current = c.Rating;
            var wnd = new RatingWindow(current);
            wnd.Owner = Window.GetWindow(this);

            if (wnd.ShowDialog() == true)
            {
                c.Rating = wnd.Rating;
                Connection.connect.SaveChanges();
                AppMessage.Success(Window.GetWindow(this), "Рейтинг обновлён");
                Load();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;

            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == id);
            if (c == null) return;

            var wnd = new RejectReasonWindow("Укажите причину удаления компании:");
            wnd.Owner = Window.GetWindow(this);

            if (wnd.ShowDialog() != true) return;

            c.IsDeleted = true;
            c.RejectionReason = wnd.Reason;
            Connection.connect.SaveChanges();

            if (c.UserId.HasValue)
            {
                var user = Connection.connect.Users
                    .FirstOrDefault(u => u.Id == c.UserId.Value);
                if (user != null)
                {
                    Notify.ToCompany(c.Id,
                        "Компания удалена",
                        "Ваша компания удалена. Причина: " + wnd.Reason,
                        "managecompanies", c.Id);
                }
            }

            AppMessage.Success(Window.GetWindow(this), "Компания удалена");
            Load();
        }
    }
}