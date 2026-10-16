using System;
using System.Linq;

namespace ProfitStart
{
    public static class Session
    {
        public static int UserId { get; set; }
        public static string Phone { get; set; }
        public static string RoleCode { get; set; }
        public static int StudentId { get; set; }
        public static int CompanyId { get; set; }

        public static bool IsAuthenticated => UserId != 0;
        public static bool IsStudent => RoleCode == "Student";
        public static bool IsEmployer => RoleCode == "Employer";
        public static bool IsCareer => RoleCode == "CareerCenter";
        public static bool IsAdmin => RoleCode == "Admin";

        
        public static void Touch()
        {
            if (!IsAuthenticated) return;

            try
            {
                var user = Connection.connect.Users
                    .FirstOrDefault(u => u.Id == UserId);
                if (user == null) return;

                user.LastSeenAt = DateTime.Now;
                Connection.connect.SaveChanges();
            }
            catch { /* игнорируем — не критично */ }
        }
    }
}