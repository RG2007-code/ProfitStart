using System.Linq;
using System.Windows;

namespace ProfitStart
{
    public partial class AdminMessageWindow : Window
    {
        private int _userId;

        public AdminMessageWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;

            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Id == userId);
            if (user != null)
                TitleText.Text = "Написать: " + user.Email;

            Loaded += (s, e) =>
            {
                Theme.ApplyToWindow(this, AppTheme.IsDark);
            };
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            var text = MsgBox.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            Notify.ToStudent(_userId,
                "Сообщение от администрации",
                text,
                "", 0);

            AppMessage.Success(this, "Сообщение отправлено");
            Close();
        }
    }
}