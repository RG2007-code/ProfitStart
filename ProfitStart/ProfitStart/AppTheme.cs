using System;

namespace ProfitStart
{
    public static class AppTheme
    {
        public static event Action ThemeChanged;

        private static bool _isDark = false;
        public static bool IsDark
        {
            get => _isDark;
            set
            {
                if (_isDark == value) return;
                _isDark = value;
                ThemeChanged?.Invoke();
            }
        }
    }
}