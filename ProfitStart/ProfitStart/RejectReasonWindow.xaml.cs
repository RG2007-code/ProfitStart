using System.Windows;

namespace ProfitStart
{
    public partial class RejectReasonWindow : Window
    {
        public string Reason { get; private set; } = "";

        public RejectReasonWindow(string title)
        {
            InitializeComponent();
            TitleText.Text = title;

            
            Loaded += (s, e) => Theme.ApplyToWindow(this, AppTheme.IsDark);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var text = ReasonBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                ErrorText.Text = "Введите причину";
                return;
            }

            Reason = text;
            DialogResult = true;
            Close();
        }
    }
}