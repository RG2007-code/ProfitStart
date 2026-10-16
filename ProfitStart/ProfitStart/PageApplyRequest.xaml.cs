using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProfitStart
{
    public partial class PageApplyRequest : Page
    {
        private int _vacancyId;

        public PageApplyRequest()
        {
            InitializeComponent();
        }

        public PageApplyRequest(int vacancyId)
        {
            InitializeComponent();
            _vacancyId = vacancyId;
            LoadVacancy();
        }

        private void LoadVacancy()
        {
            var v = Connection.connect.Vacancies
                .FirstOrDefault(x => x.Id == _vacancyId);
            if (v == null) return;

            RequestTitleText.Text = v.Title + " · " + v.Companies.Name;
        }

        private void Message_TextChanged(object sender, TextChangedEventArgs e)
        {
            CounterText.Text = MessageBox.Text.Length + " / 500";
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
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

            var app = new CompanyApplications
            {
                StudentId = Session.StudentId,
                VacancyId = _vacancyId,
                Message = MessageBox.Text.Trim(),
                Status = "Отправлен",
                CreatedAt = DateTime.Now
            };

            Connection.connect.CompanyApplications.Add(app);
            Connection.connect.SaveChanges();

            var v = Connection.connect.Vacancies
                .FirstOrDefault(x => x.Id == _vacancyId);
            if (v != null && v.CompanyId.HasValue)
            {
                Notify.ToCompany(v.CompanyId.Value,
                "Новый отклик",
                "Студент " + s.FullName + " откликнулся на «" + v.Title + "»",
                "applications", v.Id,
                 s.Id);   
            }

            AppMessage.Success(Window.GetWindow(this), "Отклик отправлен!");
            NavigationService.Navigate(new PageApplications());
        }
    }
}