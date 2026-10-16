using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProfitStart
{
    public partial class PageCompanyDispute : Page
    {
        private int _companyId;

        public PageCompanyDispute()
        {
            InitializeComponent();
            Load();
        }

        public PageCompanyDispute(int companyId)
        {
            InitializeComponent();
            _companyId = companyId;
            Load();
        }

        private void Load()
        {
            if (!Session.IsAdmin)
            {
                NavigationService.Navigate(new PageProfile());
                return;
            }

            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == _companyId);
            if (c == null)
            {
                ErrorText.Text = "Компания не найдена";
                return;
            }

            CompanyNameText.Text = c.Name ?? "—";
            CompanyInfoText.Text = "ИНН: " + (c.INN ?? "—")
                                 + " · ОГРН: " + (c.OGRN ?? "—");

            RejectReasonText.Text = string.IsNullOrEmpty(c.RejectionReason)
                ? "—" : c.RejectionReason;

            DisputeText.Text = string.IsNullOrEmpty(c.DisputeMessage)
                ? "Чтобы подать оспаривание — перейдите в профиль компании"
                : c.DisputeMessage;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageCompanyDisputes());
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsAdmin) return;

            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == _companyId);
            if (c == null) return;

            c.IsDeleted = false;
            c.IsApproved = true;
            c.IsModerated = false;
            c.DisputeRequested = false;
            c.DisputeMessage = null;
            c.RejectionReason = null;

            Connection.connect.SaveChanges();

            Notify.ToCompany(c.Id, "Компания восстановлена",
                "Ваша компания «" + c.Name + "» одобрена и восстановлена.",
                "profile", c.Id);

            MarkAdminNotifRead("companydispute", c.Id);
            MarkAdminNotifRead("managecompanies", c.Id);

            AppMessage.Success(Window.GetWindow(this),
                "Компания одобрена и восстановлена");

            var main = Window.GetWindow(this) as MainWindow;
            main?.LoadBellBadge();

            NavigationService.Navigate(new PageCompanyDisputes());
        }

        private void DeleteForever_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsAdmin) return;

            if (!AppMessage.Confirm(Window.GetWindow(this),
                "Удалить компанию окончательно?\n"
                + "Все вакансии, отклики, сообщения и уведомления будут удалены "
                + "без возможности восстановления.",
                "Окончательное удаление"))
                return;

            MarkAdminNotifRead("companydispute", _companyId);
            MarkAdminNotifRead("managecompanies", _companyId);

            bool ok = CompanyDispute.DeleteForever(_companyId);
            if (ok)
            {
                AppMessage.Success(Window.GetWindow(this),
                    "Компания удалена окончательно");

                var main = Window.GetWindow(this) as MainWindow;
                main?.LoadBellBadge();

                NavigationService.Navigate(new PageCompanyDisputes());
            }
            else
            {
                ErrorText.Text = "Не удалось удалить компанию";
            }
        }

        private void MarkAdminNotifRead(string linkPage, int linkId)
        {
            var notifs = Connection.connect.Notifications
                .Where(n => n.RecipientType == "Admin"
                         && n.LinkPage == linkPage
                         && n.LinkId == linkId
                         && n.IsRead == false)
                .ToList();

            foreach (var n in notifs)
                n.IsRead = true;

            Connection.connect.SaveChanges();
        }
    }
}