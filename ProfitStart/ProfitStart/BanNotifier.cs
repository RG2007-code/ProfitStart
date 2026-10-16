using System;
using System.Linq;
using System.Windows;

namespace ProfitStart
{
    
    public static class BanNotifier
    {
       
        public static string TakeExpiredNotice(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var ban = Connection.connect.Bans
                .FirstOrDefault(b => b.Email == email);
            if (ban == null) return null;

           
            if (!ban.BanUntil.HasValue) return null;       
            if (ban.BanUntil.Value > DateTime.Now) return null;

            
            if (ban.NotifiedAt.HasValue) return null;

           
            ban.NotifiedAt = DateTime.Now;

            
            BannedUsers.Unban(ban.Id);

            string untilText = ban.BanUntil.Value.ToString("dd.MM.yyyy");
            return "Ваш бан истёк " + untilText + ".\n"
                 + "Доступ к платформе восстановлен.";
        }

        
        public static System.Collections.Generic.List<Bans> GetExpiredBans()
        {
            var now = DateTime.Now;
            return Connection.connect.Bans
                .Where(b => b.BanUntil.HasValue && b.BanUntil.Value < now)
                .ToList();
        }

        
        public static void ShowOnLogin(Window owner, string email)
        {
            string msg = TakeExpiredNotice(email);
            if (string.IsNullOrEmpty(msg)) return;

            AppMessage.Info(owner, msg);
        }
    }
}