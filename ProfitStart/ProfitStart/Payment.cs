using System;

namespace ProfitStart
{
    
    public static class Payment
    {
        public const string Hour = "hour";
        public const string Month = "month";

        
        public static string GetDisplay(string unit)
        {
            switch (unit)
            {
                case Hour: return "рублей/час";
                case Month: return "рублей/месяц";
                default: return "";
            }
        }

        
        public static string GetCode(string display)
        {
            if (string.IsNullOrEmpty(display)) return Hour;
            if (display.Contains("час")) return Hour;
            if (display.Contains("месяц")) return Month;
            return Hour;
        }

        
        public static string Format(string salary, string unit)
        {
            if (string.IsNullOrWhiteSpace(salary)) return "";

            
            if (salary.Contains("руб") || salary.Contains("₽"))
                return salary;

            string suffix = unit == Month ? " ₽ / месяц"
                          : unit == Hour ? " ₽ / час"
                          : "";
            return salary.Trim() + suffix;
        }

        
        public static int ParseSalary(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;

            var digits = "";
            foreach (var c in s)
                if (char.IsDigit(c)) digits += c;

            return int.TryParse(digits, out int result) ? result : 0;
        }

       
        public static string ByVacancyType(string type)
        {
            if (type == "Стажировка" || type == "Практика") return Hour;
            return Month;
        }
    }
}