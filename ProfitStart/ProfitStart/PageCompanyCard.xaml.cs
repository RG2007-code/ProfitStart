using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageCompanyCard : Page, IRefreshablePage
    {
        private int _companyId;
        private List<CompanyReview> _allReviews = new List<CompanyReview>();

        public PageCompanyCard() { InitializeComponent(); }

        public PageCompanyCard(int companyId)
        {
            InitializeComponent();
            _companyId = companyId;
            Load();
        }

        public void RefreshAfterThemeChange() { }

        private void Load()
        {
            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == _companyId);
            if (c == null) return;

            NameText.Text = c.Name;
            DirText.Text = c.Direction;
            OgrnText.Text = c.OGRN;
            DescText.Text = c.Description;

            RefreshRating();
            LoadReviews();

            if (c.IsDeleted == true)
            {
                DeletedBanner.Visibility = Visibility.Visible;
                DeletedReason.Text = "Причина: " + (c.RejectionReason ?? "—");
            }

            byte[] avatar = c.AvatarData;
            if ((avatar == null || avatar.Length == 0) && c.UserId.HasValue)
            {
                var owner = Connection.connect.Users
                    .FirstOrDefault(u => u.Id == c.UserId.Value);
                if (owner != null) avatar = owner.AvatarData;
            }
            if (avatar != null && avatar.Length > 0)
                Image.LoadIntoEllipse(AvatarBig, avatar);

            var vacancies = Connection.connect.Vacancies
                .Where(v => v.CompanyId == _companyId && v.IsApproved == true)
                .OrderByDescending(v => v.CreatedAt)
                .ToList();

            var list = vacancies.Select(v => new
            {
                v.Id,
                v.Title,
                Subtitle = v.Description,
                v.Type,
                v.AvatarData
            }).ToList();

            RequestsList.ItemsSource = list;
            EmptyText.Visibility = list.Any()
                ? Visibility.Collapsed : Visibility.Visible;

            ContentFilter_Changed(null, null);
            ApplyStarsFilter();
        }

        private void RefreshRating()
        {
            double avg = CompanyRating.GetAverage(_companyId);
            int count = CompanyRating.GetCount(_companyId);

            int fullStars = (int)Math.Round(avg);
            if (fullStars > 5) fullStars = 5;
            if (fullStars < 0) fullStars = 0;

            StarsText.Text = new string('★', fullStars)
                           + new string('☆', 5 - fullStars);

            RatingText.Text = count == 0
                ? "Нет оценок"
                : avg.ToString("0.0", System.Globalization.CultureInfo.CurrentCulture)
                  + " / 5 · " + count + " " + Plural(count);

            var company = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == _companyId);

            bool canRate = Session.IsAuthenticated
                        && Session.IsStudent
                        && company != null
                        && company.UserId != Session.UserId;

            if (!canRate)
            {
                RateBlock.Visibility = Visibility.Collapsed;
                return;
            }

            RateBlock.Visibility = Visibility.Visible;

            int my = CompanyRating.GetMyRating(_companyId, Session.UserId);

            var stars = new[] { Star1, Star2, Star3, Star4, Star5 };
            for (int i = 0; i < 5; i++)
                stars[i].Content = (i < my) ? "★" : "☆";

            if (my > 0)
            {
                MyRatingText.Text = "Ваша оценка: " + my + " из 5";
                MyRatingText.Visibility = Visibility.Visible;
                RateHint.Text = "Изменить оценку:";
            }
            else
            {
                MyRatingText.Visibility = Visibility.Collapsed;
                RateHint.Text = "Поставьте оценку:";
            }
        }

        private void LoadReviews()
        {
            _allReviews = CompanyRating.GetReviews(_companyId, 100);
            ApplyStarsFilter();
        }

        private void ApplyStarsFilter()
        {
            if (ReviewsList == null) return;

            int starFilter = GetStarsFilter();

            var filtered = starFilter == 0
                ? _allReviews
                : _allReviews.Where(r => r.Value == starFilter).ToList();

            bool isOwner = (Session.IsEmployer && Session.CompanyId == _companyId);

            var display = filtered.Select(r => new
            {
                r.Id,
                r.UserName,
                r.UserAvatar,
                r.Stars,
                r.DateText,
                r.Comment,
                r.CompanyReply,
                r.ReplyVisibility,

                ReplyButtonVisibility = (isOwner && string.IsNullOrEmpty(r.CompanyReply))
                    ? Visibility.Visible
                    : Visibility.Collapsed,

                EditReplyButtonVisibility = (isOwner && !string.IsNullOrEmpty(r.CompanyReply))
                    ? Visibility.Visible
                    : Visibility.Collapsed,

                DeleteVisibility = (Session.IsStudent
                                    && r.UserId == Session.UserId)
                                    ? Visibility.Visible
                                    : Visibility.Collapsed,

                AdminDeleteVisibility = Session.IsAdmin
                                    ? Visibility.Visible
                                    : Visibility.Collapsed
            }).ToList();

            ReviewsList.ItemsSource = display;
            NoReviewsText.Visibility = display.Any()
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private int GetStarsFilter()
        {
            var sel = (StarsFilter?.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "Все оценки";

            switch (sel)
            {
                case "5 звёзд": return 5;
                case "4 звезды": return 4;
                case "3 звезды": return 3;
                case "2 звезды": return 2;
                case "1 звезда": return 1;
                default: return 0;
            }
        }

        private void StarsFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyStarsFilter();
        }

        private void ContentFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (ReviewsBlock == null || VacanciesBlock == null) return;

            var filter = (ContentFilter?.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "Всё";

            ReviewsBlock.Visibility = (filter == "Только вакансии")
                ? Visibility.Collapsed : Visibility.Visible;
            VacanciesBlock.Visibility = (filter == "Только отзывы")
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private static string Plural(int n)
        {
            int mod10 = n % 10;
            int mod100 = n % 100;
            if (mod10 == 1 && mod100 != 11) return "оценка";
            if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20))
                return "оценки";
            return "оценок";
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsAuthenticated || !Session.IsStudent)
            {
                AppMessage.Warning(Window.GetWindow(this),
                    "Оценивать компании могут только студенты");
                return;
            }

            int value = int.Parse(((Button)sender).Tag.ToString());
            CompanyRating.Set(_companyId, Session.UserId, value);

            var wnd = new CompanyReviewWindow(_companyId, Session.UserId, value);
            wnd.Owner = Window.GetWindow(this);
            wnd.ShowDialog();

            RefreshRating();
            LoadReviews();
        }

        private void ReviewAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void DeleteReview_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;

            if (!AppMessage.Confirm(Window.GetWindow(this),
                "Удалить свой отзыв?", "Удаление"))
                return;

            bool ok = CompanyRating.Delete(id, Session.UserId);
            if (ok)
            {
                AppMessage.Success(Window.GetWindow(this), "Отзыв удалён");
                RefreshRating();
                LoadReviews();
            }
            else
            {
                AppMessage.Error(Window.GetWindow(this), "Не удалось удалить");
            }
        }

        private void AdminDelete_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;

            var wnd = new RejectReasonWindow("Укажите причину удаления отзыва:");
            wnd.Owner = Window.GetWindow(this);

            if (wnd.ShowDialog() != true) return;

            string reason = wnd.Reason;

            var r = Connection.connect.CompanyRatings
                .FirstOrDefault(x => x.Id == id);
            if (r == null) return;

            int authorUserId = r.UserId;
            int companyId = r.CompanyId;

            CompanyRating.DeleteByAdmin(id);

            var st = Connection.connect.Students
                .FirstOrDefault(s => s.UserId == authorUserId);

            if (st != null)
            {
                Notify.ToStudent(st.Id,
                    "Отзыв удалён",
                    "Ваш отзыв о компании удалён администратором. Причина: " + reason,
                    "company", companyId);
            }

            AppMessage.Success(Window.GetWindow(this), "Отзыв удалён");

            RefreshRating();
            LoadReviews();
        }

        private void Reply_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;

            var wnd = new MessageDialogWindow("Ваш ответ на отзыв:");
            wnd.Owner = Window.GetWindow(this);

            if (wnd.ShowDialog() == true)
            {
                CompanyRating.SetCompanyReply(id, wnd.Message);
                AppMessage.Success(Window.GetWindow(this), "Ответ добавлен");
                LoadReviews();
            }
        }

        private void EditReply_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;

            var wnd = new MessageDialogWindow("Изменить ответ на отзыв:");
            wnd.Owner = Window.GetWindow(this);

            if (wnd.ShowDialog() == true)
            {
                CompanyRating.SetCompanyReply(id, wnd.Message);
                AppMessage.Success(Window.GetWindow(this), "Ответ обновлён");
                LoadReviews();
            }
        }

        private void DeleteReply_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;

            if (!AppMessage.Confirm(Window.GetWindow(this),
                "Удалить ответ компании?", "Удаление ответа"))
                return;

            CompanyRating.DeleteCompanyReply(id);
            AppMessage.Success(Window.GetWindow(this), "Ответ удалён");
            LoadReviews();
        }

        private void VacancyAvatar_Loaded(object sender, RoutedEventArgs e)
        {
            var ellipse = sender as System.Windows.Shapes.Ellipse;
            if (ellipse == null) return;
            var data = ellipse.Tag as byte[];
            if (data != null && data.Length > 0)
                Image.LoadIntoEllipse(ellipse, data);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Request_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;
            int id = (int)border.Tag;
            NavigationService.Navigate(new PageVacancyCard(id));
        }
    }
}