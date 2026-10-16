using System;
using System.Linq;
using System.Windows;

namespace ProfitStart
{
    public partial class CompanyReviewWindow : Window
    {
        private readonly int _companyId;
        private readonly int _userId;
        private readonly int _value;

        public CompanyReviewWindow(int companyId, int userId, int value)
        {
            InitializeComponent();
            _companyId = companyId;
            _userId = userId;
            _value = value;

            StarsText.Text = new string('★', value)
                           + new string('☆', 5 - value);
            ValueText.Text = value + " из 5";

            var existing = Connection.connect.CompanyRatings
                .FirstOrDefault(r => r.CompanyId == companyId
                                  && r.UserId == userId);

            if (existing != null
                && !string.IsNullOrWhiteSpace(existing.Comment))
            {
                CommentBox.Text = existing.Comment;
            }

            Loaded += (s, e) =>
            {
                Theme.ApplyToWindow(this, AppTheme.IsDark);
            };
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
           
            CompanyRating.SetComment(_companyId, _userId, CommentBox.Text);

            
            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == _companyId);

            Notify.ToAdmin("Новый отзыв на модерации",
                "Отзыв на компанию «"
                + (c != null ? c.Name : "—")
                + "» ждёт проверки",
                "modreviews", _companyId);

            AppMessage.Success(this,
                "Спасибо! Отзыв отправлен на модерацию.");
            DialogResult = true;
            Close();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}