using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace ProfitStart
{
    public partial class PageResume : Page
    {
        private readonly int _userId;

        public PageResume(int userId)
        {
            InitializeComponent();
            _userId = userId;
            Load();
        }

        private void Load()
        {
            var r = ResumeStorage.GetByUser(_userId);

            if (r == null)
            {
                NoResumeText.Visibility = Visibility.Visible;
                ResumeInfo.Visibility = Visibility.Collapsed;
                OpenBtn.Visibility = Visibility.Collapsed;
                DeleteBtn.Visibility = Visibility.Collapsed;
                UploadBtn.Content = "Загрузить PDF";
                return;
            }

            NoResumeText.Visibility = Visibility.Collapsed;
            ResumeInfo.Visibility = Visibility.Visible;
            OpenBtn.Visibility = Visibility.Visible;
            DeleteBtn.Visibility = Visibility.Visible;
            UploadBtn.Content = "Заменить PDF";

            FileNameText.Text = r.FileName;
            UploadedAtText.Text = "Загружено: "
                + (r.UploadedAt.HasValue
                    ? r.UploadedAt.Value.ToString("dd.MM.yyyy HH:mm")
                    : "—");
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
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
                AppMessage.Error(Window.GetWindow(this),
                    "Файл больше 10 МБ");
                return;
            }

            try
            {
                
                var old = ResumeStorage.GetByUser(_userId);
                if (old != null)
                {
                    ResumeStorage.Delete(old.FilePath);
                    Connection.connect.Resumes.Remove(old);
                    Connection.connect.SaveChanges();
                }

                string savedPath = ResumeStorage.Save(dlg.FileName, _userId);

                var resume = new Resumes
                {
                    UserId = _userId,
                    FileName = fi.Name,
                    FilePath = savedPath,
                    UploadedAt = DateTime.Now
                };
                Connection.connect.Resumes.Add(resume);
                Connection.connect.SaveChanges();

                AppMessage.Success(Window.GetWindow(this), "Резюме загружено");
                Load();
            }
            catch (Exception ex)
            {
                AppMessage.Error(Window.GetWindow(this),
                    "Ошибка загрузки: " + ex.Message);
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var r = ResumeStorage.GetByUser(_userId);
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

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!AppMessage.Confirm(Window.GetWindow(this),
                "Удалить резюме?", "Удаление"))
                return;

            var r = ResumeStorage.GetByUser(_userId);
            if (r == null) return;

            ResumeStorage.Delete(r.FilePath);
            Connection.connect.Resumes.Remove(r);
            Connection.connect.SaveChanges();

            AppMessage.Success(Window.GetWindow(this), "Резюме удалено");
            Load();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}