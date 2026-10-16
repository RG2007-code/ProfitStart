using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageAddVacancy : Page
    {
        private byte[] _avatarData = null;
        private List<string> _selectedSkillNames = new List<string>();
        private List<string> _allSkills = new List<string>();

        public PageAddVacancy()
        {
            InitializeComponent();
            LoadSkills();
            PreviewMouseDown += Page_PreviewMouseDown;
        }

        private void LoadSkills()
        {
            _allSkills = Connection.connect.Skills
                .OrderBy(s => s.Name)
                .Select(s => s.Name)
                .ToList();
        }

        private void Page_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SkillsPopup == null || !SkillsPopup.IsOpen) return;

            var pos = e.GetPosition(SkillsPopup.Child);
            bool insidePopup = pos.X >= 0 && pos.Y >= 0
                            && pos.X <= SkillsPopup.Child.RenderSize.Width
                            && pos.Y <= SkillsPopup.Child.RenderSize.Height;

            var posInput = e.GetPosition(SkillInput);
            bool insideInput = posInput.X >= 0 && posInput.Y >= 0
                            && posInput.X <= SkillInput.ActualWidth
                            && posInput.Y <= SkillInput.ActualHeight;

            if (!insidePopup && !insideInput)
                SkillsPopup.IsOpen = false;
        }

        
        private void TypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SalaryUnit == null || TypeBox == null) return;

            var type = ((ComboBoxItem)TypeBox.SelectedItem)?.Content?.ToString();

            if (type == "Стажировка" || type == "Практика")
                SalaryUnit.SelectedIndex = 0;   
            else
                SalaryUnit.SelectedIndex = 1;   
        }

        
        private void SkillInput_GotFocus(object sender, RoutedEventArgs e)
        {
            ShowSuggestions(SkillInput.Text);
        }

        private void SkillInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowSuggestions(SkillInput.Text);
        }

        private void ShowSuggestions(string query)
        {
            query = (query ?? "").Trim().ToLower();

            var filtered = _allSkills
                .Where(s => string.IsNullOrEmpty(query)
                         || s.ToLower().Contains(query))
                .Take(20)
                .ToList();

            bool hasExact = _allSkills.Any(s => s.ToLower() == query);
            if (!string.IsNullOrEmpty(query) && !hasExact)
                filtered.Insert(0, query);

            if (filtered.Count == 0)
            {
                SkillsPopup.IsOpen = false;
                return;
            }

            SkillsSuggestList.ItemsSource = filtered;
            SkillsPopup.IsOpen = true;
        }

        private void SkillSuggest_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            var name = border.Tag as string;
            if (string.IsNullOrEmpty(name)) return;

            AddSkill(name);
            SkillInput.Text = "";
            SkillsPopup.IsOpen = false;

            Dispatcher.InvokeAsync(() => SkillInput.Focus(),
                System.Windows.Threading.DispatcherPriority.Background);

            e.Handled = true;
        }

        private void SkillInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var name = SkillInput.Text.Trim();
                if (string.IsNullOrEmpty(name)) return;

                var exact = _allSkills.FirstOrDefault(s =>
                    s.Equals(name, System.StringComparison.OrdinalIgnoreCase));

                AddSkill(exact ?? name);
                SkillInput.Text = "";
                SkillsPopup.IsOpen = false;
            }
            else if (e.Key == Key.Escape)
            {
                SkillsPopup.IsOpen = false;
            }
        }

        private void AddSkill_Click(object sender, RoutedEventArgs e)
        {
            var name = SkillInput.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;

            var exact = _allSkills.FirstOrDefault(s =>
                s.Equals(name, System.StringComparison.OrdinalIgnoreCase));

            AddSkill(exact ?? name);
            SkillInput.Text = "";
            SkillsPopup.IsOpen = false;
        }

        private void AddSkill(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (_selectedSkillNames.Any(s => s.Equals(name,
                System.StringComparison.OrdinalIgnoreCase))) return;

            _selectedSkillNames.Add(name);

            if (!_allSkills.Any(s => s.Equals(name,
                System.StringComparison.OrdinalIgnoreCase)))
                _allSkills.Add(name);

            RenderSkills();
        }

        private void RenderSkills()
        {
            SkillsPanel.Children.Clear();

            foreach (var name in _selectedSkillNames)
            {
                var b = new Border
                {
                    Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#EEF2FF")),
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(14, 6, 14, 6),
                    Margin = new Thickness(0, 0, 8, 8),
                    Cursor = Cursors.Hand,
                    Tag = name
                };
                b.Child = new TextBlock
                {
                    Text = name + "  ✕",
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#5B5BF5")),
                    FontSize = 12
                };
                b.MouseLeftButtonUp += (s, e) =>
                {
                    var border = (Border)s;
                    var nm = border.Tag.ToString();
                    _selectedSkillNames.Remove(nm);
                    RenderSkills();
                };
                SkillsPanel.Children.Add(b);
            }
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
            NavigationService.Navigate(new PageMyVacancies());
        }

       
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (Session.CompanyId == 0)
            {
                ErrorText.Text = "Вы не привязаны к компании";
                return;
            }

            var title = TitleBox.Text.Trim();
            var type = ((ComboBoxItem)TypeBox.SelectedItem).Content.ToString();
            var desc = DescBox.Text.Trim();
            var req = ReqBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(desc))
            {
                ErrorText.Text = "Заполните название и описание";
                return;
            }

            if (_selectedSkillNames.Count == 0)
            {
                ErrorText.Text = "Выберите хотя бы один навык";
                return;
            }

            
            var salaryNumber = SalaryBox.Text.Trim();
            var salaryUnitDisplay =
                ((ComboBoxItem)SalaryUnit.SelectedItem).Content.ToString();
            var salaryUnitCode = Payment.GetCode(salaryUnitDisplay);

            var v = new Vacancies
            {
                CompanyId = Session.CompanyId,
                Title = title,
                Type = type,
                Description = desc,
                Requirements = req,
                Salary = salaryNumber,          
                PaymentUnit = salaryUnitCode,   
                AvatarData = _avatarData,
                MatchPercent = 0,
                IsApproved = false,
                IsModerated = true,
                CreatedAt = DateTime.Now
            };
            Connection.connect.Vacancies.Add(v);
            Connection.connect.SaveChanges();

            
            var skillIds = new List<int>();
            foreach (var name in _selectedSkillNames)
            {
                var skill = Connection.connect.Skills
                    .FirstOrDefault(x => x.Name == name);
                if (skill == null)
                {
                    skill = new Skills { Name = name };
                    Connection.connect.Skills.Add(skill);
                    Connection.connect.SaveChanges();
                }
                skillIds.Add(skill.Id);
            }

            Skill.SaveVacancySkills(v.Id, skillIds);
            Connection.connect.SaveChanges();

            Notify.ToAdmin("Новая вакансия на модерации",
                "Компания добавила вакансию: " + v.Title,
                "modvacancies", v.Id);

            AppMessage.Info(Window.GetWindow(this),
                "Вакансия отправлена на модерацию");
            NavigationService.Navigate(new PageMyVacancies());
        }
    }
}