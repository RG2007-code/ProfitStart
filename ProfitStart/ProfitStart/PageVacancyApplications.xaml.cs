using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProfitStart
{
    public partial class PageVacancyApplications : Page
    {
        private int _vacancyId;

        public PageVacancyApplications() { InitializeComponent(); }

        public PageVacancyApplications(int vacancyId)
        {
            InitializeComponent();
            _vacancyId = vacancyId;
            Load();
        }

        private void Load()
        {
            var v = Connection.connect.Vacancies
                .FirstOrDefault(x => x.Id == _vacancyId);
            if (v != null) VacancyTitle.Text = "Отклики: " + v.Title;

            var apps = Connection.connect.CompanyApplications
                .Where(a => a.VacancyId == _vacancyId)
                .OrderByDescending(a => a.CreatedAt).ToList();

            var list = apps.Select(a => new
            {
                a.Id,
                a.StudentId,
                StudentName = a.Students.FullName,
                Subtitle = a.Students.Course + " курс · " + a.Students.Direction,
                Message = a.Message,
                a.Status,
                StudentAvatar = a.Students.UserId.HasValue
                    ? a.Students.Users.AvatarData : null
            }).ToList();

            ApplicationsList.ItemsSource = list;
        }

        private void StudentAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void StudentProfile_Click(object sender, RoutedEventArgs e)
        {
            int studentId = (int)((Button)sender).Tag;
            NavigationService.Navigate(new PageStudentProfile(studentId));
        }

        private void ChangeStatus(int id, string status)
        {
            var a = Connection.connect.CompanyApplications
                .FirstOrDefault(x => x.Id == id);
            if (a == null) return;

            a.Status = status;
            Connection.connect.SaveChanges();

            var v = Connection.connect.Vacancies
                .FirstOrDefault(x => x.Id == a.VacancyId);
            string title = v != null ? v.Title : "вакансия";

            Notify.ToStudent(a.StudentId, "Обновление по отклику",
                "Ваш отклик на «" + title + "»: " + status,
                "applications", a.VacancyId);

            AppMessage.Success(Window.GetWindow(this), "Статус: " + status);
            Load();
        }

        private void Interview_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            ChangeStatus(id, "Интервью");
        }

        private void Offer_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            ChangeStatus(id, "Оффер");
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            ChangeStatus(id, "Отказ");
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageMyVacancies());
        }
    }
}