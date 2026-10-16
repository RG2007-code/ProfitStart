using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ProfitStart
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                Theme.ApplyToWindow(this, AppTheme.IsDark);
            };
        }

        private void Phone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[0-9\+\-\(\) ]");
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            var phone = PhoneBox.Text.Trim();
            var pass = PasswordBox.Visibility == Visibility.Visible
                ? PasswordBox.Password
                : PasswordPlain.Text;

            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(pass))
            {
                ErrorText.Text = "Заполните все поля";
                return;
            }

            if (!Regex.IsMatch(phone, @"^\+?[0-9]{10,12}$"))
            {
                ErrorText.Text = "Некорректный номер телефона";
                return;
            }

            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Phone == phone);

            if (user == null)
            {
                ErrorText.Text = "Пользователь не найден";
                return;
            }

            
            if (user.IsActive == false)
            {
                ShowBanMessage(user.Email);
                return;
            }

            if (user.PasswordHash != pass)
            {
                ErrorText.Text = "Неверный пароль";
                return;
            }

           
            if (BannedUsers.IsBanned(user.Email))
            {
                ShowBanMessage(user.Email);
                return;
            }

            
            Session.UserId = user.Id;
            Session.Phone = user.Phone;
            Session.RoleCode = user.Roles.Code;

            if (user.Roles.Code == "Student")
            {
                var st = Connection.connect.Students
                    .FirstOrDefault(s => s.UserId == user.Id);
                if (st != null) Session.StudentId = st.Id;
            }
            else if (user.Roles.Code == "Employer")
            {
                var cmp = Connection.connect.Companies
                    .FirstOrDefault(c => c.UserId == user.Id);
                if (cmp != null) Session.CompanyId = cmp.Id;
            }

            BanNotifier.ShowOnLogin(this, user.Email);

            var main = new MainWindow();
            main.Show();
            Close();
        }

       
        private void ShowBanMessage(string email)
        {
            var ban = Connection.connect.Bans
                .FirstOrDefault(b => b.Email == email);

            if (ban == null)
            {
                AppMessage.Error(this,
                    "Ваш аккаунт заблокирован.",
                    "Заблокировано");
                return;
            }

            string reason = string.IsNullOrEmpty(ban.Reason)
                ? "не указана"
                : ban.Reason;

            string until;
            if (ban.BanUntil.HasValue)
            {
                if (ban.BanUntil.Value < DateTime.Now)
                    until = "срок истёк — попробуйте войти снова";
                else
                    until = "до " + ban.BanUntil.Value.ToString("dd.MM.yyyy HH:mm");
            }
            else
            {
                until = "навсегда";
            }

            AppMessage.Error(this,
                "Ваш аккаунт заблокирован.\n\n"
                + "Причина: " + reason + "\n"
                + "Срок: " + until,
                "Заблокировано");
        }

        private void GoRegister_Click(object sender, RoutedEventArgs e)
        {
            var reg = new RegisterWindow();
            reg.Owner = this;
            reg.ShowDialog();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                PasswordPlain.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordPlain.Visibility = Visibility.Visible;

                PasswordPlain.Focus();
                PasswordPlain.CaretIndex = PasswordPlain.Text.Length;

                ToggleBtn.Content = "Скрыть";
                ToggleBtn.ToolTip = "Скрыть пароль";
            }
            else
            {
                PasswordBox.Password = PasswordPlain.Text;
                PasswordPlain.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;

                PasswordBox.Focus();

                ToggleBtn.Content = "Показать";
                ToggleBtn.ToolTip = "Показать пароль";
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new PasswordResetWindow();
            wnd.Owner = this;
            wnd.ShowDialog();
        }

        private void GoMainAsGuest_Click(object sender, RoutedEventArgs e)
        {
            Session.UserId = 0;
            Session.Phone = null;
            Session.RoleCode = null;
            Session.StudentId = 0;
            Session.CompanyId = 0;

            var main = new MainWindow();
            main.Show();
            Close();
        }
    }
}