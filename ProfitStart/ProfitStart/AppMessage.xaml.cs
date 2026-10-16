using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class AppMessage : Window
    {
        public bool Result { get; private set; } = false;

        public AppMessage(string message,
                          string title = "Информация",
                          AppMessageType type = AppMessageType.Info,
                          bool showCancel = false)
        {
            InitializeComponent();

            TitleText.Text = title;
            MessageText.Text = message;

            ApplyType(type);

            if (showCancel)
            {
                CancelButton.Visibility = Visibility.Visible;
                OkButton.Content = "Да";
                CancelButton.Content = "Нет";
            }

            
            Loaded += (s, e) => Theme.ApplyToWindow(this, AppTheme.IsDark);

            
            SourceInitialized += (s, e) => RemoveWindowBorder();
        }

        

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_NCRENDERING_POLICY = 2;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMNCRP_DISABLED = 1;
        private const int DWMWCP_DONOTROUND = 1;

        private void RemoveWindowBorder()
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd == IntPtr.Zero) return;

                
                int policy = DWMNCRP_DISABLED;
                DwmSetWindowAttribute(hwnd, DWMWA_NCRENDERING_POLICY,
                                      ref policy, sizeof(int));

                
                int corner = DWMWCP_DONOTROUND;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE,
                                      ref corner, sizeof(int));
            }
            catch { }
        }

        

        private void ApplyType(AppMessageType type)
        {
            bool dark = AppTheme.IsDark;

            switch (type)
            {
                case AppMessageType.Success:
                    IconText.Text = "✓";
                    IconText.Foreground = Brush("#22C55E");
                    IconBorder.Background = Brush(dark ? "#14532D" : "#DCFCE7");
                    break;

                case AppMessageType.Warning:
                    IconText.Text = "!";
                    IconText.Foreground = Brush("#F59E0B");
                    IconBorder.Background = Brush(dark ? "#78350F" : "#FEF3C7");
                    break;

                case AppMessageType.Error:
                    IconText.Text = "✕";
                    IconText.Foreground = Brush("#EF4444");
                    IconBorder.Background = Brush(dark ? "#7F1D1D" : "#FEE2E2");
                    break;

                default:
                    IconText.Text = "i";
                    IconText.Foreground = Brush("#818CF8");
                    IconBorder.Background = Brush(dark ? "#312E81" : "#EEF2FF");
                    break;
            }
        }

        private static SolidColorBrush Brush(string hex)
        {
            return new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(hex));
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }

        public static void Info(Window owner, string message,
                                string title = "Информация")
        {
            var wnd = new AppMessage(message, title, AppMessageType.Info);
            if (owner != null) wnd.Owner = owner;
            wnd.ShowDialog();
        }

        public static void Success(Window owner, string message,
                                   string title = "Готово")
        {
            var wnd = new AppMessage(message, title, AppMessageType.Success);
            if (owner != null) wnd.Owner = owner;
            wnd.ShowDialog();
        }

        public static void Warning(Window owner, string message,
                                   string title = "Внимание")
        {
            var wnd = new AppMessage(message, title, AppMessageType.Warning);
            if (owner != null) wnd.Owner = owner;
            wnd.ShowDialog();
        }

        public static void Error(Window owner, string message,
                                 string title = "Ошибка")
        {
            var wnd = new AppMessage(message, title, AppMessageType.Error);
            if (owner != null) wnd.Owner = owner;
            wnd.ShowDialog();
        }

        public static bool Confirm(Window owner, string message,
                                   string title = "Подтверждение")
        {
            var wnd = new AppMessage(message, title,
                                     AppMessageType.Warning, showCancel: true);
            if (owner != null) wnd.Owner = owner;
            wnd.ShowDialog();
            return wnd.Result;
        }
    }

    public enum AppMessageType
    {
        Info,
        Success,
        Warning,
        Error
    }
}