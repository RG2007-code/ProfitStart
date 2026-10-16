using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProfitStart
{
    public partial class PageAddCompany : Page
    {
        private byte[] _avatarData = null;

        public PageAddCompany()
        {
            InitializeComponent();
        }

        private void DigitsOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageCompanies());
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (!Session.IsAuthenticated)
            {
                ErrorText.Text = "Сначала войдите";
                return;
            }

            var name = NameBox.Text.Trim();
            var ogrn = OgrnBox.Text.Trim();
            var dir = DirBox.Text.Trim();
            var desc = DescBox.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(ogrn))
            {
                ErrorText.Text = "Заполните название и ОГРН";
                return;
            }

            if (!Regex.IsMatch(ogrn, @"^[0-9]{13}$"))
            {
                ErrorText.Text = "ОГРН — ровно 13 цифр";
                return;
            }

            if (Connection.connect.Companies.Any(x => x.OGRN == ogrn))
            {
                ErrorText.Text = "Компания с таким ОГРН уже есть";
                return;
            }

            var company = new Companies
            {
                UserId = Session.UserId,
                Name = name,
                OGRN = ogrn,
                Direction = dir,
                Description = desc,
                AvatarData = _avatarData,
                IsPartner = true,
                IsApproved = false,
                IsModerated = true,
                IsDeleted = false
            };
            Connection.connect.Companies.Add(company);
            Connection.connect.SaveChanges();

            Notify.ToAdmin("Новая компания на модерации",
                "Компания «" + name + "» ждёт одобрения",
                "modcompanies", company.Id);

            AppMessage.Info(Window.GetWindow(this),
                "Компания отправлена на модерацию.");
            NavigationService.Navigate(new PageCompanies());
        }
    }
}