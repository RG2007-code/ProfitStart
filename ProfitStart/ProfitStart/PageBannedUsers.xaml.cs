using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageBannedUsers : Page, IRefreshablePage
    {
        public PageBannedUsers()
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
            if (BansList == null) return;

            var now = DateTime.Now;
            bool dark = AppTheme.IsDark;

            var list = Connection.connect.Bans
                .OrderByDescending(b => b.BannedAt)
                .ToList();

            var display = list.Select(b =>
            {
                bool expired = b.BanUntil.HasValue
                            && b.BanUntil.Value < now;

                string statusText;
                string statusBg;
                string rowBg;

                if (expired)
                {
                    statusText = "ИСТЁК";
                    statusBg = "#D97706";
                    rowBg = dark ? "#78350F" : "#FEF3C7";
                }
                else if (b.BanUntil.HasValue)
                {
                    statusText = "АКТИВЕН";
                    statusBg = "#DC2626";
                    rowBg = dark ? "#1E293B" : "#FFFFFF";
                }
                else
                {
                    statusText = "НАВСЕГДА";
                    statusBg = "#7F1D1D";
                    rowBg = dark ? "#1E293B" : "#FFFFFF";
                }

                var brushRow = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(rowBg));
                var brushStatus = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(statusBg));

                return new
                {
                    b.Id,
                    b.Email,
                    AvatarData = GetAvatarForBan(b.UserId),
                    Info = "Телефон: " + (b.Phone ?? "—")
                           + " · Забанен: "
                           + b.BannedAt.ToString("dd.MM.yyyy")
                           + " · Срок: "
                           + (b.BanUntil.HasValue
                               ? b.BanUntil.Value.ToString("dd.MM.yyyy")
                               : "навсегда"),
                    Reason = "Причина: " + (b.Reason ?? "—"),
                    StatusText = statusText,
                    StatusBg = brushStatus,
                    RowBackground = brushRow
                };
            }).ToList();

            BansList.ItemsSource = display;
            EmptyText.Visibility = display.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private byte[] GetAvatarForBan(int? userId)
        {
            if (!userId.HasValue) return null;

            var u = Connection.connect.Users
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == userId.Value);

            return u?.AvatarData;
        }

        private void BanAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void Unban_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;

            if (!AppMessage.Confirm(Window.GetWindow(this),
                "Разбанить пользователя?", "Разбан"))
                return;

            BannedUsers.Unban(id);

            AppMessage.Success(Window.GetWindow(this), "Пользователь разбанен");
            Load();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageManageUsers());
        }
    }
}