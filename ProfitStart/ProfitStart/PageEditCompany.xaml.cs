using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProfitStart
{
    public partial class PageEditCompany : Page
    {
        private int _companyId;
        private byte[] _avatarData = null;

        public PageEditCompany()
        {
            InitializeComponent();
            _companyId = Session.CompanyId;
            Load();
        }

        public PageEditCompany(int companyId)
        {
            InitializeComponent();
            _companyId = companyId;
            Load();
        }

        private void Load()
        {
            if (_companyId == 0)
            {
                StatusText.Text = "Компания не найдена";
                return;
            }

            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == _companyId);
            if (c == null)
            {
                StatusText.Text = "Компания не найдена";
                return;
            }

            NameBox.Text = c.Name;
            OgrnBox.Text = c.OGRN;
            DirBox.Text = c.Direction;
            DescBox.Text = c.Description;

            _avatarData = c.AvatarData;
            if (_avatarData != null && _avatarData.Length > 0)
                Image.LoadIntoBorder(PreviewAvatar, _avatarData);

            if (c.IsModerated == true)
                StatusText.Text = "⚠ Есть непринятые изменения.";
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
            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == _companyId);
            if (c == null) return;

            
            if (Session.IsAdmin)
            {
                c.Name = NameBox.Text.Trim();
                c.Direction = DirBox.Text.Trim();
                c.Description = DescBox.Text.Trim();
                c.AvatarData = _avatarData;
                c.IsApproved = true;
                c.IsModerated = false;
                c.PendingData = null;

                Connection.connect.SaveChanges();
                AppMessage.Success(Window.GetWindow(this), "Компания сохранена");
                NavigationService.Navigate(new PageModerationCompanies());
                return;
            }

            
            c.AvatarData = _avatarData;
            c.PendingData = NameBox.Text.Trim() + "|" +
                            DirBox.Text.Trim() + "|" +
                            DescBox.Text.Trim();
            c.IsModerated = true;
            Connection.connect.SaveChanges();

            AppMessage.Info(Window.GetWindow(this),
                "Изменения отправлены на модерацию.");
            NavigationService.Navigate(new PageCompanies());
        }
    }
}