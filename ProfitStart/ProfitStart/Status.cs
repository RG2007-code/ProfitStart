using System;

namespace ProfitStart
{
    public static class Status
    {
        
        private const int OnlineSeconds = 60;

        public static bool IsOnline(DateTime? lastSeen)
        {
            if (!lastSeen.HasValue) return false;
            return (DateTime.Now - lastSeen.Value).TotalSeconds < OnlineSeconds;
        }

        public static string Format(DateTime? lastSeen)
        {
            if (!lastSeen.HasValue) return "был(а) давно";

            var diff = DateTime.Now - lastSeen.Value;

            if (diff.TotalSeconds < OnlineSeconds) return "в сети";
            if (diff.TotalMinutes < 60)
                return "был(а) " + (int)diff.TotalMinutes + " мин назад";
            if (diff.TotalHours < 24)
                return "был(а) " + (int)diff.TotalHours + " ч назад";
            if (diff.TotalDays < 2)
                return "был(а) вчера в " + lastSeen.Value.ToString("HH:mm");
            if (diff.TotalDays < 7)
                return "был(а) " + (int)diff.TotalDays + " дн назад";

            return "был(а) " + lastSeen.Value.ToString("dd.MM.yyyy");
        }
    }
}