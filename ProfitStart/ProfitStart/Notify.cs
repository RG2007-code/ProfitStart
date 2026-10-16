using System;

namespace ProfitStart
{
    public static class Notify
    {
        public static void ToStudent(int studentId, string title,
            string message, string page, int linkId)
        {
            using (var ctx = new StudWorkEntities1())
            {
                var n = new Notifications
                {
                    StudentId = studentId,
                    RecipientType = "Student",
                    Title = title,
                    Message = message,
                    LinkPage = page,
                    LinkId = linkId,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    SenderType = "System"
                };
                ctx.Notifications.Add(n);
                ctx.SaveChanges();
            }
        }

        public static void ToCompany(int companyId, string title,
            string message, string page, int linkId,
            int? senderId = null)
        {
            using (var ctx = new StudWorkEntities1())
            {
                var n = new Notifications
                {
                    CompanyId = companyId,
                    RecipientType = "Company",
                    Title = title,
                    Message = message,
                    LinkPage = page,
                    LinkId = linkId,
                    SenderId = senderId,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    SenderType = "System"
                };
                ctx.Notifications.Add(n);
                ctx.SaveChanges();
            }
        }

        public static void ToAdmin(string title, string message,
            string page, int linkId)
        {
            using (var ctx = new StudWorkEntities1())
            {
                var n = new Notifications
                {
                    RecipientType = "Admin",
                    Title = title,
                    Message = message,
                    LinkPage = page,
                    LinkId = linkId,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    SenderType = "System"
                };
                ctx.Notifications.Add(n);
                ctx.SaveChanges();
            }
        }

        public static void SendMessage(int studentId, int companyId,
            int? vacancyId, string senderType, string text)
        {
            using (var ctx = new StudWorkEntities1())
            {
                var m = new Messages
                {
                    StudentId = studentId,
                    CompanyId = companyId,
                    VacancyId = vacancyId,
                    SenderType = senderType,
                    Text = text,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                ctx.Messages.Add(m);
                ctx.SaveChanges();
            }
        }
    }
}