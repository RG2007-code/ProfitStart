using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitStart
{
    
    public static class UserDelete
    {
        public static bool DeleteForever(int userId)
        {
            var u = Connection.connect.Users
                .FirstOrDefault(x => x.Id == userId);
            if (u == null) return false;

            
            var st = Connection.connect.Students
                .FirstOrDefault(s => s.UserId == userId);
            var cmp = Connection.connect.Companies
                .FirstOrDefault(c => c.UserId == userId);

            int? studentId = st?.Id;
            int? companyId = cmp?.Id;

           
            if (studentId.HasValue)
            {
                int sid = studentId.Value;

               
                try
                {
                    var msgs = Connection.connect.Messages
                        .Where(m => m.StudentId == sid)
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
                            "DELETE FROM Messages WHERE StudentId = {0}", sid);
                    }
                    catch { }
                }

                
                try
                {
                    Connection.connect.Database.ExecuteSqlCommand(
                        "DELETE FROM StudentSkills WHERE StudentId = {0}", sid);
                }
                catch { }

                try
                {
                    if (st != null && st.Skills != null)
                    {
                        st.Skills.Clear();
                        Connection.connect.SaveChanges();
                    }
                }
                catch { }

                
                try
                {
                    Connection.connect.Database.ExecuteSqlCommand(
                        "DELETE FROM Applications WHERE StudentId = {0}", sid);
                }
                catch { }

                try
                {
                    var apps = Connection.connect.Applications
                        .Where(a => a.StudentId == sid)
                        .ToList();
                    foreach (var a in apps)
                        Connection.connect.Applications.Remove(a);

                    Connection.connect.SaveChanges();
                }
                catch { }

                
                try
                {
                    var notifs = Connection.connect.Notifications
                        .Where(n => n.StudentId == sid)
                        .ToList();
                    foreach (var n in notifs)
                        Connection.connect.Notifications.Remove(n);

                    Connection.connect.SaveChanges();
                }
                catch
                {
                    try
                    {
                        Connection.connect.Database.ExecuteSqlCommand(
                            "DELETE FROM Notifications WHERE StudentId = {0}", sid);
                    }
                    catch { }
                }

                
                try
                {
                    var apps = Connection.connect.CompanyApplications
                        .Where(a => a.StudentId == sid)
                        .ToList();
                    foreach (var a in apps)
                        Connection.connect.CompanyApplications.Remove(a);

                    Connection.connect.SaveChanges();
                }
                catch
                {
                    try
                    {
                        Connection.connect.Database.ExecuteSqlCommand(
                            "DELETE FROM CompanyApplications WHERE StudentId = {0}", sid);
                    }
                    catch { }
                }
            }

            
            if (companyId.HasValue)
            {
                int cid = companyId.Value;

               
                var vacancyIds = Connection.connect.Vacancies
                    .Where(v => v.CompanyId == cid)
                    .Select(v => v.Id)
                    .ToList();

                if (vacancyIds.Count > 0)
                {
                    
                    try
                    {
                        var vapps = Connection.connect.CompanyApplications
                            .Where(a => vacancyIds.Contains(a.VacancyId))
                            .ToList();
                        foreach (var a in vapps)
                            Connection.connect.CompanyApplications.Remove(a);

                        Connection.connect.SaveChanges();
                    }
                    catch { }

                    
                    foreach (var vid in vacancyIds)
                    {
                        try { Skill.SaveVacancySkills(vid, new List<int>()); }
                        catch { }
                    }

                    
                    try
                    {
                        var vnotifs = Connection.connect.Notifications
                            .Where(n => n.LinkId.HasValue
                                     && vacancyIds.Contains(n.LinkId.Value))
                            .ToList();
                        foreach (var n in vnotifs)
                            Connection.connect.Notifications.Remove(n);

                        Connection.connect.SaveChanges();
                    }
                    catch { }
                }

                
                try
                {
                    var vacs = Connection.connect.Vacancies
                        .Where(v => v.CompanyId == cid)
                        .ToList();
                    foreach (var v in vacs)
                        Connection.connect.Vacancies.Remove(v);

                    Connection.connect.SaveChanges();
                }
                catch
                {
                    try
                    {
                        Connection.connect.Database.ExecuteSqlCommand(
                            "DELETE FROM Vacancies WHERE CompanyId = {0}", cid);
                    }
                    catch { }
                }

                
                try
                {
                    var ratings = Connection.connect.CompanyRatings
                        .Where(r => r.CompanyId == cid)
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
                            "DELETE FROM CompanyRatings WHERE CompanyId = {0}", cid);
                    }
                    catch { }
                }

                
                try
                {
                    var msgs = Connection.connect.Messages
                        .Where(m => m.CompanyId == cid)
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
                            "DELETE FROM Messages WHERE CompanyId = {0}", cid);
                    }
                    catch { }
                }

               
                try
                {
                    var notifs = Connection.connect.Notifications
                        .Where(n => n.CompanyId == cid)
                        .ToList();
                    foreach (var n in notifs)
                        Connection.connect.Notifications.Remove(n);

                    Connection.connect.SaveChanges();
                }
                catch { }
            }

            
            try
            {
                var ratings = Connection.connect.CompanyRatings
                    .Where(r => r.UserId == userId)
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
                        "DELETE FROM CompanyRatings WHERE UserId = {0}", userId);
                }
                catch { }
            }

            
            try
            {
                var resumes = Connection.connect.Resumes
                    .Where(r => r.UserId == userId)
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
                        "DELETE FROM Resumes WHERE UserId = {0}", userId);
                }
                catch { }
            }

            
            if (studentId.HasValue)
            {
                try
                {
                    var st2 = Connection.connect.Students
                        .FirstOrDefault(s => s.Id == studentId.Value);
                    if (st2 != null)
                    {
                        Connection.connect.Students.Remove(st2);
                        Connection.connect.SaveChanges();
                    }
                }
                catch
                {
                    try
                    {
                        Connection.connect.Database.ExecuteSqlCommand(
                            "DELETE FROM Students WHERE Id = {0}", studentId.Value);
                    }
                    catch { }
                }
            }

            
            try
            {
                var userNotifs = Connection.connect.Notifications
                    .Where(n => n.SenderId == userId)
                    .ToList();
                foreach (var n in userNotifs)
                    Connection.connect.Notifications.Remove(n);

                Connection.connect.SaveChanges();
            }
            catch { }

            
            if (companyId.HasValue)
            {
                try
                {
                    var cmp2 = Connection.connect.Companies
                        .FirstOrDefault(c => c.Id == companyId.Value);
                    if (cmp2 != null)
                    {
                        Connection.connect.Companies.Remove(cmp2);
                        Connection.connect.SaveChanges();
                    }
                }
                catch
                {
                    try
                    {
                        Connection.connect.Database.ExecuteSqlCommand(
                            "DELETE FROM Companies WHERE Id = {0}", companyId.Value);
                    }
                    catch { }
                }
            }

            
            try
            {
                Connection.connect.Users.Remove(u);
                Connection.connect.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    Connection.connect.Database.ExecuteSqlCommand(
                        "DELETE FROM Users WHERE Id = {0}", userId);
                    return true;
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "User delete failed: " + ex2.Message);
                    System.Diagnostics.Debug.WriteLine(
                        "EF-путь: " + ex.Message);
                    return false;
                }
            }
        }
    }
}