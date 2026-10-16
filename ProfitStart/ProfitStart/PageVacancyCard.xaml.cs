using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageVacancyCard : Page
    {
        private int _vacancyId;
        private int _companyId;

        public PageVacancyCard() { InitializeComponent(); }

        public PageVacancyCard(int vacancyId)
        {
            InitializeComponent();
            _vacancyId = vacancyId;
            Load();
        }

        private void Load()
        {
            var v = Connection.connect.Vacancies
                .Where(x => x.Id == _vacancyId)
                .Select(x => new
                {
                    x.Title,
                    CompanyName = x.Companies.Name,
                    x.CompanyId,
                    x.Type,
                    x.Description,
                    x.Requirements,
                    x.MatchPercent,
                    x.Salary,
                    x.PaymentUnit,
                    x.AvatarData
                })
                .FirstOrDefault();

            if (v == null) return;
            _companyId = v.CompanyId ?? 0;

            TitleText.Text = v.Title;
            SubText.Text = v.CompanyName + " · " + v.Type;
            DescText.Text = v.Description;
            ReqText.Text = v.Requirements;

            int match = 0;
            if (Session.IsStudent && Session.StudentId != 0)
                match = Skill.CalculateMatch(Session.StudentId, _vacancyId);
            else
                match = v.MatchPercent ?? 0;

            MatchText.Text = "Подходит вам — " + match + "%";

            
            string salaryFormatted = Payment.Format(v.Salary, v.PaymentUnit);
            if (!string.IsNullOrEmpty(salaryFormatted))
            {
                SalaryText.Text = salaryFormatted;
                SalaryBlock.Visibility = Visibility.Visible;
            }
            else
            {
                SalaryBlock.Visibility = Visibility.Collapsed;
            }

            
            byte[] avatar = v.AvatarData;
            if ((avatar == null || avatar.Length == 0) && v.CompanyId.HasValue)
            {
                var company = Connection.connect.Companies
                    .FirstOrDefault(c => c.Id == v.CompanyId.Value);
                if (company != null)
                {
                    avatar = company.AvatarData;
                    if ((avatar == null || avatar.Length == 0)
                        && company.UserId.HasValue)
                    {
                        var owner = Connection.connect.Users
                            .FirstOrDefault(u => u.Id == company.UserId.Value);
                        if (owner != null) avatar = owner.AvatarData;
                    }
                }
            }
            if (avatar != null && avatar.Length > 0)
                Image.LoadIntoEllipse(AvatarBig, avatar);

            LoadSkills();
        }

        private void LoadSkills()
        {
            SkillsPanel.Children.Clear();

            var skillIds = Skill.GetVacancySkills(_vacancyId);
            var skills = Connection.connect.Skills
                .Where(s => skillIds.Contains(s.Id))
                .OrderBy(s => s.Name)
                .ToList();

            if (skills.Count == 0)
            {
                var noSkills = new TextBlock
                {
                    Text = "Навыки не указаны",
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#9CA3AF")),
                    FontSize = 13
                };
                SkillsPanel.Children.Add(noSkills);
                return;
            }

            var studentSkillIds = new System.Collections.Generic.List<int>();
            if (Session.IsStudent && Session.StudentId != 0)
            {
                var student = Connection.connect.Students
                    .FirstOrDefault(s => s.Id == Session.StudentId);
                if (student != null)
                    studentSkillIds = student.Skills.Select(s => s.Id).ToList();
            }

            foreach (var skill in skills)
            {
                bool hasSkill = studentSkillIds.Contains(skill.Id);

                var b = new Border
                {
                    Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString(
                            hasSkill ? "#DCFCE7" : "#EEF2FF")),
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(16, 8, 16, 8),
                    Margin = new Thickness(0, 0, 8, 8)
                };

                var sp = new StackPanel { Orientation = Orientation.Horizontal };

                if (hasSkill && Session.IsStudent)
                {
                    sp.Children.Add(new TextBlock
                    {
                        Text = "✓ ",
                        Foreground = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#16A34A")),
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                }

                sp.Children.Add(new TextBlock
                {
                    Text = skill.Name,
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString(
                            hasSkill ? "#16A34A" : "#5B5BF5")),
                    FontSize = 13
                });

                b.Child = sp;
                SkillsPanel.Children.Add(b);
            }
        }

        private void CompanyText_Click(object sender, MouseButtonEventArgs e)
            => OpenCompany();

        private void Company_Click(object sender, RoutedEventArgs e)
            => OpenCompany();

        private void OpenCompany()
        {
            if (_companyId == 0) return;
            NavigationService.Navigate(new PageCompanyCard(_companyId));
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsAuthenticated)
            {
                AppMessage.Warning(Window.GetWindow(this),
                    "Сначала войдите в систему", "Отклик");
                return;
            }

            if (!Session.IsStudent)
            {
                AppMessage.Warning(Window.GetWindow(this),
                    "Откликаться могут только студенты", "Отклик");
                return;
            }

            var exists = Connection.connect.CompanyApplications
                .Any(a => a.StudentId == Session.StudentId
                       && a.VacancyId == _vacancyId);

            if (exists)
            {
                AppMessage.Warning(Window.GetWindow(this),
                    "Вы уже откликнулись на эту вакансию", "Отклик");
                return;
            }

            NavigationService.Navigate(new PageApplyRequest(_vacancyId));
        }
    }
}