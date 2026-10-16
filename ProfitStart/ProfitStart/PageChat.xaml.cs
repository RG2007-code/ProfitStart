using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProfitStart
{
    public partial class PageChat : Page
    {
        private int _otherId;

        private int _studentId;
        private int _companyId;
        private bool _isAdminDialog = false;

        private DispatcherTimer _statusTimer;
        private DispatcherTimer _msgTimer;
        private int _lastMsgCount = -1;

        public PageChat() { InitializeComponent(); }

        public PageChat(int otherId)
        {
            InitializeComponent();
            _otherId = otherId;

            Loaded += PageChat_Loaded;
            Unloaded += PageChat_Unloaded;
        }

        public PageChat(int studentId, int companyId)
        {
            InitializeComponent();

            _isAdminDialog = true;
            _studentId = studentId;
            _companyId = companyId;
            _otherId = 0;

            Loaded += PageChat_Loaded;
            Unloaded += PageChat_Unloaded;
        }

        private void PageChat_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
            Session.Touch();

            if (!_isAdminDialog)
            {
                if (_statusTimer == null)
                {
                    _statusTimer = new DispatcherTimer();
                    _statusTimer.Interval = TimeSpan.FromSeconds(5);
                    _statusTimer.Tick += (s, args) => UpdateStatus();
                }
                _statusTimer.Start();
            }

            if (_msgTimer == null)
            {
                _msgTimer = new DispatcherTimer();
                _msgTimer.Interval = TimeSpan.FromMilliseconds(500);
                _msgTimer.Tick += (s, args) => CheckNewMessages();
            }
            _msgTimer.Start();
        }

        private void PageChat_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_statusTimer != null) _statusTimer.Stop();
            if (_msgTimer != null) _msgTimer.Stop();
        }

        private void Load()
        {
            HeaderText.Text = ResolveOtherName();

            if (_isAdminDialog)
            {
                StatusDot.Visibility = Visibility.Collapsed;
                StatusText.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpdateStatus();
            }

            var unread = Connection.connect.Messages
                .Where(m => m.IsRead == false)
                .ToList();

            foreach (var m in unread)
            {
                bool mine = false;

                if (_isAdminDialog)
                {
                    mine = m.StudentId == _studentId
                        && m.CompanyId == _companyId;
                }
                else if (Session.IsStudent && Session.StudentId != 0)
                {
                    mine = m.SenderType == "Company"
                        && m.StudentId == Session.StudentId
                        && m.CompanyId == _otherId;
                }
                else if (Session.IsEmployer && Session.CompanyId != 0)
                {
                    mine = m.SenderType == "Student"
                        && m.CompanyId == Session.CompanyId
                        && m.StudentId == _otherId;
                }

                if (mine) m.IsRead = true;
            }
            Connection.connect.SaveChanges();

            var main = Window.GetWindow(this) as MainWindow;
            main?.LoadMailBadge();

            RenderMessages();
        }

        private void CheckNewMessages()
        {
            int current = GetCurrentDialogMessagesCount();

            if (current != _lastMsgCount)
            {
                _lastMsgCount = current;
                MarkIncomingAsRead();
                RenderMessages();

                var main = Window.GetWindow(this) as MainWindow;
                main?.LoadMailBadge();
            }
        }

        private int GetCurrentDialogMessagesCount()
        {
            if (_isAdminDialog)
            {
                return Connection.connect.Messages
                    .AsNoTracking()
                    .Where(m => m.StudentId == _studentId
                             && m.CompanyId == _companyId)
                    .ToList()
                    .Count;
            }

            if (Session.IsStudent && Session.StudentId != 0)
                return Connection.connect.Messages
                    .AsNoTracking()
                    .Where(m => m.StudentId == Session.StudentId
                             && m.CompanyId == _otherId)
                    .ToList()
                    .Count;

            if (Session.IsEmployer && Session.CompanyId != 0)
                return Connection.connect.Messages
                    .AsNoTracking()
                    .Where(m => m.CompanyId == Session.CompanyId
                             && m.StudentId == _otherId)
                    .ToList()
                    .Count;

            return 0;
        }

        private void MarkIncomingAsRead()
        {
            var unread = Connection.connect.Messages
                .Where(m => m.IsRead == false)
                .ToList();

            bool changed = false;

            foreach (var m in unread)
            {
                bool mine = false;

                if (_isAdminDialog)
                {
                    mine = m.StudentId == _studentId
                        && m.CompanyId == _companyId;
                }
                else if (Session.IsStudent && Session.StudentId != 0)
                {
                    mine = m.SenderType == "Company"
                        && m.StudentId == Session.StudentId
                        && m.CompanyId == _otherId;
                }
                else if (Session.IsEmployer && Session.CompanyId != 0)
                {
                    mine = m.SenderType == "Student"
                        && m.CompanyId == Session.CompanyId
                        && m.StudentId == _otherId;
                }

                if (mine) { m.IsRead = true; changed = true; }
            }

            if (changed) Connection.connect.SaveChanges();
        }

        private string ResolveOtherName()
        {
            if (_isAdminDialog)
            {
                var s = Connection.connect.Students
                    .FirstOrDefault(x => x.Id == _studentId);
                var c = Connection.connect.Companies
                    .FirstOrDefault(x => x.Id == _companyId);

                string sName = s != null ? s.FullName : "Студент";
                string cName = c != null ? c.Name : "Компания";

                return sName + " ↔ " + cName;
            }

            if (Session.IsStudent)
            {
                var c = Connection.connect.Companies
                    .FirstOrDefault(x => x.Id == _otherId);
                return c != null ? c.Name : "Компания";
            }
            else if (Session.IsEmployer)
            {
                var s = Connection.connect.Students
                    .FirstOrDefault(x => x.Id == _otherId);
                return s != null ? s.FullName : "Студент";
            }

            return "Пользователь";
        }

        private int? ResolveOtherUserId()
        {
            if (_isAdminDialog)
            {
                var s = Connection.connect.Students
                    .FirstOrDefault(x => x.Id == _studentId);
                if (s != null && s.UserId.HasValue) return s.UserId.Value;

                var c = Connection.connect.Companies
                    .FirstOrDefault(x => x.Id == _companyId);
                if (c != null && c.UserId.HasValue) return c.UserId.Value;

                return null;
            }

            var comp = Connection.connect.Companies
                .FirstOrDefault(x => x.Id == _otherId);
            if (comp != null && comp.UserId.HasValue) return comp.UserId.Value;

            var stud = Connection.connect.Students
                .FirstOrDefault(x => x.Id == _otherId);
            if (stud != null && stud.UserId.HasValue) return stud.UserId.Value;

            return null;
        }

        private DateTime? GetOtherLastSeen()
        {
            int? otherUserId = ResolveOtherUserId();
            if (!otherUserId.HasValue) return null;

            var u = Connection.connect.Users
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == otherUserId.Value);

            return u?.LastSeenAt;
        }

        private void UpdateStatus()
        {
            if (StatusDot == null || StatusText == null) return;
            if (_isAdminDialog) return;

            DateTime? lastSeen = GetOtherLastSeen();
            bool online = Status.IsOnline(lastSeen);

            StatusDot.Fill = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(
                    online ? "#16A34A" : "#9CA3AF"));

            StatusText.Text = Status.Format(lastSeen);
            StatusText.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(
                    online ? "#16A34A" : "#6B7280"));
        }

        private void RenderMessages()
        {
            List<Messages> msgs;

            if (_isAdminDialog)
            {
                msgs = Connection.connect.Messages
                    .AsNoTracking()
                    .Where(m => m.StudentId == _studentId
                             && m.CompanyId == _companyId)
                    .OrderBy(m => m.CreatedAt)
                    .ToList();
            }
            else if (Session.IsStudent && Session.StudentId != 0)
            {
                msgs = Connection.connect.Messages
                    .AsNoTracking()
                    .Where(m => m.StudentId == Session.StudentId
                             && m.CompanyId == _otherId)
                    .OrderBy(m => m.CreatedAt)
                    .ToList();
            }
            else if (Session.IsEmployer && Session.CompanyId != 0)
            {
                msgs = Connection.connect.Messages
                    .AsNoTracking()
                    .Where(m => m.CompanyId == Session.CompanyId
                             && m.StudentId == _otherId)
                    .OrderBy(m => m.CreatedAt)
                    .ToList();
            }
            else
            {
                msgs = new List<Messages>();
            }

            _lastMsgCount = msgs.Count;

            var list = msgs.Select(m =>
            {
                bool isMine;

                if (_isAdminDialog)
                    isMine = m.SenderType == "Company";
                else
                    isMine = (Session.IsStudent && m.SenderType == "Student")
                          || (Session.IsEmployer && m.SenderType == "Company");

                return new
                {
                    Text = m.Text,
                    Time = m.CreatedAt.ToString("HH:mm"),
                    Bg = isMine
                        ? new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#5B5BF5"))
                        : new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#F3F4F6")),
                    Fg = isMine
                        ? new SolidColorBrush(Colors.White)
                        : new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#374151")),
                    Align = isMine
                        ? HorizontalAlignment.Right
                        : HorizontalAlignment.Left,
                    Margin = isMine
                        ? new Thickness(20, 4, 0, 4)
                        : new Thickness(0, 4, 20, 4)
                };
            }).ToList();

            MessagesList.ItemsSource = list;

            Dispatcher.InvokeAsync(() =>
                MessagesScroll.ScrollToEnd(),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private void SendMessage()
        {
            var text = MessageBox.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            int studentId;
            int companyId;
            string senderType;

            if (_isAdminDialog)
            {
                studentId = _studentId;
                companyId = _companyId;
                senderType = "Company";
            }
            else if (Session.IsStudent && Session.StudentId != 0)
            {
                studentId = Session.StudentId;
                companyId = _otherId;
                senderType = "Student";
            }
            else if (Session.IsEmployer && Session.CompanyId != 0)
            {
                studentId = _otherId;
                companyId = Session.CompanyId;
                senderType = "Company";
            }
            else
            {
                AppMessage.Warning(Window.GetWindow(this),
                    "Вы не можете отправлять сообщения",
                    "Недоступно");
                return;
            }

            Notify.SendMessage(studentId, companyId, null, senderType, text);

            MessageBox.Text = "";
            Session.Touch();

            RenderMessages();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageMessages());
        }
    }
}