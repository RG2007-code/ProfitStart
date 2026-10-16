namespace ProfitStart
{
    public static class StatusText
    {
        public static string Ru(string status)
        {
            if (string.IsNullOrEmpty(status)) return "—";

            switch (status.Trim())
            {
                case "Sent":
                case "Отправлен":
                    return "Отправлен";

                case "Approved":
                case "Одобрен":
                    return "Одобрен";

                case "Rejected":
                case "Отказ":
                case "Отклонён":
                    return "Отклонён";

                case "Interview":
                case "Интервью":
                    return "Интервью";

                case "Offer":
                case "Оффер":
                    return "Оффер";

                default:
                    return status;
            }
        }
    }
}