using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitStart
{
    public static class BannedUsers
    {
        public static List<string> StandardReasons = new List<string>
        {
            "Нарушение правил платформы",
            "Фейковый аккаунт",
            "Оскорбительное поведение",
            "Спам и реклама",
            "Мошенничество",
            "Недостоверная информация в профиле",
            "Другое (введите вручную)"
        };

        public static bool IsBanned(string email)
        {
            var ban = Connection.connect.Bans
                .FirstOrDefault(b => b.Email == email);

            if (ban == null) return false;

            if (ban.BanUntil.HasValue && ban.BanUntil.Value < DateTime.Now)
            {
                Unban(ban.Id);
                return false;
            }

            return true;
        }

        public static void Ban(int userId, string reason, string reasonType,
            DateTime? until)
        {
            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Id == userId);
            if (user == null) return;

            if (Connection.connect.Bans.Any(b => b.UserId == user.Id))
                return;

            var ban = new Bans
            {
                Email = user.Email,
                Phone = user.Phone,
                PasswordHash = user.PasswordHash,
                RoleId = user.RoleId,
                Reason = reason,
                ReasonType = reasonType,
                BanUntil = until,
                UserId = user.Id,
                BannedAt = DateTime.Now
            };
            Connection.connect.Bans.Add(ban);

            user.IsActive = false;

            Connection.connect.SaveChanges();
        }

        public static void Unban(int banId)
        {
            var ban = Connection.connect.Bans
                .FirstOrDefault(b => b.Id == banId);
            if (ban == null) return;

            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Id == ban.UserId);

            if (user != null)
                user.IsActive = true;

            Connection.connect.Bans.Remove(ban);
            Connection.connect.SaveChanges();
        }
    }
}