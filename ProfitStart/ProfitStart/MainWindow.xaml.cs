using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProfitStart
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _bannerTimer;
        private bool _bannerDismissed = false;
        private bool _isDark = false;

        public MainWindow()
        {
            InitializeComponent();

            _isDark = AppTheme.IsDark;

            ApplyRole();
            LoadBellBadge();
            LoadMailBadge();
            LoadModVacBadge();
            LoadBanners();
            RefreshAvatar();
            GlobalSearchPlaceholder.Visibility = Visibility.Visible;

            ApplyTheme(_isDark);

            MainFrame.Navigate(new PageMain());

            MainFrame.Navigated += (s, e) =>
            {
                if (MainFrame.Content is Page)
                {
                    ApplyTheme(_isDark);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ApplyTheme(_isDark);
                    }), DispatcherPriority.Background);
                }
            };

            _bannerTimer = new DispatcherTimer();
            _bannerTimer.Interval = TimeSpan.FromSeconds(2);
            _bannerTimer.Tick += (s, e) =>
            {
                LoadBanners();
                LoadMailBadge();
                LoadBellBadge();
                Session.Touch();
            };
            _bannerTimer.Start();
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            _isDark = !_isDark;
            ApplyTheme(_isDark);
        }

        private void ApplyTheme(bool dark)
        {
            AppTheme.IsDark = dark;

            var res = Application.Current.Resources;

            if (dark)
            {
                res["PageBg"] = Brush("#0F172A");
                res["CardBg"] = Brush("#1E293B");
                res["TextPrimary"] = Brush("#E5E7EB");
                res["TextMuted"] = Brush("#E5E7EB");
                res["TextPlaceholder"] = Brush("#E5E7EB");
                res["AccentBgSoft"] = Brush("#334155");
                res["BorderLine"] = Brush("#1E293B");
                res["SidebarBg"] = Brush("#111827");
                res["HeaderBg"] = Brush("#111827");

                Background = Brush("#0F172A");
                HeaderBorder.Background = Brush("#111827");
                HeaderBorder.BorderBrush = Brush("#1E293B");
                SidebarBorder.Background = Brush("#111827");
                SidebarBorder.BorderBrush = Brush("#1E293B");
                SearchBorder.Background = Brush("#1E293B");
                SearchOverlay.Background = Brush("#0F172A");
                AppTitleText.Foreground = Brush("#E5E7EB");

                GlobalSearchBox.Foreground = Brush("#E5E7EB");
                GlobalSearchBox.CaretBrush = Brush("#E5E7EB");
                GlobalSearchPlaceholder.Foreground = Brush("#E5E7EB");

                ThemeToggle.Content = "☀";
                ThemeToggle.Foreground = Brush("#FBBF24");
                ThemeToggle.Visibility = Visibility.Visible;

                foreach (var el in MailIcon.Children)
                    if (el is TextBlock tb) tb.Foreground = Brush("#E5E7EB");
                foreach (var el in BellIcon.Children)
                    if (el is TextBlock tb) tb.Foreground = Brush("#E5E7EB");

                foreach (var btn in FindVisualChildren<Button>(this))
                {
                    if (btn.Style == (Style)FindResource("MenuBtn"))
                        btn.Foreground = Brush("#E5E7EB");
                }

                if (SearchTitle != null)
                    SearchTitle.Foreground = Brush("#E5E7EB");
            }
            else
            {
                res["PageBg"] = Brush("#F5F6FA");
                res["CardBg"] = new SolidColorBrush(Colors.White);
                res["TextPrimary"] = Brush("#111827");
                res["TextMuted"] = Brush("#6B7280");
                res["TextPlaceholder"] = Brush("#9CA3AF");
                res["AccentBgSoft"] = Brush("#EEF2FF");
                res["BorderLine"] = Brush("#E5E7EB");
                res["SidebarBg"] = new SolidColorBrush(Colors.White);
                res["HeaderBg"] = new SolidColorBrush(Colors.White);

                Background = Brush("#F5F6FA");
                HeaderBorder.Background = Brushes.White;
                HeaderBorder.BorderBrush = Brush("#E5E7EB");
                SidebarBorder.Background = Brushes.White;
                SidebarBorder.BorderBrush = Brush("#E5E7EB");
                SearchBorder.Background = Brush("#F5F6FA");
                SearchOverlay.Background = Brush("#F5F6FA");
                AppTitleText.Foreground = Brush("#0F172A");

                GlobalSearchBox.Foreground = Brush("#374151");
                GlobalSearchBox.CaretBrush = Brush("#111827");
                GlobalSearchPlaceholder.Foreground = Brush("#9CA3AF");

                ThemeToggle.Content = "🌙";
                ThemeToggle.Foreground = Brush("#374151");
                ThemeToggle.Visibility = Visibility.Visible;

                foreach (var el in MailIcon.Children)
                    if (el is TextBlock tb) tb.Foreground = Brush("#374151");
                foreach (var el in BellIcon.Children)
                    if (el is TextBlock tb) tb.Foreground = Brush("#374151");

                foreach (var btn in FindVisualChildren<Button>(this))
                {
                    if (btn.Style == (Style)FindResource("MenuBtn"))
                        btn.Foreground = Brush("#374151");
                }

                if (SearchTitle != null)
                    SearchTitle.Foreground = Brush("#111827");
            }

            LoadBanners();

            if (MainFrame.Content is IRefreshablePage refreshable)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    refreshable.RefreshAfterThemeChange();
                }), DispatcherPriority.Background);
            }
        }

        public static SolidColorBrush Brush(string hex) =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) yield break;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t) yield return t;
                foreach (var c in FindVisualChildren<T>(child)) yield return c;
            }
        }

        private void ApplyRole()
        {
            bool auth = Session.IsAuthenticated;

            ThemeToggle.Visibility = Visibility.Visible;

            BtnPracticeApps.Visibility = Session.IsEmployer
                ? Visibility.Visible : Visibility.Collapsed;

            BtnApps.Visibility = (Session.IsStudent || Session.IsEmployer)
                ? Visibility.Visible : Visibility.Collapsed;

            if (BtnMyPractices != null)
                BtnMyPractices.Visibility = Session.IsStudent
                    ? Visibility.Visible : Visibility.Collapsed;

            BtnPractice.Visibility = (auth && !Session.IsAdmin)
                ? Visibility.Visible : Visibility.Collapsed;

            BtnProfile.Visibility = auth ? Visibility.Visible : Visibility.Collapsed;
            BtnMessages.Visibility = auth ? Visibility.Visible : Visibility.Collapsed;
            BtnNotif.Visibility = auth ? Visibility.Visible : Visibility.Collapsed;

            BtnAdmin.Visibility = Session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnModerationCompanies.Visibility = Session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

            if (BtnCompanyDisputes != null)
                BtnCompanyDisputes.Visibility = Session.IsAdmin
                    ? Visibility.Visible : Visibility.Collapsed;

            BtnModVacanciesGrid.Visibility = Session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnManageCompanies.Visibility = Session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnManageUsers.Visibility = Session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

            BtnMyVacancies.Visibility = Session.IsEmployer ? Visibility.Visible : Visibility.Collapsed;

            if (BtnManagePractice != null)
                BtnManagePractice.Visibility = (Session.IsAdmin || Session.IsEmployer)
                    ? Visibility.Visible : Visibility.Collapsed;

            if (BtnModerationPractice != null)
                BtnModerationPractice.Visibility = Session.IsAdmin
                    ? Visibility.Visible : Visibility.Collapsed;

            if (BtnModerationReviews != null)
                BtnModerationReviews.Visibility = Session.IsAdmin
                    ? Visibility.Visible : Visibility.Collapsed;

            BtnLogin.Visibility = auth ? Visibility.Collapsed : Visibility.Visible;
            BtnLogout.Visibility = auth ? Visibility.Visible : Visibility.Collapsed;
            BellIcon.Visibility = auth ? Visibility.Visible : Visibility.Collapsed;
            MailIcon.Visibility = auth ? Visibility.Visible : Visibility.Collapsed;
            AvatarIcon.Visibility = auth ? Visibility.Visible : Visibility.Collapsed;
        }

        public void LoadBellBadge()
        {
            if (!Session.IsAuthenticated)
            {
                BellBadge.Visibility = Visibility.Collapsed;
                return;
            }

            int count = 0;
            if (Session.IsStudent && Session.StudentId != 0)
                count = Connection.connect.Notifications
                    .Count(n => n.StudentId == Session.StudentId && n.IsRead == false);
            else if (Session.IsEmployer && Session.CompanyId != 0)
                count = Connection.connect.Notifications
                    .Count(n => n.CompanyId == Session.CompanyId && n.IsRead == false);
            else if (Session.IsAdmin || Session.IsCareer)
                count = Connection.connect.Notifications
                    .Count(n => n.RecipientType == "Admin" && n.IsRead == false);

            if (count > 0)
            {
                BellCount.Text = count > 99 ? "99+" : count.ToString();
                BellBadge.Visibility = Visibility.Visible;
            }
            else BellBadge.Visibility = Visibility.Collapsed;
        }

        public void LoadMailBadge()
        {
            if (!Session.IsAuthenticated)
            {
                MailBadge.Visibility = Visibility.Collapsed;
                return;
            }

            int count = 0;
            if (Session.IsStudent && Session.StudentId != 0)
                count = Connection.connect.Messages
                    .Count(m => m.StudentId == Session.StudentId
                             && m.SenderType == "Company"
                             && m.IsRead == false);
            else if (Session.IsEmployer && Session.CompanyId != 0)
                count = Connection.connect.Messages
                    .Count(m => m.CompanyId == Session.CompanyId
                             && m.SenderType == "Student"
                             && m.IsRead == false);

            if (count > 0)
            {
                MailCount.Text = count > 99 ? "99+" : count.ToString();
                MailBadge.Visibility = Visibility.Visible;
            }
            else MailBadge.Visibility = Visibility.Collapsed;
        }

        public void RefreshBellBadge() { LoadBellBadge(); }

        public void LoadModVacBadge()
        {
            if (!Session.IsAdmin)
            {
                if (ModVacBadge != null) ModVacBadge.Visibility = Visibility.Collapsed;
                return;
            }
            int count = Connection.connect.Vacancies
                .Count(v => v.IsApproved == false && v.IsModerated == true);
            if (count > 0)
            {
                ModVacCount.Text = count > 99 ? "99+" : count.ToString();
                ModVacBadge.Visibility = Visibility.Visible;
            }
            else ModVacBadge.Visibility = Visibility.Collapsed;
        }

        public void LoadBanners()
        {
            if (TopBanner == null) return;

            if (_bannerDismissed)
            {
                TopBanner.Visibility = Visibility.Collapsed;
                return;
            }

            if (Session.IsEmployer && Session.CompanyId != 0)
            {
                var c = Connection.connect.Companies
                    .FirstOrDefault(x => x.Id == Session.CompanyId);

                if (c != null && c.IsDeleted == true)
                {
                    TopBannerText.Text = "Ваша компания удалена. Причина: "
                                       + (c.RejectionReason ?? "не указана");

                    if (AppTheme.IsDark)
                    {
                        TopBanner.Background = Brush("#7F1D1D");
                        TopBannerText.Foreground = Brush("#FCA5A5");
                    }
                    else
                    {
                        TopBanner.Background = Brush("#FEE2E2");
                        TopBannerText.Foreground = Brush("#DC2626");
                    }

                    if (TopBannerButton != null)
                        TopBannerButton.Visibility = Visibility.Collapsed;

                    TopBanner.Visibility = Visibility.Visible;
                    return;
                }

                if (c != null && c.IsApproved == false)
                {
                    TopBannerText.Text = "Ваша компания на модерации. Дождитесь одобрения админа.";

                    if (AppTheme.IsDark)
                    {
                        TopBanner.Background = Brush("#78350F");
                        TopBannerText.Foreground = Brush("#FCD34D");
                    }
                    else
                    {
                        TopBanner.Background = Brush("#FEF3C7");
                        TopBannerText.Foreground = Brush("#D97706");
                    }

                    if (TopBannerButton != null)
                    {
                        TopBannerButton.Content = "Мои вакансии";
                        TopBannerButton.Tag = "myvacancies";
                        TopBannerButton.Visibility = Visibility.Visible;
                    }

                    TopBanner.Visibility = Visibility.Visible;
                    return;
                }
            }

            if (Session.IsStudent && Session.StudentId != 0)
            {
                var s = Connection.connect.Students
                    .FirstOrDefault(x => x.Id == Session.StudentId);
                if (s != null && (string.IsNullOrEmpty(s.ResumeText)
                               || s.PassportData == null
                               || s.PassportData.Length == 0))
                {
                    TopBannerText.Text = "Заполните профиль: резюме и паспорт обязательны.";

                    if (AppTheme.IsDark)
                    {
                        TopBanner.Background = Brush("#1E3A8A");
                        TopBannerText.Foreground = Brush("#93C5FD");
                    }
                    else
                    {
                        TopBanner.Background = Brush("#EEF2FF");
                        TopBannerText.Foreground = Brush("#5B5BF5");
                    }

                    if (TopBannerButton != null)
                    {
                        TopBannerButton.Content = "Заполнить";
                        TopBannerButton.Tag = "editprofile";
                        TopBannerButton.Visibility = Visibility.Visible;
                    }

                    TopBanner.Visibility = Visibility.Visible;
                    return;
                }
            }

            TopBanner.Visibility = Visibility.Collapsed;
            if (TopBannerButton != null)
                TopBannerButton.Visibility = Visibility.Collapsed;
        }

        private void TopBannerButton_Click(object sender, RoutedEventArgs e)
        {
            var tag = TopBannerButton.Tag?.ToString();

            if (tag == "myvacancies")
                MainFrame.Navigate(new PageMyVacancies());
            else if (tag == "editprofile")
                MainFrame.Navigate(new PageEditProfile());
        }

        private void HideBanner_Click(object sender, RoutedEventArgs e)
        {
            _bannerDismissed = true;
            TopBanner.Visibility = Visibility.Collapsed;
        }

        public void RefreshAvatar()
        {
            if (AvatarImage == null) return;

            if (!Session.IsAuthenticated)
            {
                ShowAvatarPlaceholder(null);
                return;
            }

            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Id == Session.UserId);

            if (user == null
                || user.AvatarData == null
                || user.AvatarData.Length == 0)
            {
                ShowAvatarPlaceholder(user);
                return;
            }

            AvatarImage.Fill = null;
            Image.LoadIntoEllipse(AvatarImage, user.AvatarData);

            if (AvatarPlaceholderText != null)
                AvatarPlaceholderText.Visibility = Visibility.Collapsed;
        }

        private void ShowAvatarPlaceholder(Users user)
        {
            AvatarImage.Fill = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#5B5BF5"));

            string letter = "?";

            if (user != null && !string.IsNullOrEmpty(user.Email))
                letter = user.Email.Substring(0, 1).ToUpper();

            if (AvatarPlaceholderText != null)
            {
                AvatarPlaceholderText.Text = letter;
                AvatarPlaceholderText.Visibility = Visibility.Visible;
            }
        }

        public void OpenPage(Page page) { MainFrame.Navigate(page); }

        private void GlobalSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GlobalSearchPlaceholder != null)
                GlobalSearchPlaceholder.Visibility =
                    string.IsNullOrEmpty(GlobalSearchBox.Text)
                    ? Visibility.Visible : Visibility.Collapsed;

            var query = GlobalSearchBox.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                SearchOverlay.Visibility = Visibility.Collapsed;
                return;
            }

            SearchOverlay.Visibility = Visibility.Visible;
            SearchTitle.Text = "Результаты: \"" + query + "\"";
            SearchResultsPanel.Children.Clear();
            SearchTitle.Foreground = _isDark ? Brush("#E5E7EB") : Brush("#111827");

            var vacancies = Connection.connect.Vacancies
                .Where(v => v.IsApproved == true
                         && (v.Companies == null || v.Companies.IsDeleted != true)
                         && (v.Title.Contains(query) ||
                             v.Description.Contains(query) ||
                             v.Requirements.Contains(query)))
                .Take(10).ToList();

            if (vacancies.Any())
            {
                AddSearchHeader("Вакансии");
                foreach (var v in vacancies)
                {
                    byte[] avatar = v.AvatarData;
                    if ((avatar == null || avatar.Length == 0) && v.CompanyId.HasValue)
                    {
                        var company = Connection.connect.Companies
                            .FirstOrDefault(c => c.Id == v.CompanyId.Value);
                        if (company != null) avatar = company.AvatarData;
                    }
                    AddSearchCard(avatar, v.Title,
                        (v.Companies != null ? v.Companies.Name : "—") + " · " + v.Type,
                        new PageVacancyCard(v.Id));
                }
            }

            var companies = Connection.connect.Companies
                .Where(c => c.IsApproved == true && c.IsDeleted != true &&
                       (c.Name.Contains(query) ||
                        c.Direction.Contains(query) ||
                        c.Description.Contains(query)))
                .Take(10).ToList();

            if (companies.Any())
            {
                AddSearchHeader("Компании");
                foreach (var c in companies)
                {
                    byte[] avatar = c.AvatarData;
                    if ((avatar == null || avatar.Length == 0) && c.UserId.HasValue)
                    {
                        var owner = Connection.connect.Users
                            .FirstOrDefault(u => u.Id == c.UserId.Value);
                        if (owner != null) avatar = owner.AvatarData;
                    }
                    AddSearchCard(avatar, c.Name, c.Direction ?? "",
                        new PageCompanyCard(c.Id));
                }
            }

            var deletedCompanyIds = Connection.connect.Companies
                .Where(c => c.IsDeleted == true)
                .Select(c => c.Id)
                .ToList();

            var practices = Connection.connect.Practices
                .Where(p => p.IsApproved == true
                         && (p.CompanyId == null
                             || !deletedCompanyIds.Contains(p.CompanyId.Value))
                         && (p.Title.Contains(query) ||
                             p.Direction.Contains(query)))
                .Take(10).ToList();

            if (practices.Any())
            {
                AddSearchHeader("Практики");
                foreach (var p in practices)
                {
                    AddSearchCard(p.AvatarData, p.Title,
                        (p.Direction ?? "") + " · " + (p.Duration ?? ""),
                        new PagePractice());
                }
            }

            if (!vacancies.Any() && !companies.Any() && !practices.Any())
            {
                SearchResultsPanel.Children.Add(new TextBlock
                {
                    Text = "Ничего не найдено",
                    Foreground = _isDark ? Brush("#E5E7EB") : Brushes.Gray,
                    FontSize = 14,
                    Margin = new Thickness(0, 20, 0, 0)
                });
            }
        }

        private void AddSearchHeader(string text)
        {
            SearchResultsPanel.Children.Add(new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 16, 0, 8),
                Foreground = _isDark ? Brush("#E5E7EB") : Brush("#111827")
            });
        }

        private void AddSearchCard(byte[] avatar, string title,
            string subtitle, Page targetPage)
        {
            var border = new Border
            {
                Background = _isDark ? Brush("#1E293B") : Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 10),
                Cursor = Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var avatarGrid = new Grid { Width = 40, Height = 40 };
            avatarGrid.Children.Add(new System.Windows.Shapes.Ellipse
            {
                Width = 40,
                Height = 40,
                Fill = _isDark ? Brush("#334155") : Brush("#EEF2FF")
            });

            if (avatar != null && avatar.Length > 0)
            {
                var ellipse = new System.Windows.Shapes.Ellipse { Width = 40, Height = 40 };
                Image.LoadIntoEllipse(ellipse, avatar);
                avatarGrid.Children.Add(ellipse);
            }

            Grid.SetColumn(avatarGrid, 0);
            grid.Children.Add(avatarGrid);

            var sp = new StackPanel { Margin = new Thickness(16, 0, 0, 0) };
            sp.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = _isDark ? Brush("#E5E7EB") : Brush("#111827")
            });
            sp.Children.Add(new TextBlock
            {
                Text = subtitle,
                Foreground = _isDark ? Brush("#E5E7EB") : Brushes.Gray,
                FontSize = 12,
                Margin = new Thickness(0, 4, 0, 0)
            });

            Grid.SetColumn(sp, 1);
            grid.Children.Add(sp);

            border.Child = grid;
            border.MouseLeftButtonUp += (s, e) =>
            {
                MainFrame.Navigate(targetPage);
                SearchOverlay.Visibility = Visibility.Collapsed;
                GlobalSearchBox.Text = "";
            };

            SearchResultsPanel.Children.Add(border);
        }

        private void CloseSearchOverlay_Click(object sender, RoutedEventArgs e)
        {
            SearchOverlay.Visibility = Visibility.Collapsed;
            GlobalSearchBox.Text = "";
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();
            switch (tag)
            {
                case "main": MainFrame.Navigate(new PageMain()); break;
                case "vacancies": MainFrame.Navigate(new PageVacancies()); break;
                case "practice": MainFrame.Navigate(new PagePractice()); break;
                case "companies": MainFrame.Navigate(new PageCompanies()); break;
                case "apps": MainFrame.Navigate(new PageApplications()); break;
                case "mypractices": MainFrame.Navigate(new PageMyPractices()); break;
                case "messages": MainFrame.Navigate(new PageMessages()); break;
                case "notif": MainFrame.Navigate(new PageNotifications()); break;
                case "profile": MainFrame.Navigate(new PageProfile()); break;
                case "myvacancies": MainFrame.Navigate(new PageMyVacancies()); break;
                case "admin": MainFrame.Navigate(new PageAdmin()); break;
                case "users": MainFrame.Navigate(new PageManageUsers()); break;
                case "modvacancies": MainFrame.Navigate(new PageModerationVacancies()); break;
                case "modcompanies": MainFrame.Navigate(new PageModerationCompanies()); break;
                case "companydisputes": MainFrame.Navigate(new PageCompanyDisputes()); break;
                case "managecompanies": MainFrame.Navigate(new PageManageCompanies()); break;
                case "managepractice": MainFrame.Navigate(new PageManagePractice()); break;
                case "modpractice": MainFrame.Navigate(new PageModerationPractice()); break;
                case "skills": MainFrame.Navigate(new PageManageSkills()); break;
                case "practiceapps": MainFrame.Navigate(new PagePracticeApplications()); break;
                case "modreviews": MainFrame.Navigate(new PageModerationReviews()); break;
            }

            LoadBanners();
            Session.Touch();
        }

        private void Bell_Click(object sender, MouseButtonEventArgs e)
        {
            if (!Session.IsAuthenticated)
            {
                AppMessage.Warning(this, "Сначала войдите", "Уведомления");
                return;
            }
            MainFrame.Navigate(new PageNotifications());
        }

        private void Mail_Click(object sender, MouseButtonEventArgs e)
        {
            if (!Session.IsAuthenticated)
            {
                AppMessage.Warning(this, "Сначала войдите", "Сообщения");
                return;
            }
            MainFrame.Navigate(new PageMessages());
        }

        private void Avatar_Click(object sender, MouseButtonEventArgs e)
        {
            if (!Session.IsAuthenticated)
            {
                AppMessage.Warning(this, "Сначала войдите", "Профиль");
                return;
            }
            MainFrame.Navigate(new PageProfile());
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new LoginWindow();
            wnd.Show();
            Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (!AppMessage.Confirm(this, "Выйти из аккаунта?", "Выход"))
                return;

            if (_bannerTimer != null)
                _bannerTimer.Stop();

            Session.UserId = 0;
            Session.Phone = null;
            Session.RoleCode = null;
            Session.StudentId = 0;
            Session.CompanyId = 0;

            var login = new LoginWindow();
            login.Show();
            Close();
        }
    }
}