using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ProfitStart
{
    public partial class PasswordResetWindow : Window
    {
        public PasswordResetWindow()
        {
            InitializeComponent();
            PhoneBox.Focus();
            PhoneBox.CaretIndex = PhoneBox.Text.Length;

            
            Loaded += (s, e) => Theme.ApplyToWindow(this, AppTheme.IsDark);
        }

        private void Phone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[0-9\+\-\(\) ]");
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            MessageText.Text = "";
            ResultPanel.Visibility = Visibility.Collapsed;

            string phone = PhoneBox.Text.Trim();

            if (!PasswordReset.IsValidPhone(phone))
            {
                MessageText.Foreground = System.Windows.Media.Brushes.Red;
                MessageText.Text = "Некорректный номер телефона";
                return;
            }

            var (ok, result) = PasswordReset.ResetByPhone(phone);

            if (!ok)
            {
                MessageText.Foreground = System.Windows.Media.Brushes.Red;
                MessageText.Text = result;
                return;
            }

            TempPasswordText.Text = result;
            ResultPanel.Visibility = Visibility.Visible;

            MessageText.Foreground = System.Windows.Media.Brushes.Green;
            MessageText.Text = "Пароль успешно сброшен";

            ResetBtn.IsEnabled = false;
            PhoneBox.IsEnabled = false;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TempPasswordText.Text)) return;

            try
            {
                Clipboard.SetText(TempPasswordText.Text);
                AppMessage.Success(this, "Пароль скопирован в буфер обмена");
            }
            catch
            {
                AppMessage.Warning(this,
                    "Не удалось скопировать. Запишите пароль вручную.");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}