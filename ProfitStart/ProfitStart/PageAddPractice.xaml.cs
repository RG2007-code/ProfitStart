using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageAddPractice : Page
    {
        private byte[] _avatarData = null;
        private readonly List<int> _selectedSkillIds = new List<int>();

        public PageAddPractice()
        {
            InitializeComponent();
            SkillPlaceholder.Visibility = Visibility.Visible;
            Loaded += (s, e) => TitleBox.Focus();
        }

        private void SkillSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            ShowDropdown(SkillSearchBox.Text.Trim());
        }

        private void SkillSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SkillPlaceholder.Visibility =
                string.IsNullOrEmpty(SkillSearchBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            ShowDropdown(SkillSearchBox.Text.Trim());
        }

        private void SkillSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddSkill_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                SkillDropdown.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        private void ShowDropdown(string q)
        {
            var query = Connection.connect.Skills.AsQueryable();

            if (!string.IsNullOrEmpty(q))
                query = query.Where(s => s.Name.Contains(q));

            var found = query
                .Where(s => !_selectedSkillIds.Contains(s.Id))
                .OrderBy(s => s.Name)
                .Take(50)
                .Select(s => new { s.Id, s.Name })
                .ToList();

            if (found.Count == 0)
            {
                SkillDropdown.Visibility = Visibility.Collapsed;
                return;
            }

            SkillDropdownList.ItemsSource = found;
            SkillDropdown.Visibility = Visibility.Visible;
        }

        private void SkillOption_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            if (!(border.Tag is int id)) return;

            var skill = Connection.connect.Skills
                .FirstOrDefault(s => s.Id == id);
            if (skill == null) return;

            if (_selectedSkillIds.Contains(id)) return;

            _selectedSkillIds.Add(id);
            AddSkillChip(skill.Id, skill.Name);

            SkillSearchBox.Text = string.Empty;
            SkillDropdown.Visibility = Visibility.Collapsed;

            e.Handled = true;
            SkillSearchBox.Focus();
        }

        private void AddSkill_Click(object sender, RoutedEventArgs e)
        {
            var q = SkillSearchBox.Text.Trim();
            if (string.IsNullOrEmpty(q)) return;

            var exact = Connection.connect.Skills
                .FirstOrDefault(s => s.Name == q);

            if (exact != null && !_selectedSkillIds.Contains(exact.Id))
            {
                _selectedSkillIds.Add(exact.Id);
                AddSkillChip(exact.Id, exact.Name);
                SkillSearchBox.Text = string.Empty;
                SkillDropdown.Visibility = Visibility.Collapsed;
                SkillSearchBox.Focus();
                return;
            }

            var first = Connection.connect.Skills
                .Where(s => s.Name.Contains(q))
                .Where(s => !_selectedSkillIds.Contains(s.Id))
                .OrderBy(s => s.Name)
                .FirstOrDefault();

            if (first != null)
            {
                _selectedSkillIds.Add(first.Id);
                AddSkillChip(first.Id, first.Name);
                SkillSearchBox.Text = string.Empty;
                SkillDropdown.Visibility = Visibility.Collapsed;
                SkillSearchBox.Focus();
            }
        }

        private void AddSkillChip(int skillId, string skillName)
        {
            var chip = new Border
            {
                Background = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#DCFCE7")),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(14, 6, 10, 6),
                Margin = new Thickness(0, 0, 8, 8),
                Tag = skillId
            };

            var sp = new StackPanel { Orientation = Orientation.Horizontal };

            sp.Children.Add(new TextBlock
            {
                Text = skillName,
                Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#16A34A")),
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            });

            var removeBtn = new TextBlock
            {
                Text = " ✕",
                Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#16A34A")),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(6, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };

            removeBtn.MouseLeftButtonUp += (s, e) =>
            {
                _selectedSkillIds.Remove(skillId);
                SelectedSkillsPanel.Children.Remove(chip);
            };

            sp.Children.Add(removeBtn);
            chip.Child = sp;

            SelectedSkillsPanel.Children.Add(chip);
        }

        private void UploadAvatar_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Картинки|*.png;*.jpg;*.jpeg;*.bmp"
            };
            if (dlg.ShowDialog() != true) return;

            _avatarData = Image.FileToBytes(dlg.FileName);
            Image.LoadIntoBorder(PreviewAvatar, _avatarData);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageManagePractice());
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            var title = TitleBox.Text.Trim();
            var dir = DirectionBox.Text.Trim();
            var dur = DurationBox.Text.Trim();
            var desc = DescBox.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                ErrorText.Text = "Заполните название";
                return;
            }

            if (_selectedSkillIds.Count == 0)
            {
                ErrorText.Text = "Добавьте хотя бы один навык";
                return;
            }

            var p = new Practices
            {
                Title = title,
                Direction = dir,
                Duration = dur,
                Description = desc,
                AvatarData = _avatarData,
                CompanyId = Session.CompanyId != 0
                            ? Session.CompanyId : (int?)null,
                IsApproved = false,
                IsModerated = true,
                CreatedAt = DateTime.Now
            };

            Connection.connect.Practices.Add(p);
            Connection.connect.SaveChanges();

            Skill.SavePracticeSkills(p.Id, _selectedSkillIds);
            Connection.connect.SaveChanges();

            Notify.ToAdmin("Новая практика на модерации",
                "Добавлена практика: " + p.Title,
                "modpractice", p.Id);

            AppMessage.Info(Window.GetWindow(this),
                "Практика отправлена на модерацию");

            NavigationService.Navigate(new PageManagePractice());
        }
    }
}