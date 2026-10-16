using System.Windows;

namespace ProfitStart
{
    public partial class DisputeWindow : Window
    {
        public string Message { get; private set; }

        public DisputeWindow(string initialText = "")
        {
            InitializeComponent();
            MessageBox.Text = initialText ?? "";
            MessageBox.Focus();
            MessageBox.CaretIndex = MessageBox.Text.Length;

            
            Loaded += (s, e) => Theme.ApplyToWindow(this, AppTheme.IsDark);
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            var text = MessageBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                AppMessage.Warning(this, "Опишите причину оспаривания");
                return;
            }

            Message = text;
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