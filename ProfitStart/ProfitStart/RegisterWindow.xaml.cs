using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProfitStart
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                Theme.ApplyToWindow(this, AppTheme.IsDark);
            };
        }

        private void DigitsOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void PhoneInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[0-9\+\-\(\) ]");
        }

        private void Role_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (RoleBox == null || RoleBox.SelectedItem == null) return;
            if (StudentPanel == null || CompanyPanel == null ||
                AdminPanel == null || NameLabel == null) return;

            var tag = ((ComboBoxItem)RoleBox.SelectedItem).Tag.ToString();

            StudentPanel.Visibility = tag == "Student" ? Visibility.Visible : Visibility.Collapsed;
            CompanyPanel.Visibility = tag == "Employer" ? Visibility.Visible : Visibility.Collapsed;
            AdminPanel.Visibility = tag == "Admin" ? Visibility.Visible : Visibility.Collapsed;

            NameLabel.Text = tag == "Employer" ? "Название компании" : "ФИО";
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                PasswordPlain.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordPlain.Visibility = Visibility.Visible;

                PasswordPlain.Focus();
                PasswordPlain.CaretIndex = PasswordPlain.Text.Length;

                ToggleBtn1.Content = "Скрыть";
                ToggleBtn1.ToolTip = "Скрыть пароль";
            }
            else
            {
                PasswordBox.Password = PasswordPlain.Text;
                PasswordPlain.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;

                PasswordBox.Focus();

                ToggleBtn1.Content = "Показать";
                ToggleBtn1.ToolTip = "Показать пароль";
            }
        }

        private void TogglePassword2_Click(object sender, RoutedEventArgs e)
        {
            if (Password2Box.Visibility == Visibility.Visible)
            {
                Password2Plain.Text = Password2Box.Password;
                Password2Box.Visibility = Visibility.Collapsed;
                Password2Plain.Visibility = Visibility.Visible;

                Password2Plain.Focus();
                Password2Plain.CaretIndex = Password2Plain.Text.Length;

                ToggleBtn2.Content = "Скрыть";
                ToggleBtn2.ToolTip = "Скрыть пароль";
            }
            else
            {
                Password2Box.Password = Password2Plain.Text;
                Password2Plain.Visibility = Visibility.Collapsed;
                Password2Box.Visibility = Visibility.Visible;

                Password2Box.Focus();

                ToggleBtn2.Content = "Показать";
                ToggleBtn2.ToolTip = "Показать пароль";
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            var role = ((ComboBoxItem)RoleBox.SelectedItem).Tag.ToString();
            var phone = PhoneBox.Text.Trim();
            var email = EmailBox.Text.Trim();

            var pass = PasswordBox.Visibility == Visibility.Visible
                ? PasswordBox.Password
                : PasswordPlain.Text;
            var pass2 = Password2Box.Visibility == Visibility.Visible
                ? Password2Box.Password
                : Password2Plain.Text;

            var name = NameBox.Text.Trim();

            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(pass) ||
                string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                ErrorText.Text = "Заполните все поля";
                return;
            }
            if (!Regex.IsMatch(phone, @"^\+?[0-9]{10,12}$"))
            {
                ErrorText.Text = "Телефон: только цифры, 10–12 знаков";
                return;
            }
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ErrorText.Text = "Некорректный email";
                return;
            }
            if (pass.Length < 6)
            {
                ErrorText.Text = "Пароль не короче 6 символов";
                return;
            }
            if (pass != pass2)
            {
                ErrorText.Text = "Пароли не совпадают";
                return;
            }
            if (Connection.connect.Users.Any(u => u.Phone == phone))
            {
                ErrorText.Text = "Такой телефон уже зарегистрирован";
                return;
            }
            if (Connection.connect.Users.Any(u => u.Email == email))
            {
                ErrorText.Text = "Такой email уже зарегистрирован";
                return;
            }

            if (role == "Student")
            {
                var card = CardBox.Text.Trim();
                if (!Regex.IsMatch(card, @"^[0-9]+$"))
                {
                    ErrorText.Text = "Номер студенческого — только цифры";
                    return;
                }
                if (Connection.connect.Students.Any(s => s.StudentCardNumber == card))
                {
                    ErrorText.Text = "Студенческий с таким номером уже есть";
                    return;
                }
            }
            else if (role == "Employer")
            {
                var ogrn = OgrnBox.Text.Trim();
                var inn = InnBox.Text.Trim();

                if (!Regex.IsMatch(ogrn, @"^[0-9]{13}$"))
                {
                    ErrorText.Text = "ОГРН — ровно 13 цифр";
                    return;
                }
                if (!Regex.IsMatch(inn, @"^[0-9]{10}$|^[0-9]{12}$"))
                {
                    ErrorText.Text = "ИНН — 10 или 12 цифр";
                    return;
                }
                if (Connection.connect.Companies.Any(c => c.OGRN == ogrn))
                {
                    ErrorText.Text = "Компания с таким ОГРН уже есть";
                    return;
                }
                if (Connection.connect.Companies.Any(c => c.INN == inn))
                {
                    ErrorText.Text = "Компания с таким ИНН уже есть";
                    return;
                }
            }
            else if (role == "Admin")
            {
                var card = AdminCardBox.Text.Trim();
                if (!Regex.IsMatch(card, @"^[0-9]+$"))
                {
                    ErrorText.Text = "Удостоверение админа — только цифры";
                    return;
                }
                if (Connection.connect.Users.Any(u => u.AdminPassNumber == card))
                {
                    ErrorText.Text = "Такое удостоверение уже используется";
                    return;
                }
            }

            var dbRole = Connection.connect.Roles.FirstOrDefault(r => r.Code == role);
            if (dbRole == null)
            {
                ErrorText.Text = "Роль не найдена в системе";
                return;
            }

            var user = new Users
            {
                Phone = phone,
                Email = email,
                PasswordHash = pass,
                RoleId = dbRole.Id,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            if (role == "Admin")
                user.AdminPassNumber = AdminCardBox.Text.Trim();

            Connection.connect.Users.Add(user);
            Connection.connect.SaveChanges();

            if (role == "Student")
            {
                int course = 1;
                int.TryParse(CourseBox.Text.Trim(), out course);

                var st = new Students
                {
                    UserId = user.Id,
                    FullName = name,
                    Course = course,
                    Direction = DirectionBox.Text.Trim(),
                    StudentCardNumber = CardBox.Text.Trim()
                };
                Connection.connect.Students.Add(st);
            }
            else if (role == "Employer")
            {
                var cmp = new Companies
                {
                    UserId = user.Id,
                    Name = name,
                    Direction = CompanyDirBox.Text.Trim(),
                    Description = CompanyDirBox.Text.Trim(),
                    OGRN = OgrnBox.Text.Trim(),
                    INN = InnBox.Text.Trim(),
                    IsPartner = true,
                    IsApproved = false,
                    IsModerated = true,
                    Rating = 0
                };
                Connection.connect.Companies.Add(cmp);
                Connection.connect.SaveChanges();

                Notify.ToAdmin(
                    "Новая компания на модерации",
                    "Зарегистрирована компания: " + cmp.Name,
                    "modcompanies",
                    cmp.Id);
            }

            Connection.connect.SaveChanges();

            AppMessage.Info(this, "Аккаунт создан! Теперь войдите.");
            Close();
        }

        private void GoLogin_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}