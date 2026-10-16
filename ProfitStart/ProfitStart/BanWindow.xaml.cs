using System;
using System.Windows;
using System.Windows.Controls;

namespace ProfitStart
{
    public partial class BanWindow : Window
    {
        private int _userId;

        public BanWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            ReasonCombo.ItemsSource = BannedUsers.StandardReasons;

            Loaded += (s, e) =>
            {
                Theme.ApplyToWindow(this, AppTheme.IsDark);
            };
        }

        private void ReasonChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomReasonBlock == null) return;

            var selected = ReasonCombo.SelectedItem?.ToString();
            CustomReasonBlock.Visibility = selected == "Другое (введите вручную)"
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Ban_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            var reasonType = ReasonCombo.SelectedItem?.ToString();
            string reason = reasonType;

            if (reasonType == "Другое (введите вручную)")
            {
                reason = CustomReasonBox.Text.Trim();
                if (string.IsNullOrEmpty(reason))
                {
                    ErrorText.Text = "Введите причину";
                    return;
                }
            }

            DateTime? until = null;
            var duration = (DurationCombo.SelectedItem as ComboBoxItem)
                ?.Content?.ToString();

            switch (duration)
            {
                case "1 день": until = DateTime.Now.AddDays(1); break;
                case "7 дней": until = DateTime.Now.AddDays(7); break;
                case "30 дней": until = DateTime.Now.AddDays(30); break;
                case "1 год": until = DateTime.Now.AddYears(1); break;
                case "Навсегда": until = null; break;
            }

            BannedUsers.Ban(_userId, reason, reasonType, until);

            DialogResult = true;
            Close();
        }
    }
}