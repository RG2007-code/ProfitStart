using System.Collections.Generic;
using System.Linq;

namespace ProfitStart
{
    public static class Skill
    {
        public static int CalculateMatch(int studentId, int vacancyId)
        {
            var student = Connection.connect.Students
                .FirstOrDefault(s => s.Id == studentId);
            if (student == null) return 0;

            var vacancy = Connection.connect.Vacancies
                .FirstOrDefault(v => v.Id == vacancyId);
            if (vacancy == null) return 0;

            List<int> studentSkillIds = student.Skills
                .Select(s => s.Id)
                .ToList();

            List<int> vacancySkillIds = vacancy.Skills
                .Select(s => s.Id)
                .ToList();

            int total = vacancySkillIds.Count;
            if (total == 0) return 0;

            int matched = studentSkillIds.Intersect(vacancySkillIds).Count();
            return matched * 100 / total;
        }

        public static void SaveVacancySkills(int vacancyId, List<int> skillIds)
        {
            var vacancy = Connection.connect.Vacancies
                .FirstOrDefault(v => v.Id == vacancyId);
            if (vacancy == null) return;

            var oldLinks = vacancy.Skills.ToList();
            foreach (var sk in oldLinks)
                vacancy.Skills.Remove(sk);

            foreach (var skillId in skillIds)
            {
                var skill = Connection.connect.Skills
                    .FirstOrDefault(s => s.Id == skillId);
                if (skill != null)
                    vacancy.Skills.Add(skill);
            }
        }

        public static List<int> GetVacancySkills(int vacancyId)
        {
            var vacancy = Connection.connect.Vacancies
                .FirstOrDefault(v => v.Id == vacancyId);
            if (vacancy == null) return new List<int>();

            return vacancy.Skills.Select(s => s.Id).ToList();
        }

       

        public static int CalculatePracticeMatch(int studentId, int practiceId)
        {
            var student = Connection.connect.Students
                .FirstOrDefault(s => s.Id == studentId);
            if (student == null) return 0;

            var practice = Connection.connect.Practices
                .FirstOrDefault(p => p.Id == practiceId);
            if (practice == null) return 0;

            List<int> studentSkillIds = student.Skills
                .Select(s => s.Id)
                .ToList();

            List<int> practiceSkillIds = practice.Skills
                .Select(s => s.Id)
                .ToList();

            int total = practiceSkillIds.Count;
            if (total == 0) return 0;

            int matched = studentSkillIds.Intersect(practiceSkillIds).Count();
            return matched * 100 / total;
        }

        public static void SavePracticeSkills(int practiceId, List<int> skillIds)
        {
            var practice = Connection.connect.Practices
                .FirstOrDefault(p => p.Id == practiceId);
            if (practice == null) return;

            var oldLinks = practice.Skills.ToList();
            foreach (var sk in oldLinks)
                practice.Skills.Remove(sk);

            foreach (var skillId in skillIds)
            {
                var skill = Connection.connect.Skills
                    .FirstOrDefault(s => s.Id == skillId);
                if (skill != null)
                    practice.Skills.Add(skill);
            }
        }

        public static List<int> GetPracticeSkills(int practiceId)
        {
            var practice = Connection.connect.Practices
                .FirstOrDefault(p => p.Id == practiceId);
            if (practice == null) return new List<int>();

            return practice.Skills.Select(s => s.Id).ToList();
        }
    }
}