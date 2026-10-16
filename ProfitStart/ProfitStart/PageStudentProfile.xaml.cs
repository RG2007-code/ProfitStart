using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProfitStart
{
    public partial class PageStudentProfile : Page
    {
        private int _studentId;
        private int _studentUserId = 0;

        public PageStudentProfile() { InitializeComponent(); }

        public PageStudentProfile(int studentId)
        {
            InitializeComponent();
            _studentId = studentId;
            Load();
        }

        private void Load()
        {
            var s = Connection.connect.Students
                .FirstOrDefault(x => x.Id == _studentId);
            if (s == null) return;

            NameText.Text = s.FullName;
            SubtitleText.Text = "Студент · " + s.Course + " курс";
            DirText.Text = s.Direction;

            if (s.UserId.HasValue)
            {
                _studentUserId = s.UserId.Value;

                var u = Connection.connect.Users
                    .FirstOrDefault(x => x.Id == s.UserId.Value);

                if (u != null && u.AvatarData != null
                    && u.AvatarData.Length > 0)
                {
                    Image.LoadIntoEllipse(AvatarBig, u.AvatarData);
                }
            }

           
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

            
            if (!string.IsNullOrEmpty(s.ResumeText))
                ResumeText.Text = s.ResumeText;
            else
                ResumeText.Text = "Резюме не заполнено";

            
            LoadPdfInfo();

           
            if (s.PassportData != null && s.PassportData.Length > 0)
            {
                Image.LoadIntoBorderFull(PassportPreview, s.PassportData);
                PassportHint.Visibility = Visibility.Collapsed;
            }

            
            if (s.IsGraduated == true && s.DiplomaData != null
                && s.DiplomaData.Length > 0)
            {
                DiplomaBlock.Visibility = Visibility.Visible;
                Image.LoadIntoBorderFull(DiplomaPreview, s.DiplomaData);
                DiplomaHint.Visibility = Visibility.Collapsed;
            }
        }

        
        private void LoadPdfInfo()
        {
            if (PdfNoText == null) return;

            if (_studentUserId == 0)
            {
                PdfNoText.Visibility = Visibility.Visible;
                PdfInfo.Visibility = Visibility.Collapsed;
                PdfOpenBtn.Visibility = Visibility.Collapsed;
                PdfSaveBtn.Visibility = Visibility.Collapsed;
                return;
            }

            var r = ResumeStorage.GetByUser(_studentUserId);

            if (r == null)
            {
                PdfNoText.Visibility = Visibility.Visible;
                PdfInfo.Visibility = Visibility.Collapsed;
                PdfOpenBtn.Visibility = Visibility.Collapsed;
                PdfSaveBtn.Visibility = Visibility.Collapsed;
                return;
            }

            PdfNoText.Visibility = Visibility.Collapsed;
            PdfInfo.Visibility = Visibility.Visible;
            PdfOpenBtn.Visibility = Visibility.Visible;
            PdfSaveBtn.Visibility = Visibility.Visible;

            PdfNameText.Text = r.FileName;
            PdfDateText.Text = "Загружено: "
                + (r.UploadedAt.HasValue
                    ? r.UploadedAt.Value.ToString("dd.MM.yyyy HH:mm")
                    : "—");
        }

        private void PdfOpen_Click(object sender, RoutedEventArgs e)
        {
            var r = ResumeStorage.GetByUser(_studentUserId);
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

        private void PdfSave_Click(object sender, RoutedEventArgs e)
        {
            var r = ResumeStorage.GetByUser(_studentUserId);
            if (r == null) return;

            if (!File.Exists(r.FilePath))
            {
                AppMessage.Error(Window.GetWindow(this),
                    "Файл не найден на диске");
                return;
            }

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = r.FileName,
                Filter = "PDF-файлы (*.pdf)|*.pdf"
            };

            if (dlg.ShowDialog() != true) return;

            try
            {
                File.Copy(r.FilePath, dlg.FileName, true);
                AppMessage.Success(Window.GetWindow(this),
                    "Файл сохранён");
            }
            catch (Exception ex)
            {
                AppMessage.Error(Window.GetWindow(this),
                    "Ошибка сохранения: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}