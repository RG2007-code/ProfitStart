using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitStart
{
    public static class CompanyDispute
    {
        public static bool Submit(int companyId, string message)
        {
            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == companyId);
            if (c == null) return false;
            if (c.IsDeleted != true) return false;

            c.DisputeRequested = true;
            c.DisputeMessage = string.IsNullOrWhiteSpace(message)
                ? "Просьба пересмотреть удаление компании."
                : message.Trim();

            Connection.connect.SaveChanges();

            Notify.ToAdmin(
                "Оспаривание удаления компании",
                "Компания «" + c.Name + "» оспаривает удаление: " + c.DisputeMessage,
                "companydispute",
                c.Id);

            return true;
        }

        public static bool DeleteForever(int companyId)
        {
            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == companyId);
            if (c == null) return false;

            int? ownerUserId = c.UserId;

            try
            {
                var ratings = Connection.connect.CompanyRatings
                    .Where(r => r.CompanyId == companyId)
                    .ToList();
                foreach (var r in ratings)
                    Connection.connect.CompanyRatings.Remove(r);

                Connection.connect.SaveChanges();
            }
            catch
            {
                try
                {
                    Connection.connect.Database.ExecuteSqlCommand(
                        "DELETE FROM CompanyRatings WHERE CompanyId = {0}",
                        companyId);
                }
                catch { }
            }

            try
            {
                var msgs = Connection.connect.Messages
                    .Where(m => m.CompanyId == companyId)
                    .ToList();
                foreach (var m in msgs)
                    Connection.connect.Messages.Remove(m);

                Connection.connect.SaveChanges();
            }
            catch
            {
                try
                {
                    Connection.connect.Database.ExecuteSqlCommand(
                        "DELETE FROM Messages WHERE CompanyId = {0}",
                        companyId);
                }
                catch { }
            }

            var vacancyIds = Connection.connect.Vacancies
                .Where(v => v.CompanyId == companyId)
                .Select(v => v.Id)
                .ToList();

            if (vacancyIds.Count > 0)
            {
                var apps = Connection.connect.CompanyApplications
                    .Where(a => vacancyIds.Contains(a.VacancyId))
                    .ToList();
                foreach (var a in apps)
                    Connection.connect.CompanyApplications.Remove(a);

                try
                {
                    var msgsV = Connection.connect.Messages
                        .Where(m => m.VacancyId.HasValue
                                 && vacancyIds.Contains(m.VacancyId.Value))
                        .ToList();
                    foreach (var m in msgsV)
                        Connection.connect.Messages.Remove(m);
                }
                catch { }

                try
                {
                    var notifsV = Connection.connect.Notifications
                        .Where(n => n.LinkId.HasValue
                                 && vacancyIds.Contains(n.LinkId.Value))
                        .ToList();
                    foreach (var n in notifsV)
                        Connection.connect.Notifications.Remove(n);
                }
                catch { }

                Connection.connect.SaveChanges();

                foreach (var vid in vacancyIds)
                {
                    try { Skill.SaveVacancySkills(vid, new List<int>()); }
                    catch { }
                }

                Connection.connect.SaveChanges();
            }

            var vacancies = Connection.connect.Vacancies
                .Where(v => v.CompanyId == companyId)
                .ToList();
            foreach (var v in vacancies)
                Connection.connect.Vacancies.Remove(v);

            Connection.connect.SaveChanges();

            try
            {
                var notifs = Connection.connect.Notifications
                    .Where(n => n.CompanyId == companyId)
                    .ToList();
                foreach (var n in notifs)
                    Connection.connect.Notifications.Remove(n);

                Connection.connect.SaveChanges();
            }
            catch { }

            Connection.connect.Companies.Remove(c);
            Connection.connect.SaveChanges();

            if (ownerUserId.HasValue)
            {
                int uid = ownerUserId.Value;

                try
                {
                    var ratingsByUser = Connection.connect.CompanyRatings
                        .Where(r => r.UserId == uid)
                        .ToList();
                    foreach (var r in ratingsByUser)
                        Connection.connect.CompanyRatings.Remove(r);

                    Connection.connect.SaveChanges();
                }
                catch
                {
                    try
                    {
                        Connection.connect.Database.ExecuteSqlCommand(
                            "DELETE FROM CompanyRatings WHERE UserId = {0}", uid);
                    }
                    catch { }
                }

                try
                {
                    var resumes = Connection.connect.Resumes
                        .Where(r => r.UserId == uid)
                        .ToList();
                    foreach (var r in resumes)
                    {
                        try { ResumeStorage.Delete(r.FilePath); } catch { }
                        Connection.connect.Resumes.Remove(r);
                    }

                    Connection.connect.SaveChanges();
                }
                catch
                {
                    try
                    {
                        Connection.connect.Database.ExecuteSqlCommand(
                            "DELETE FROM Resumes WHERE UserId = {0}", uid);
                    }
                    catch { }
                }

                try
                {
                    var sts = Connection.connect.Students
                        .Where(s => s.UserId == uid)
                        .ToList();
                    foreach (var s in sts)
                    {
                        try { s.Skills.Clear(); Connection.connect.SaveChanges(); }
                        catch { }

                        Connection.connect.Students.Remove(s);
                    }

                    Connection.connect.SaveChanges();
                }
                catch
                {
                    try
                    {
                        Connection.connect.Database.ExecuteSqlCommand(
                            "DELETE FROM Students WHERE UserId = {0}", uid);
                    }
                    catch { }
                }

                try
                {
                    var userNotifs = Connection.connect.Notifications
                        .Where(n => n.SenderId == uid)
                        .ToList();
                    foreach (var n in userNotifs)
                        Connection.connect.Notifications.Remove(n);

                    Connection.connect.SaveChanges();
                }
                catch { }

                try
                {
                    var u = Connection.connect.Users
                        .FirstOrDefault(x => x.Id == uid);

                    if (u != null)
                    {
                        Connection.connect.Users.Remove(u);
                        Connection.connect.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        Connection.connect.Database.ExecuteSqlCommand(
                            "DELETE FROM Users WHERE Id = {0}", uid);
                    }
                    catch (Exception ex2)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "User delete failed: " + ex2.Message);
                    }

                    System.Diagnostics.Debug.WriteLine(
                        "User delete (EF) failed: " + ex.Message);
                }
            }

            return true;
        }
    }
}



