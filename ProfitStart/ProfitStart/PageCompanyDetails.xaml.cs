using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProfitStart
{
    public partial class PageCompanyDetails : Page
    {
        private int _companyId;

        public PageCompanyDetails() { InitializeComponent(); }

        public PageCompanyDetails(int companyId)
        {
            InitializeComponent();
            _companyId = companyId;
            Load();
        }

        private void Load()
        {
            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == _companyId);
            if (c == null) return;

            NameText.Text = c.Name;
            DirText.Text = c.Direction;
            OgrnText.Text = c.OGRN;
            DescText.Text = c.Description;

            if (c.IsApproved == false)
                StatusText.Text = "⚠ Новая компания — ждёт модерации";
            else if (c.IsModerated == true)
                StatusText.Text = "⚠ Есть непринятые изменения";
            else
                StatusText.Text = "✓ Компания одобрена";

            
            byte[] avatar = c.AvatarData;
            if ((avatar == null || avatar.Length == 0) && c.UserId.HasValue)
            {
                var owner = Connection.connect.Users
                    .FirstOrDefault(u => u.Id == c.UserId.Value);
                if (owner != null) avatar = owner.AvatarData;
            }

            if (avatar != null && avatar.Length > 0)
                Image.LoadIntoEllipse(AvatarBig, avatar);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}