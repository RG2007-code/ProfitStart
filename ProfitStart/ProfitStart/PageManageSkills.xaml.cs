using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProfitStart
{
    public partial class PageManageSkills : Page
    {
        public PageManageSkills()
        {
            InitializeComponent();
            SearchPlaceholder.Visibility = Visibility.Visible;
            Load();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchPlaceholder != null)
                SearchPlaceholder.Visibility =
                    string.IsNullOrEmpty(SearchBox.Text)
                    ? Visibility.Visible : Visibility.Collapsed;

            Load();
        }

        private void Load()
        {
            if (SkillsList == null) return;

            var text = SearchBox?.Text?.Trim() ?? "";

            var query = Connection.connect.Skills.AsQueryable();

            if (!string.IsNullOrEmpty(text))
                query = query.Where(s => s.Name.Contains(text));

            var skills = query.OrderBy(s => s.Name).ToList();

            var list = new List<object>();

            foreach (var s in skills)
            {
                int studentsCount = 0;
                int vacanciesCount = 0;

                try
                {
                    studentsCount = Connection.connect.Students
                        .Count(st => st.Skills.Any(sk => sk.Id == s.Id));
                }
                catch { }

                try
                {
                    vacanciesCount = Connection.connect.Vacancies
                        .Count(v => v.Skills.Any(sk => sk.Id == s.Id));
                }
                catch { }

                list.Add(new
                {
                    s.Id,
                    s.Name,
                    StudentsCount = studentsCount,
                    VacanciesCount = vacanciesCount
                });
            }

            SkillsList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void AddSkill_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            var name = NewSkillBox.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;

            if (Connection.connect.Skills.Any(s => s.Name == name))
            {
                ErrorText.Text = "Такой навык уже есть";
                return;
            }

            var skill = new Skills { Name = name };
            Connection.connect.Skills.Add(skill);
            Connection.connect.SaveChanges();

            NewSkillBox.Text = "";
            Load();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;

            if (!AppMessage.Confirm(Window.GetWindow(this),
                "Удалить навык?", "Удаление"))
                return;

            var skill = Connection.connect.Skills
                .FirstOrDefault(s => s.Id == id);
            if (skill == null) return;

           
            try
            {
                Connection.connect.Database.ExecuteSqlCommand(
                    "DELETE FROM StudentSkills WHERE SkillId = {0}", id);
            }
            catch { }

            
            try
            {
                Connection.connect.Database.ExecuteSqlCommand(
                    "DELETE FROM VacancySkills WHERE SkillId = {0}", id);
            }
            catch { }

            Connection.connect.Skills.Remove(skill);
            Connection.connect.SaveChanges();

            AppMessage.Success(Window.GetWindow(this), "Навык удалён");
            Load();
        }
    }
}