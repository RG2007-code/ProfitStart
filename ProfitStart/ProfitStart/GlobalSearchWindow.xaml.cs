using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class GlobalSearchWindow : Window
    {
        public GlobalSearchWindow(string query,
            List<Vacancies> vacancies,
            List<Companies> companies,
            List<Practices> practices)
        {
            InitializeComponent();

            TitleText.Text = "Результаты: \"" + query + "\"";
            ResultsPanel.Children.Clear();

            if (vacancies.Any())
            {
                AddHeader("Вакансии");
                foreach (var v in vacancies)
                {
                    byte[] avatar = v.AvatarData;
                    if ((avatar == null || avatar.Length == 0) && v.CompanyId.HasValue)
                    {
                        var company = Connection.connect.Companies
                            .FirstOrDefault(c => c.Id == v.CompanyId.Value);
                        if (company != null)
                        {
                            avatar = company.AvatarData;
                            if ((avatar == null || avatar.Length == 0)
                                && company.UserId.HasValue)
                            {
                                var owner = Connection.connect.Users
                                    .FirstOrDefault(u => u.Id == company.UserId.Value);
                                if (owner != null) avatar = owner.AvatarData;
                            }
                        }
                    }

                    AddCard(avatar,
                            v.Title,
                            (v.Companies != null ? v.Companies.Name : "—")
                            + " · " + v.Type,
                            () => OpenPage(new PageVacancyCard(v.Id)));
                }
            }

            if (companies.Any())
            {
                AddHeader("Компании");
                foreach (var c in companies)
                {
                    byte[] avatar = c.AvatarData;
                    if ((avatar == null || avatar.Length == 0) && c.UserId.HasValue)
                    {
                        var owner = Connection.connect.Users
                            .FirstOrDefault(u => u.Id == c.UserId.Value);
                        if (owner != null) avatar = owner.AvatarData;
                    }

                    AddCard(avatar, c.Name, c.Direction,
                            () => OpenPage(new PageCompanyCard(c.Id)));
                }
            }

            if (practices.Any())
            {
                AddHeader("Практики");
                foreach (var p in practices)
                {
                    AddCard(p.AvatarData, p.Title,
                            p.Direction + " · " + p.Duration,
                            () => OpenPage(new PagePractice()));
                }
            }

            if (!vacancies.Any() && !companies.Any() && !practices.Any())
            {
                ResultsPanel.Children.Add(new TextBlock
                {
                    Text = "Ничего не найдено",
                    Foreground = Brushes.Gray,
                    FontSize = 14,
                    Margin = new Thickness(0, 20, 0, 0)
                });
            }

            
            Loaded += (s, e) => Theme.ApplyToWindow(this, AppTheme.IsDark);
        }

        private void AddHeader(string text)
        {
            ResultsPanel.Children.Add(new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 16, 0, 8)
            });
        }

        private void AddCard(byte[] avatar, string title, string subtitle, Action onClick)
        {
            var border = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 10),
                Cursor = Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var avatarGrid = new Grid { Width = 40, Height = 40 };
            avatarGrid.Children.Add(new System.Windows.Shapes.Ellipse
            {
                Width = 40,
                Height = 40,
                Fill = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#EEF2FF"))
            });

            if (avatar != null && avatar.Length > 0)
            {
                var ellipse = new System.Windows.Shapes.Ellipse
                {
                    Width = 40,
                    Height = 40
                };
                Image.LoadIntoEllipse(ellipse, avatar);
                avatarGrid.Children.Add(ellipse);
            }

            Grid.SetColumn(avatarGrid, 0);
            grid.Children.Add(avatarGrid);

            var sp = new StackPanel { Margin = new Thickness(16, 0, 0, 0) };
            sp.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });
            sp.Children.Add(new TextBlock
            {
                Text = subtitle,
                Foreground = Brushes.Gray,
                FontSize = 12,
                Margin = new Thickness(0, 4, 0, 0)
            });

            Grid.SetColumn(sp, 1);
            grid.Children.Add(sp);

            border.Child = grid;
            border.MouseLeftButtonUp += (s, e) => { onClick(); Close(); };
            ResultsPanel.Children.Add(border);
        }

        private void OpenPage(Page page)
        {
            var main = Owner as MainWindow;
            main?.OpenPage(page);
        }
    }
}