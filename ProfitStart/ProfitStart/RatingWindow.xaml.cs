using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class RatingWindow : Window
    {
        public decimal Rating { get; private set; }

        public RatingWindow(decimal current)
        {
            InitializeComponent();
            Rating = current;
            UpdateStars((int)current);

            Loaded += (s, e) => Theme.ApplyToWindow(this, AppTheme.IsDark);
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            
            if (Session.IsAdmin)
            {
                AppMessage.Warning(this,
                    "Рейтинг может ставить только студент",
                    "Недоступно");
                return;
            }

            int stars = int.Parse(((Button)sender).Tag.ToString());
            UpdateStars(stars);
        }

        private void UpdateStars(int count)
        {
            var gray = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#D1D5DB"));
            var yellow = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#F59E0B"));

            var stars = new[] { S1, S2, S3, S4, S5 };
            for (int i = 0; i < stars.Length; i++)
                stars[i].Foreground = i < count ? yellow : gray;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (Session.IsAdmin)
            {
                AppMessage.Warning(this,
                    "Рейтинг может ставить только студент",
                    "Недоступно");
                DialogResult = false;
                Close();
                return;
            }

            var yellow = (Color)ColorConverter.ConvertFromString("#F59E0B");
            int count = 0;
            var stars = new[] { S1, S2, S3, S4, S5 };
            foreach (var s in stars)
                if (s.Foreground is SolidColorBrush b && b.Color == yellow)
                    count++;

            Rating = count;
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