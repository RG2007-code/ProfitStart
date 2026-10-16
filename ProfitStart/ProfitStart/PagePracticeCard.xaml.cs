using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProfitStart
{
    public partial class PagePracticeCard : Page
    {
        private int _practiceId;

        public PagePracticeCard() { InitializeComponent(); }

        public PagePracticeCard(int practiceId)
        {
            InitializeComponent();
            _practiceId = practiceId;
            Load();
        }

        private void Load()
        {
            var p = Connection.connect.Practices
                .FirstOrDefault(x => x.Id == _practiceId);
            if (p == null) return;

            TitleText.Text = p.Title ?? "Практика";
            DirectionText.Text = p.Direction ?? "—";
            DurationText.Text = p.Duration ?? "—";
            DescText.Text = p.Description ?? "—";

            if (p.CompanyId.HasValue)
            {
                var c = Connection.connect.Companies
                    .FirstOrDefault(x => x.Id == p.CompanyId.Value);
                CompanyText.Text = c != null ? c.Name : "—";
            }
            else
            {
                CompanyText.Text = "—";
            }

            PaymentText.Text = "—";

            int match = 0;
            if (Session.IsStudent && Session.StudentId != 0)
                match = Skill.CalculatePracticeMatch(Session.StudentId, _practiceId);
            MatchText.Text = "Подходит вам — " + match + "%";

            byte[] avatar = p.AvatarData;
            if ((avatar == null || avatar.Length == 0) && p.CompanyId.HasValue)
            {
                var c = Connection.connect.Companies
                    .FirstOrDefault(x => x.Id == p.CompanyId.Value);
                if (c != null && c.AvatarData != null && c.AvatarData.Length > 0)
                    avatar = c.AvatarData;
                else if (c != null && c.UserId.HasValue)
                {
                    var u = Connection.connect.Users
                        .FirstOrDefault(x => x.Id == c.UserId.Value);
                    if (u != null) avatar = u.AvatarData;
                }
            }

            if (avatar != null && avatar.Length > 0)
                LoadAvatar(avatar);

            LoadSkills();
        }

        private void LoadSkills()
        {
            SkillsPanel.Children.Clear();

            var skillIds = Skill.GetPracticeSkills(_practiceId);
            var skills = Connection.connect.Skills
                .Where(s => skillIds.Contains(s.Id))
                .OrderBy(s => s.Name)
                .ToList();

            if (skills.Count == 0)
            {
                SkillsPanel.Children.Add(new TextBlock
                {
                    Text = "Навыки не указаны",
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#9CA3AF")),
                    FontSize = 13
                });
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
                    Padding = new Thickness(16, 6, 16, 6),
                    Margin = new Thickness(0, 0, 8, 8)
                };

                b.Child = new TextBlock
                {
                    Text = skill.Name,
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString(
                            hasSkill ? "#16A34A" : "#5B5BF5")),
                    FontSize = 12
                };

                SkillsPanel.Children.Add(b);
            }
        }

        private void LoadAvatar(byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();

                    AvatarBig.Fill = new ImageBrush(bmp)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
            }
            catch { }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsAuthenticated)
            {
                AppMessage.Warning(Window.GetWindow(this),
                    "Сначала войдите в систему");
                return;
            }

            if (!Session.IsStudent)
            {
                AppMessage.Warning(Window.GetWindow(this),
                    "Откликаться могут только студенты");
                return;
            }

            var existing = Connection.connect.PracticeApplications
                .FirstOrDefault(a => a.PracticeId == _practiceId
                                  && a.StudentId == Session.StudentId);

            if (existing != null)
            {
                if (existing.Status == "Отклонён")
                {
                    AppMessage.Warning(Window.GetWindow(this),
                        "Ваш отклик на эту практику был отклонён. "
                        + "Повторная подача невозможна.");
                    return;
                }

                if (existing.Status == "Одобрен")
                {
                    AppMessage.Info(Window.GetWindow(this),
                        "Ваш отклик уже одобрен. Смотрите его в разделе «Мои практики».");
                    return;
                }

                AppMessage.Warning(Window.GetWindow(this),
                    "Вы уже откликнулись на эту практику. Дождитесь ответа компании.");
                return;
            }

            NavigationService.Navigate(new PageApplyPractice(_practiceId));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new PagePractice());
        }
    }
}