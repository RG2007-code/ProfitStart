using System.Windows;

namespace ProfitStart
{
    public partial class MessageDialogWindow : Window
    {
        public string Message { get; private set; } = "";

        public MessageDialogWindow(string title)
        {
            InitializeComponent();
            TitleText.Text = title;

            
            Loaded += (s, e) => Theme.ApplyToWindow(this, AppTheme.IsDark);
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            Message = "";
            DialogResult = true;
            Close();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Message = MsgBox.Text.Trim();
            DialogResult = true;
            Close();
        }
    }
}