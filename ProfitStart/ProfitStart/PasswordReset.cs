using System;
using System.Linq;
using System.Text;

namespace ProfitStart
{
    public static class PasswordReset
    {
        
        public static string GenerateTempPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rnd = new Random();
            var sb = new StringBuilder();

            for (int i = 0; i < 6; i++)
                sb.Append(chars[rnd.Next(chars.Length)]);

            return "temp-" + sb.ToString();
        }

        
        public static (bool ok, string result) ResetByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return (false, "Введите телефон");

            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Phone == phone);

            if (user == null)
                return (false, "Пользователь с таким телефоном не найден");

            if (user.IsActive == false)
                return (false, "Аккаунт заблокирован. Обратитесь к администратору");

            string temp = GenerateTempPassword();

            user.PasswordHash = temp;
            Connection.connect.SaveChanges();

            return (true, temp);
        }

        
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(
                phone.Trim(), @"^\+?[0-9]{10,12}$");
        }
    }
}