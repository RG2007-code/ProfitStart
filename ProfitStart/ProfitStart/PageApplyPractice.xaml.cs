using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProfitStart
{
    public partial class PageApplyPractice : Page
    {
        private int _practiceId;

        public PageApplyPractice()
        {
            InitializeComponent();
        }

        public PageApplyPractice(int practiceId)
        {
            InitializeComponent();
            _practiceId = practiceId;
            LoadPractice();
        }

        private void LoadPractice()
        {
            var p = Connection.connect.Practices
                .FirstOrDefault(x => x.Id == _practiceId);
            if (p == null) return;

            string companyName = "—";
            if (p.CompanyId.HasValue)
            {
                var c = Connection.connect.Companies
                    .FirstOrDefault(x => x.Id == p.CompanyId.Value);
                if (c != null) companyName = c.Name;
            }

            RequestTitleText.Text = p.Title + " · " + companyName;
        }

        private void Message_TextChanged(object sender, TextChangedEventArgs e)
        {
            CounterText.Text = MessageBox.Text.Length + " / 500";
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new PagePractice());
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (!Session.IsAuthenticated || !Session.IsStudent)
            {
                ErrorText.Text = "Только студенты могут отправлять отклики";
                return;
            }

            var s = Connection.connect.Students
                .FirstOrDefault(x => x.Id == Session.StudentId);
            if (s == null) return;

            if (string.IsNullOrEmpty(s.ResumeText))
            {
                ErrorText.Text = "Сначала заполните резюме в профиле";
                return;
            }

            if (s.PassportData == null || s.PassportData.Length == 0)
            {
                ErrorText.Text = "Сначала загрузите паспорт в профиле";
                return;
            }

            if (s.IsGraduated == true
                && (s.DiplomaData == null || s.DiplomaData.Length == 0))
            {
                ErrorText.Text = "Сначала загрузите диплом в профиле";
                return;
            }

            bool exists = Connection.connect.PracticeApplications
                .Any(a => a.PracticeId == _practiceId
                       && a.StudentId == Session.StudentId);

            if (exists)
            {
                ErrorText.Text = "Вы уже откликнулись на эту практику";
                return;
            }

            var app = new PracticeApplications
            {
                StudentId = Session.StudentId,
                PracticeId = _practiceId,
                Message = MessageBox.Text.Trim(),
                Status = "Отправлен",
                CreatedAt = DateTime.Now
            };

            Connection.connect.PracticeApplications.Add(app);
            Connection.connect.SaveChanges();

            var p = Connection.connect.Practices
                .FirstOrDefault(x => x.Id == _practiceId);

            if (p != null && p.CompanyId.HasValue)
            {
                Notify.ToCompany(p.CompanyId.Value,
                    "Новый отклик на практику",
                    "Студент " + s.FullName
                    + " откликнулся на практику «" + p.Title + "»",
                    "practiceapps", p.Id,
                    s.Id);
            }

            AppMessage.Success(Window.GetWindow(this),
                "Отклик на практику отправлен!");
            NavigationService.Navigate(new PagePractice());
        }
    }
}