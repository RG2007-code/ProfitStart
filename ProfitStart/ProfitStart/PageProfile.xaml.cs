using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace ProfitStart
{
    public partial class PageProfile : Page
    {
        public PageProfile()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            if (!Session.IsAuthenticated) return;

            HideAllBlocks();
            LoadAvatar();

            if (Session.IsStudent) LoadStudent();
            else if (Session.IsEmployer) LoadCompany();
            else LoadUser();
        }

        private void HideAllBlocks()
        {
            OgrnBlock.Visibility = Visibility.Collapsed;
            InnBlock.Visibility = Visibility.Collapsed;
            DirBlock.Visibility = Visibility.Collapsed;
            AboutBlock.Visibility = Visibility.Collapsed;
            CourseBlock.Visibility = Visibility.Collapsed;
            StudentCardBlock.Visibility = Visibility.Collapsed;
            DirectionBlock.Visibility = Visibility.Collapsed;
            SkillsBlock.Visibility = Visibility.Collapsed;
            ResumeBlock.Visibility = Visibility.Collapsed;

            if (DisputeBlock != null)
                DisputeBlock.Visibility = Visibility.Collapsed;
        }

      
        private void LoadAvatar()
        {
            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Id == Session.UserId);

            byte[] avatarData = null;
            string email = "";

            if (user != null)
            {
                avatarData = user.AvatarData;
                email = user.Email;
            }

            avatarData = Avatar.GetOrDefault(avatarData, email);

            if (avatarData != null)
                Image.LoadIntoEllipse(AvatarImg, avatarData);
        }

        private void LoadStudent()
        {
            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Id == Session.UserId);
            var s = Connection.connect.Students
                .FirstOrDefault(x => x.Id == Session.StudentId);
            if (s == null) return;

            NameText.Text = s.FullName;
            SubtitleText.Text = "Студент";
            EmailText.Text = user?.Email ?? "—";
            PhoneText.Text = user?.Phone ?? "—";

            CourseBlock.Visibility = Visibility.Visible;
            StudentCardBlock.Visibility = Visibility.Visible;
            DirectionBlock.Visibility = Visibility.Visible;
            SkillsBlock.Visibility = Visibility.Visible;
            ResumeBlock.Visibility = Visibility.Visible;

            CourseText.Text = s.Course + " курс";
            StudentCardText.Text = s.StudentCardNumber;
            DirectionText.Text = s.Direction;
            ResumeText.Text = string.IsNullOrEmpty(s.ResumeText)
                ? "Резюме не заполнено"
                : s.ResumeText;

            SkillsPanel.Children.Clear();
            foreach (var skill in s.Skills)
            {
                var b = new Border
                {
                    Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#EEF2FF")),
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(16, 6, 16, 6),
                    Margin = new Thickness(0, 0, 8, 8)
                };
                b.Child = new TextBlock
                {
                    Text = skill.Name,
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#5B5BF5")),
                    FontSize = 12
                };
                SkillsPanel.Children.Add(b);
            }

            LoadPdfInfo();
        }

        private void LoadPdfInfo()
        {
            var r = ResumeStorage.GetByUser(Session.UserId);

            if (r == null)
            {
                PdfNoText.Visibility = Visibility.Visible;
                PdfInfo.Visibility = Visibility.Collapsed;
                PdfOpenBtn.Visibility = Visibility.Collapsed;
                PdfDeleteBtn.Visibility = Visibility.Collapsed;
                PdfUploadBtn.Content = "Загрузить PDF";
                return;
            }

            PdfNoText.Visibility = Visibility.Collapsed;
            PdfInfo.Visibility = Visibility.Visible;
            PdfOpenBtn.Visibility = Visibility.Visible;
            PdfDeleteBtn.Visibility = Visibility.Visible;
            PdfUploadBtn.Content = "Заменить PDF";

            PdfNameText.Text = r.FileName;
            PdfDateText.Text = "Загружено: "
                + (r.UploadedAt.HasValue
                    ? r.UploadedAt.Value.ToString("dd.MM.yyyy HH:mm")
                    : "—");
        }

        private void PdfUpload_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Выберите резюме (PDF)",
                Filter = "PDF-файлы (*.pdf)|*.pdf"
            };

            if (dlg.ShowDialog() != true) return;

            var fi = new FileInfo(dlg.FileName);
            if (fi.Length > 10 * 1024 * 1024)
            {
                AppMessage.Error(Window.GetWindow(this), "Файл больше 10 МБ");
                return;
            }

            try
            {
                var old = ResumeStorage.GetByUser(Session.UserId);
                if (old != null)
                {
                    ResumeStorage.Delete(old.FilePath);
                    Connection.connect.Resumes.Remove(old);
                    Connection.connect.SaveChanges();
                }

                string savedPath = ResumeStorage.Save(dlg.FileName, Session.UserId);

                var resume = new Resumes
                {
                    UserId = Session.UserId,
                    FileName = fi.Name,
                    FilePath = savedPath,
                    UploadedAt = DateTime.Now
                };
                Connection.connect.Resumes.Add(resume);
                Connection.connect.SaveChanges();

                AppMessage.Success(Window.GetWindow(this), "Резюме загружено");
                LoadPdfInfo();
            }
            catch (Exception ex)
            {
                AppMessage.Error(Window.GetWindow(this),
                    "Ошибка загрузки: " + ex.Message);
            }
        }

        private void PdfOpen_Click(object sender, RoutedEventArgs e)
        {
            var r = ResumeStorage.GetByUser(Session.UserId);
            if (r == null) return;

            if (!File.Exists(r.FilePath))
            {
                AppMessage.Error(Window.GetWindow(this),
                    "Файл не найден на диске");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(r.FilePath)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                AppMessage.Error(Window.GetWindow(this),
                    "Не удалось открыть: " + ex.Message);
            }
        }

        private void PdfDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!AppMessage.Confirm(Window.GetWindow(this),
                "Удалить PDF-резюме?", "Удаление"))
                return;

            var r = ResumeStorage.GetByUser(Session.UserId);
            if (r == null) return;

            ResumeStorage.Delete(r.FilePath);
            Connection.connect.Resumes.Remove(r);
            Connection.connect.SaveChanges();

            AppMessage.Success(Window.GetWindow(this), "Резюме удалено");
            LoadPdfInfo();
        }

        private void LoadCompany()
        {
            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Id == Session.UserId);
            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == Session.CompanyId);
            if (c == null) return;

            NameText.Text = c.Name;
            SubtitleText.Text = "Компания";
            EmailText.Text = user?.Email ?? "—";
            PhoneText.Text = user?.Phone ?? "—";

            OgrnBlock.Visibility = Visibility.Visible;
            InnBlock.Visibility = Visibility.Visible;
            DirBlock.Visibility = Visibility.Visible;
            AboutBlock.Visibility = Visibility.Visible;

            OgrnText.Text = c.OGRN ?? "—";
            InnText.Text = c.INN ?? "—";
            DirText.Text = c.Direction ?? "—";
            AboutText.Text = c.Description ?? "—";

            if (c.IsDeleted == true)
            {
                DisputeBlock.Visibility = Visibility.Visible;
                EditProfileBtn.Visibility = Visibility.Collapsed;

                DisputeReasonText.Text = "Причина: "
                    + (c.RejectionReason ?? "не указана");

                if (c.DisputeRequested == true)
                    DisputeAlreadySent.Visibility = Visibility.Visible;
                else
                    DisputeAlreadySent.Visibility = Visibility.Collapsed;
            }
            else
            {
                DisputeBlock.Visibility = Visibility.Collapsed;
                EditProfileBtn.Visibility = Visibility.Visible;
            }
        }

        private void LoadUser()
        {
            var user = Connection.connect.Users
                .FirstOrDefault(u => u.Id == Session.UserId);
            if (user == null) return;

            NameText.Text = user.Email;
            SubtitleText.Text = user.Roles.Code;
            EmailText.Text = user.Email;
            PhoneText.Text = user.Phone ?? "—";
        }

        private void Dispute_Click(object sender, RoutedEventArgs e)
        {
            var c = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == Session.CompanyId);
            if (c == null) return;

            if (c.DisputeRequested == true)
            {
                AppMessage.Info(Window.GetWindow(this),
                    "Оспаривание уже отправлено. Дождитесь ответа админа.");
                return;
            }

            var wnd = new DisputeWindow(c.DisputeMessage ?? "");
            wnd.Owner = Window.GetWindow(this);

            if (wnd.ShowDialog() == true)
            {
                bool ok = CompanyDispute.Submit(Session.CompanyId, wnd.Message);
                if (ok)
                {
                    AppMessage.Success(Window.GetWindow(this),
                        "Оспаривание отправлено админу");
                    Load();
                }
                else
                {
                    AppMessage.Error(Window.GetWindow(this),
                        "Не удалось отправить оспаривание");
                }
            }
        }

        private void DeleteForever_Click(object sender, RoutedEventArgs e)
        {
            if (!AppMessage.Confirm(Window.GetWindow(this),
                "Удалить компанию окончательно?\n"
                + "Все вакансии, отклики, сообщения и уведомления будут удалены "
                + "без возможности восстановления.",
                "Окончательное удаление"))
                return;

            bool ok = CompanyDispute.DeleteForever(Session.CompanyId);
            if (ok)
            {
                AppMessage.Success(Window.GetWindow(this), "Компания удалена");

                Session.CompanyId = 0;
                Session.UserId = 0;
                Session.Phone = null;
                Session.RoleCode = null;

                var main = Window.GetWindow(this) as MainWindow;

                var login = new LoginWindow();
                login.Show();

                if (main != null) main.Close();
                else Window.GetWindow(this)?.Close();
            }
            else
            {
                AppMessage.Error(Window.GetWindow(this),
                    "Не удалось удалить компанию");
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsAuthenticated)
            {
                AppMessage.Warning(Window.GetWindow(this), "Сначала войдите");
                return;
            }
            NavigationService.Navigate(new PageEditProfile());
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (!Session.IsAuthenticated)
            {
                AppMessage.Warning(Window.GetWindow(this), "Сначала войдите");
                return;
            }

            var wnd = new ChangePasswordWindow();
            wnd.Owner = Window.GetWindow(this);
            wnd.ShowDialog();
        }
    }
}