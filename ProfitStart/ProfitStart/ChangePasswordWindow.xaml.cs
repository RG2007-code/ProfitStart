using System;
using System.Linq;
using System.Windows;

namespace ProfitStart
{
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            InitializeComponent();

            Loaded += (s, e) => Theme.ApplyToWindow(this, AppTheme.IsDark);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (!Session.IsAuthenticated)
            {
                ErrorText.Text = "Вы не авторизованы";
                return;
            }

            var current = CurrentPasswordBox.Password;
            var newPass = NewPasswordBox.Password;
            var confirm = ConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(current) ||
                string.IsNullOrEmpty(newPass) ||
                string.IsNullOrEmpty(confirm))
            {
                ErrorText.Text = "Заполните все поля";
                return;
            }

            if (newPass.Length < 6)
            {
                ErrorText.Text = "Новый пароль — не короче 6 символов";
                return;
            }

            if (newPass != confirm)
            {
                ErrorText.Text = "Пароли не совпадают";
                return;
            }

            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Id == Session.UserId);
            if (user == null)
            {
                ErrorText.Text = "Пользователь не найден";
                return;
            }

            if (user.PasswordHash != current)
            {
                ErrorText.Text = "Текущий пароль указан неверно";
                return;
            }

           
            user.PasswordHash = newPass;
            Connection.connect.SaveChanges();

            AppMessage.Success(this, "Пароль успешно изменён");
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}