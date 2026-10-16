using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProfitStart
{
    public partial class PageAdmin : Page
    {
        private DispatcherTimer _refreshTimer;

        public PageAdmin()
        {
            InitializeComponent();

            Load();
            Loaded += (s, e) =>
            {
                DrawCharts();
                StartTimer();
            };

            Unloaded += (s, e) => StopTimer();

            IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue)
                {
                    Load();
                    DrawCharts();
                    StartTimer();
                }
                else
                {
                    StopTimer();
                }
            };
        }

        private static string RuStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return "—";

            switch (status.Trim())
            {
                case "Sent":
                case "Отправлен":
                    return "Отправлен";
                case "Approved":
                case "Одобрен":
                    return "Одобрен";
                case "Rejected":
                case "Отказ":
                case "Отклонён":
                    return "Отклонён";
                case "Interview":
                case "Интервью":
                    return "Интервью";
                case "Offer":
                case "Оффер":
                    return "Оффер";
                default:
                    return status;
            }
        }

        private void StartTimer()
        {
            if (_refreshTimer == null)
            {
                _refreshTimer = new DispatcherTimer();
                _refreshTimer.Interval = TimeSpan.FromSeconds(3);
                _refreshTimer.Tick += (s, e) =>
                {
                    Load();
                    DrawCharts();
                };
            }

            _refreshTimer.Start();
        }

        private void StopTimer()
        {
            if (_refreshTimer != null)
                _refreshTimer.Stop();
        }

        private void Load()
        {
            int students = Connection.connect.Students.Count();
            int vacancies = Connection.connect.Vacancies.Count();
            int practices = Connection.connect.Practices.Count();
            int companies = Connection.connect.Companies.Count();

            int appsVac = Connection.connect.CompanyApplications.Count();
            int appsPrac = Connection.connect.PracticeApplications.Count();
            int appsTotal = appsVac + appsPrac;

            CntStudents.Text = students.ToString();
            CntVacancies.Text = vacancies.ToString();
            CntCompanies.Text = companies.ToString();
            CntApps.Text = appsTotal.ToString();

            var lastVac = Connection.connect.CompanyApplications
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToList()
                .Select(a => new
                {
                    Text = (a.Students != null ? a.Students.FullName : "—")
                           + " · "
                           + (a.Vacancies != null ? a.Vacancies.Title : "—"),
                    Status = RuStatus(a.Status),
                    Date = a.CreatedAt
                })
                .ToList();

            var lastPrac = Connection.connect.PracticeApplications
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToList()
                .Select(a => new
                {
                    Text = (Connection.connect.Students
                                .FirstOrDefault(s => s.Id == a.StudentId)?.FullName ?? "—")
                           + " · "
                           + (Connection.connect.Practices
                                .FirstOrDefault(p => p.Id == a.PracticeId)?.Title ?? "—"),
                    Status = RuStatus(a.Status),
                    Date = (DateTime?)a.CreatedAt
                })
                .ToList();

            var combined = lastVac
                .Select(x => new { x.Text, x.Status, Date = (DateTime?)x.Date })
                .Concat(lastPrac)
                .OrderByDescending(x => x.Date)
                .Take(5)
                .Select(x => new { x.Text, x.Status })
                .ToList();

            LastApps.ItemsSource = combined;

            BuildSummary(students, companies, vacancies, practices,
                         appsVac, appsPrac);
        }

        private void BuildSummary(int students, int companies,
                                  int vacancies, int practices,
                                  int appsVac, int appsPrac)
        {
            if (SummaryText == null) return;

            int approvedVac = Connection.connect.Vacancies
                .Count(v => v.IsApproved == true);

            int approvedPrac = Connection.connect.Practices
                .Count(p => p.IsApproved == true);

            int vacApproved = Connection.connect.CompanyApplications
                .Count(a => a.Status == "Одобрен" || a.Status == "Approved");
            int vacRejected = Connection.connect.CompanyApplications
                .Count(a => a.Status == "Отказ"
                         || a.Status == "Отклонён"
                         || a.Status == "Rejected");
            int vacPending = appsVac - vacApproved - vacRejected;

            int pracApproved = Connection.connect.PracticeApplications
                .Count(a => a.Status == "Одобрен" || a.Status == "Approved");
            int pracRejected = Connection.connect.PracticeApplications
                .Count(a => a.Status == "Отклонён"
                         || a.Status == "Отказ"
                         || a.Status == "Rejected");
            int pracPending = appsPrac - pracApproved - pracRejected;

            int appsTotal = appsVac + appsPrac;
            int approvedTotal = vacApproved + pracApproved;
            int rejectedTotal = vacRejected + pracRejected;
            int pendingTotal = vacPending + pracPending;

            double avgRating = 0;
            var rated = Connection.connect.Companies.ToList();
            if (rated.Count > 0)
                avgRating = rated.Average(c => (double)c.Rating);

            SummaryText.Text =
                $"Студентов: {students}\n" +
                $"Компаний: {companies}\n" +
                $"Вакансий всего: {vacancies}\n" +
                $"  • одобрено: {approvedVac}\n" +
                $"Практик всего: {practices}\n" +
                $"  • одобрено: {approvedPrac}\n" +
                $"\n" +
                $"Откликов всего: {appsTotal}\n" +
                $"  • на вакансии: {appsVac}\n" +
                $"  • на практики: {appsPrac}\n" +
                $"\n" +
                $"✓ Одобрено: {approvedTotal}\n" +
                $"  • вакансии: {vacApproved}\n" +
                $"  • практики: {pracApproved}\n" +
                $"\n" +
                $"✕ Отклонено: {rejectedTotal}\n" +
                $"  • вакансии: {vacRejected}\n" +
                $"  • практики: {pracRejected}\n" +
                $"\n" +
                $"⏳ В ожидании: {pendingTotal}\n" +
                $"  • вакансии: {vacPending}\n" +
                $"  • практики: {pracPending}\n" +
                $"\n" +
                $"Средний рейтинг компаний: {avgRating:0.00}";
        }

        private void Period_Changed(object sender, SelectionChangedEventArgs e)
            => DrawCharts();

        private void DrawCharts()
        {
            DrawRegistrationsChart();
            DrawVacancyTypesChart();
            DrawApplicationStatusesChart();
            DrawTopCompaniesChart();
        }

        private int GetMonthsBack()
        {
            var item = PeriodFilter?.SelectedItem as ComboBoxItem;
            string tag = item?.Content?.ToString() ?? "За 6 месяцев";

            if (tag == "За 12 месяцев") return 12;
            if (tag == "За всё время") return -1;
            return 6;
        }

        private void DrawRegistrationsChart()
        {
            if (ChartRegistrations == null) return;

            int monthsBack = GetMonthsBack();
            var now = DateTime.Now;

            var users = Connection.connect.Users
                .Where(u => u.CreatedAt != null)
                .Select(u => u.CreatedAt.Value)
                .ToList();

            var data = new Dictionary<string, int>();

            if (monthsBack == -1)
            {
                if (users.Count == 0)
                {
                    Chart.DrawEmpty(ChartRegistrations, "Нет данных");
                    return;
                }

                var byYear = users
                    .GroupBy(d => d.Year)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key.ToString(),
                                  g => g.Count());

                foreach (var kv in byYear)
                    data[kv.Key] = kv.Value;
            }
            else
            {
                var months = new List<DateTime>();
                for (int i = monthsBack - 1; i >= 0; i--)
                    months.Add(new DateTime(now.Year, now.Month, 1)
                        .AddMonths(-i));

                foreach (var m in months)
                {
                    int cnt = users.Count(d =>
                        d.Year == m.Year && d.Month == m.Month);

                    string label = m.ToString("MMM yy",
                        new System.Globalization.CultureInfo("ru-RU"));
                    data[label] = cnt;
                }
            }

            Chart.DrawBars(ChartRegistrations, data);
        }

        private void DrawVacancyTypesChart()
        {
            if (ChartVacancyTypes == null) return;

            var grouped = Connection.connect.Vacancies
                .GroupBy(v => v.Type)
                .Select(g => new { Key = g.Key ?? "—", Cnt = g.Count() })
                .OrderByDescending(g => g.Cnt)
                .ToList();

            var data = new Dictionary<string, int>();
            foreach (var g in grouped)
                data[g.Key] = g.Cnt;

            Chart.DrawPie(ChartVacancyTypes, data);

            if (VacancyLegend != null)
            {
                var legendItems = new List<object>();
                int idx = 0;
                foreach (var g in grouped)
                {
                    legendItems.Add(new
                    {
                        Label = g.Key + " (" + g.Cnt + ")",
                        Color = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString(
                                Chart.Palette[idx % Chart.Palette.Length]))
                    });
                    idx++;
                }
                VacancyLegend.ItemsSource = legendItems;
            }
        }

        private void DrawApplicationStatusesChart()
        {
            if (ChartApplicationStatuses == null) return;

            var vacApps = Connection.connect.CompanyApplications
                .Select(a => a.Status).ToList()
                .Select(s => RuStatus(s)).ToList();

            var pracApps = Connection.connect.PracticeApplications
                .Select(a => a.Status).ToList()
                .Select(s => RuStatus(s)).ToList();

            var all = vacApps.Concat(pracApps).ToList();

            var grouped = all
                .GroupBy(s => s)
                .Select(g => new { Key = g.Key, Cnt = g.Count() })
                .OrderByDescending(g => g.Cnt)
                .ToList();

            var data = new Dictionary<string, int>();
            foreach (var g in grouped)
                data[g.Key] = g.Cnt;

            Chart.DrawBars(ChartApplicationStatuses, data);
        }

        private void DrawTopCompaniesChart()
        {
            if (ChartTopCompanies == null) return;

            var apps = Connection.connect.CompanyApplications
                .Where(a => a.Vacancies != null)
                .ToList();

            var grouped = apps
                .GroupBy(a => a.Vacancies.Companies != null
                              ? a.Vacancies.Companies.Name
                              : "—")
                .Select(g => new { Key = g.Key, Cnt = g.Count() })
                .OrderByDescending(g => g.Cnt)
                .Take(5)
                .ToList();

            var data = new Dictionary<string, int>();
            foreach (var g in grouped)
                data[g.Key] = g.Cnt;

            Chart.DrawHorizontalBars(ChartTopCompanies, data);
        }

        private void ManageSkills_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new PageManageSkills());

        private void ManageCompanies_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new PageManageCompanies());

        private void ModerateVacancies_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new PageModerationVacancies());

        private void ManageUsers_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new PageManageUsers());

        private void ExportUsers_Click(object sender, RoutedEventArgs e)
        {
            var users = Connection.connect.Users
                .Select(u => new
                {
                    u.Email,
                    u.Phone,
                    Role = u.Roles != null ? u.Roles.Name : "—",
                    RoleCode = u.Roles != null ? u.Roles.Code : "—",
                    u.IsActive,
                    u.CreatedAt
                })
                .ToList();

            Export.Items(
                "Пользователи_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx",
                "Пользователи",
                new List<string>
                {
                    "Email", "Телефон", "Роль", "Код роли",
                    "Активен", "Создан"
                },
                users,
                u => new List<object>
                {
                    u.Email ?? "",
                    u.Phone ?? "",
                    u.Role ?? "",
                    u.RoleCode ?? "",
                    u.IsActive == true ? "Да" : "Нет",
                    u.CreatedAt.HasValue ? (object)u.CreatedAt.Value : ""
                },
                Window.GetWindow(this));
        }

        private void ExportVacancies_Click(object sender, RoutedEventArgs e)
        {
            var vacancies = Connection.connect.Vacancies
                .Select(v => new
                {
                    v.Title,
                    Company = v.Companies != null ? v.Companies.Name : "—",
                    v.Type,
                    v.Salary,
                    v.PaymentUnit,
                    v.IsApproved,
                    v.CreatedAt
                })
                .ToList();

            Export.Items(
                "Вакансии_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx",
                "Вакансии",
                new List<string>
                {
                    "Название", "Компания", "Тип",
                    "Зарплата", "Единица", "Одобрено", "Создана"
                },
                vacancies,
                v => new List<object>
                {
                    v.Title ?? "",
                    v.Company ?? "",
                    v.Type ?? "",
                    v.Salary ?? "",
                    v.PaymentUnit == "hour"  ? "час"
                    : v.PaymentUnit == "month" ? "месяц"
                    : "",
                    v.IsApproved == true ? "Да" : "Нет",
                    v.CreatedAt
                },
                Window.GetWindow(this));
        }

        private void ExportPractices_Click(object sender, RoutedEventArgs e)
        {
            var practices = Connection.connect.Practices
                .Select(p => new
                {
                    p.Title,
                    p.IsApproved,
                    p.CreatedAt
                })
                .ToList();

            Export.Items(
                "Практики_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx",
                "Практики",
                new List<string>
                {
                    "Название", "Одобрена", "Создана"
                },
                practices,
                p => new List<object>
                {
                    p.Title ?? "",
                    p.IsApproved == true ? "Да" : "Нет",
                    p.CreatedAt
                },
                Window.GetWindow(this));
        }

        private void ExportCompanies_Click(object sender, RoutedEventArgs e)
        {
            var companies = Connection.connect.Companies
                .Select(c => new
                {
                    c.Name,
                    c.OGRN,
                    c.INN,
                    c.Direction,
                    c.Rating,
                    c.IsApproved,
                    c.IsPartner,
                    c.IsDeleted
                })
                .ToList();

            Export.Items(
                "Компании_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx",
                "Компании",
                new List<string>
                {
                    "Название", "ОГРН", "ИНН", "Сфера",
                    "Рейтинг", "Одобрена", "Партнёр", "Удалена"
                },
                companies,
                c => new List<object>
                {
                    c.Name ?? "",
                    c.OGRN ?? "",
                    c.INN ?? "",
                    c.Direction ?? "",
                    (double)c.Rating,
                    c.IsApproved == true ? "Да" : "Нет",
                    c.IsPartner  == true ? "Да" : "Нет",
                    c.IsDeleted  == true ? "Да" : "Нет"
                },
                Window.GetWindow(this));
        }

        private void ExportApplications_Click(object sender, RoutedEventArgs e)
        {
            var vacApps = Connection.connect.CompanyApplications
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    Student = a.Students != null ? a.Students.FullName : "—",
                    Vacancy = a.Vacancies != null ? a.Vacancies.Title : "—",
                    Company = a.Vacancies != null && a.Vacancies.Companies != null
                              ? a.Vacancies.Companies.Name : "—",
                    a.Status,
                    a.CreatedAt
                })
                .ToList();

            Export.Items(
                "Отклики_вакансии_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx",
                "Отклики на вакансии",
                new List<string>
                {
                    "Студент", "Вакансия", "Компания", "Статус", "Создан"
                },
                vacApps,
                a => new List<object>
                {
                    a.Student ?? "",
                    a.Vacancy ?? "",
                    a.Company ?? "",
                    RuStatus(a.Status),
                    a.CreatedAt
                },
                Window.GetWindow(this));

            var students = Connection.connect.Students.ToList();
            var practices = Connection.connect.Practices.ToList();

            var pracAppsRaw = Connection.connect.PracticeApplications
                .OrderBy(a => a.Id)
                .ToList();

            var pracApps = pracAppsRaw
                .Select(a =>
                {
                    var st = students.FirstOrDefault(s => s.Id == a.StudentId);
                    var pr = practices.FirstOrDefault(p => p.Id == a.PracticeId);
                    return new
                    {
                        Student = st != null ? st.FullName : "—",
                        Practice = pr != null ? pr.Title : "—",
                        a.Status,
                        a.CreatedAt
                    };
                })
                .ToList();

            Export.Items(
                "Отклики_практика_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx",
                "Отклики на практику",
                new List<string>
                {
                    "Студент", "Практика", "Статус", "Создан"
                },
                pracApps,
                a => new List<object>
                {
                    a.Student ?? "",
                    a.Practice ?? "",
                    RuStatus(a.Status),
                    a.CreatedAt
                },
                Window.GetWindow(this));
        }
    }
}