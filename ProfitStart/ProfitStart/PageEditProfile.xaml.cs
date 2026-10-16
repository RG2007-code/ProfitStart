using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProfitStart
{
    public partial class PageEditProfile : Page
    {
        private byte[] _avatarData = null;
        private byte[] _passportData = null;
        private byte[] _diplomaData = null;
        private System.Collections.Generic.List<string> _selectedSkills
            = new System.Collections.Generic.List<string>();

        public PageEditProfile()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            if (!Session.IsAuthenticated) return;

            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Id == Session.UserId);
            if (user == null) return;

            EmailBox.Text = user.Email ?? "";
            PhoneBox.Text = user.Phone ?? "";

            _avatarData = user.AvatarData;

            var preview = Avatar.GetOrDefault(_avatarData, user.Email);
            if (preview != null)
                Image.LoadIntoEllipse(AvatarPreview, preview);

            if (Session.IsStudent)
            {
                StudentPanel.Visibility = Visibility.Visible;
                CompanyPanel.Visibility = Visibility.Collapsed;
                AdminPassBlock.Visibility = Visibility.Collapsed;
                CareerPassBlock.Visibility = Visibility.Collapsed;

                LoadStudent();
            }
            else if (Session.IsEmployer)
            {
                StudentPanel.Visibility = Visibility.Collapsed;
                CompanyPanel.Visibility = Visibility.Visible;
                AdminPassBlock.Visibility = Visibility.Collapsed;
                CareerPassBlock.Visibility = Visibility.Collapsed;

                LoadCompany();
            }
            else if (Session.IsAdmin)
            {
                StudentPanel.Visibility = Visibility.Collapsed;
                CompanyPanel.Visibility = Visibility.Collapsed;
                AdminPassBlock.Visibility = Visibility.Visible;
                CareerPassBlock.Visibility = Visibility.Collapsed;

                AdminPassBox.Text = user.AdminPassNumber ?? "—";
            }
            else if (Session.IsCareer)
            {
                StudentPanel.Visibility = Visibility.Collapsed;
                CompanyPanel.Visibility = Visibility.Collapsed;
                AdminPassBlock.Visibility = Visibility.Collapsed;
                CareerPassBlock.Visibility = Visibility.Visible;

                CareerPassBox.Text = user.AdminPassNumber ?? "—";
            }
        }

        private void LoadStudent()
        {
            var s = Connection.connect.Students
                .FirstOrDefault(x => x.Id == Session.StudentId);
            if (s == null) return;

            FullNameBox.Text = s.FullName ?? "";
            CourseBox.Text = s.Course.ToString();
            DirectionBox.Text = s.Direction ?? "";
            StudentCardBox.Text = s.StudentCardNumber ?? "";

            ResumeTextBox.Text = s.ResumeText ?? "";
            ResumeCounter.Text = ResumeTextBox.Text.Length + " / 3000";

            GraduatedToggle.IsChecked = s.IsGraduated == true;
            DiplomaPanel.Visibility = s.IsGraduated == true
                ? Visibility.Visible : Visibility.Collapsed;

            if (s.PassportData != null && s.PassportData.Length > 0)
            {
                _passportData = s.PassportData;
                Image.LoadIntoBorder(PassportPreview, _passportData);
                PassportHint.Visibility = Visibility.Collapsed;
            }

            if (s.DiplomaData != null && s.DiplomaData.Length > 0)
            {
                _diplomaData = s.DiplomaData;
                Image.LoadIntoBorder(DiplomaPreview, _diplomaData);
                DiplomaHint.Visibility = Visibility.Collapsed;
            }

            _selectedSkills = s.Skills.Select(sk => sk.Name).ToList();
            RenderSkills();
        }

        private void LoadCompany()
        {
            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == Session.CompanyId);
            if (c == null) return;

            CompanyNameBox.Text = c.Name ?? "";
            OgrnBox.Text = c.OGRN ?? "";
            InnBox.Text = c.INN ?? "";
            CompanyDirBox.Text = c.Direction ?? "";
            CompanyDescBox.Text = c.Description ?? "";
        }

        private void RenderSkills()
        {
            SkillsPanel.Children.Clear();

            foreach (var name in _selectedSkills)
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
                    _selectedSkills.Remove(nm);
                    RenderSkills();
                };
                SkillsPanel.Children.Add(b);
            }
        }

        private void SkillInput_GotFocus(object sender, RoutedEventArgs e)
        {
            ShowSkillSuggestions(SkillInput.Text);
        }

        private void SkillInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowSkillSuggestions(SkillInput.Text);
        }

        private void SkillInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddSkillFromInput();
            }
            else if (e.Key == Key.Escape)
            {
                SkillsPopup.IsOpen = false;
            }
        }

        private void AddSkill_Click(object sender, RoutedEventArgs e)
        {
            AddSkillFromInput();
        }

        private void AddSkillFromInput()
        {
            var name = SkillInput.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;
            if (_selectedSkills.Contains(name))
            {
                SkillInput.Text = "";
                SkillsPopup.IsOpen = false;
                return;
            }

            _selectedSkills.Add(name);
            RenderSkills();

            SkillInput.Text = "";
            SkillsPopup.IsOpen = false;
        }

        private void ShowSkillSuggestions(string query)
        {
            query = (query ?? "").Trim().ToLower();

            var all = Connection.connect.Skills
                .OrderBy(s => s.Name)
                .Select(s => s.Name)
                .ToList();

            var filtered = all
                .Where(s => string.IsNullOrEmpty(query)
                         || s.ToLower().Contains(query))
                .Where(s => !_selectedSkills.Contains(s))
                .Take(20)
                .ToList();

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

            if (!_selectedSkills.Contains(name))
            {
                _selectedSkills.Add(name);
                RenderSkills();
            }

            SkillInput.Text = "";
            SkillsPopup.IsOpen = false;

            Dispatcher.InvokeAsync(() => SkillInput.Focus(),
                System.Windows.Threading.DispatcherPriority.Background);

            e.Handled = true;
        }

        private void Avatar_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Картинки|*.png;*.jpg;*.jpeg;*.bmp"
            };
            if (dlg.ShowDialog() != true) return;

            _avatarData = Image.FileToBytes(dlg.FileName);
            Image.LoadIntoEllipse(AvatarPreview, _avatarData);
        }

        private void UploadPassport_Click(object sender, MouseButtonEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Картинки|*.png;*.jpg;*.jpeg;*.bmp"
            };
            if (dlg.ShowDialog() != true) return;

            _passportData = Image.FileToBytes(dlg.FileName);
            Image.LoadIntoBorder(PassportPreview, _passportData);
            PassportHint.Visibility = Visibility.Collapsed;
        }

        private void UploadDiploma_Click(object sender, MouseButtonEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Картинки|*.png;*.jpg;*.jpeg;*.bmp"
            };
            if (dlg.ShowDialog() != true) return;

            _diplomaData = Image.FileToBytes(dlg.FileName);
            Image.LoadIntoBorder(DiplomaPreview, _diplomaData);
            DiplomaHint.Visibility = Visibility.Collapsed;
        }

        private void Graduated_Changed(object sender, RoutedEventArgs e)
        {
            if (DiplomaPanel == null) return;

            DiplomaPanel.Visibility = GraduatedToggle.IsChecked == true
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DigitsOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void Resume_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ResumeCounter == null) return;
            ResumeCounter.Text = ResumeTextBox.Text.Length + " / 3000";
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageProfile());
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (!Session.IsAuthenticated)
            {
                ErrorText.Text = "Вы не авторизованы";
                return;
            }

            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Id == Session.UserId);
            if (user == null) return;

            var newEmail = EmailBox.Text.Trim();
            var newPhone = PhoneBox.Text.Trim();

            if (string.IsNullOrEmpty(newEmail) || string.IsNullOrEmpty(newPhone))
            {
                ErrorText.Text = "Email и телефон обязательны";
                return;
            }

            if (Connection.connect.Users.Any(u =>
                u.Email == newEmail && u.Id != user.Id))
            {
                ErrorText.Text = "Этот email уже используется";
                return;
            }

            if (Connection.connect.Users.Any(u =>
                u.Phone == newPhone && u.Id != user.Id))
            {
                ErrorText.Text = "Этот телефон уже используется";
                return;
            }

            user.Email = newEmail;
            user.Phone = newPhone;

            if (_avatarData != null)
                user.AvatarData = _avatarData;

            if (Session.IsStudent)
            {
                var s = Connection.connect.Students
                    .FirstOrDefault(x => x.Id == Session.StudentId);
                if (s != null)
                {
                    s.FullName = FullNameBox.Text.Trim();

                    int course = 1;
                    int.TryParse(CourseBox.Text.Trim(), out course);
                    s.Course = course;

                    s.Direction = DirectionBox.Text.Trim();
                    s.ResumeText = ResumeTextBox.Text.Trim();

                    if (_passportData != null) s.PassportData = _passportData;
                    if (_diplomaData != null) s.DiplomaData = _diplomaData;

                    s.IsGraduated = GraduatedToggle.IsChecked == true;

                    s.Skills.Clear();
                    foreach (var name in _selectedSkills)
                    {
                        var skill = Connection.connect.Skills
                            .FirstOrDefault(x => x.Name == name);
                        if (skill == null)
                        {
                            skill = new Skills { Name = name };
                            Connection.connect.Skills.Add(skill);
                            Connection.connect.SaveChanges();
                        }
                        s.Skills.Add(skill);
                    }
                }
            }
            else if (Session.IsEmployer)
            {
                var c = Connection.connect.Companies
                    .FirstOrDefault(x => x.Id == Session.CompanyId);
                if (c != null)
                {
                    c.Name = CompanyNameBox.Text.Trim();
                    c.Direction = CompanyDirBox.Text.Trim();
                    c.Description = CompanyDescBox.Text.Trim();

                    c.IsApproved = false;
                    c.IsModerated = true;
                    c.RejectionReason = null;

                    Connection.connect.SaveChanges();

                    Notify.ToAdmin(
                        "Компания изменена",
                        "Изменён профиль компании: " + c.Name,
                        "modcompany",
                        c.Id);
                }
            }

            Connection.connect.SaveChanges();

            var main = Window.GetWindow(this) as MainWindow;
            main?.RefreshAvatar();

            AppMessage.Success(Window.GetWindow(this), "Профиль сохранён");
            NavigationService.Navigate(new PageProfile());
        }
    }
}