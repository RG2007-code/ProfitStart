using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProfitStart
{
    public static class Theme
    {
        private static readonly Dictionary<Page, Dictionary<DependencyObject, Brush>> _pageBg
            = new Dictionary<Page, Dictionary<DependencyObject, Brush>>();
        private static readonly Dictionary<Page, Dictionary<DependencyObject, Brush>> _pageFg
            = new Dictionary<Page, Dictionary<DependencyObject, Brush>>();

        private static readonly Dictionary<Window, Dictionary<DependencyObject, Brush>> _winBg
            = new Dictionary<Window, Dictionary<DependencyObject, Brush>>();
        private static readonly Dictionary<Window, Dictionary<DependencyObject, Brush>> _winFg
            = new Dictionary<Window, Dictionary<DependencyObject, Brush>>();

        private static readonly Color White = Colors.White;
        private static readonly Color LightBg = (Color)ColorConverter.ConvertFromString("#F5F6FA");
        private static readonly Color LightPanel = (Color)ColorConverter.ConvertFromString("#EEF2FF");
        private static readonly Color DarkBg = (Color)ColorConverter.ConvertFromString("#0F172A");
        private static readonly Color DarkCard = (Color)ColorConverter.ConvertFromString("#1E293B");
        private static readonly Color DarkPanel = (Color)ColorConverter.ConvertFromString("#334155");

        private static readonly Color TextDark = (Color)ColorConverter.ConvertFromString("#374151");
        private static readonly Color TextDarker = (Color)ColorConverter.ConvertFromString("#111827");
        private static readonly Color TextBlack = (Color)ColorConverter.ConvertFromString("#0F172A");
        private static readonly Color TextLight = (Color)ColorConverter.ConvertFromString("#E5E7EB");
        private static readonly Color TextMuted = (Color)ColorConverter.ConvertFromString("#94A3B8");
        private static readonly Color Gray1 = (Color)ColorConverter.ConvertFromString("#6B7280");
        private static readonly Color Gray2 = (Color)ColorConverter.ConvertFromString("#9CA3AF");

        private static SolidColorBrush B(Color c) => new SolidColorBrush(c);

        

        public static void ApplyToPage(Page page, bool dark)
        {
            if (page == null) return;

            if (!_pageBg.ContainsKey(page))
                _pageBg[page] = new Dictionary<DependencyObject, Brush>();
            if (!_pageFg.ContainsKey(page))
                _pageFg[page] = new Dictionary<DependencyObject, Brush>();

            var bgs = _pageBg[page];
            var fgs = _pageFg[page];

            if (!bgs.ContainsKey(page))
                bgs[page] = page.Background;

            var origPageBg = bgs[page];
            page.Background = dark ? B(DarkBg) : (origPageBg ?? B(LightBg));

            foreach (var el in MainWindow.FindVisualChildren<Border>(page)) ApplyBg(el, dark, bgs);
            foreach (var el in MainWindow.FindVisualChildren<Grid>(page)) ApplyBg(el, dark, bgs);
            foreach (var el in MainWindow.FindVisualChildren<StackPanel>(page)) ApplyBg(el, dark, bgs);
            foreach (var el in MainWindow.FindVisualChildren<WrapPanel>(page)) ApplyBg(el, dark, bgs);
            foreach (var el in MainWindow.FindVisualChildren<DockPanel>(page)) ApplyBg(el, dark, bgs);

            foreach (var el in MainWindow.FindVisualChildren<TextBlock>(page)) ApplyFg(el, dark, fgs);
            foreach (var el in MainWindow.FindVisualChildren<Label>(page)) ApplyFg(el, dark, fgs);

           
            foreach (var el in MainWindow.FindVisualChildren<TextBox>(page)) ApplyTextBox(el, dark);
            foreach (var el in MainWindow.FindVisualChildren<PasswordBox>(page)) ApplyPasswordBox(el, dark);
        }

       

        public static void ApplyToWindow(Window w, bool dark)
        {
            if (w == null) return;

            if (!_winBg.ContainsKey(w))
                _winBg[w] = new Dictionary<DependencyObject, Brush>();
            if (!_winFg.ContainsKey(w))
                _winFg[w] = new Dictionary<DependencyObject, Brush>();

            var bgs = _winBg[w];
            var fgs = _winFg[w];

            if (!bgs.ContainsKey(w))
                bgs[w] = w.Background;

            var origWinBg = bgs[w];
            w.Background = dark ? B(DarkBg) : (origWinBg ?? B(LightBg));

            foreach (var el in MainWindow.FindVisualChildren<Border>(w)) ApplyBg(el, dark, bgs);
            foreach (var el in MainWindow.FindVisualChildren<Grid>(w)) ApplyBg(el, dark, bgs);
            foreach (var el in MainWindow.FindVisualChildren<StackPanel>(w)) ApplyBg(el, dark, bgs);
            foreach (var el in MainWindow.FindVisualChildren<WrapPanel>(w)) ApplyBg(el, dark, bgs);
            foreach (var el in MainWindow.FindVisualChildren<DockPanel>(w)) ApplyBg(el, dark, bgs);

            foreach (var el in MainWindow.FindVisualChildren<TextBlock>(w)) ApplyFg(el, dark, fgs);
            foreach (var el in MainWindow.FindVisualChildren<Label>(w)) ApplyFg(el, dark, fgs);

            
            foreach (var el in MainWindow.FindVisualChildren<TextBox>(w)) ApplyTextBox(el, dark);
            foreach (var el in MainWindow.FindVisualChildren<PasswordBox>(w)) ApplyPasswordBox(el, dark);
        }

        public static void ForgetWindow(Window w)
        {
            if (w == null) return;
            if (_winBg.ContainsKey(w)) _winBg.Remove(w);
            if (_winFg.ContainsKey(w)) _winFg.Remove(w);
        }

       

        private static void ApplyTextBox(TextBox tx, bool dark)
        {
            tx.Foreground = dark ? B(TextLight) : B(TextDark);
            tx.CaretBrush = dark ? B(TextLight) : B(TextDarker);
        }

        private static void ApplyPasswordBox(PasswordBox pb, bool dark)
        {
            pb.Foreground = dark ? B(TextLight) : B(TextDark);
            pb.CaretBrush = dark ? B(TextLight) : B(TextDarker);
        }

       

        private static void ApplyBg(DependencyObject obj, bool dark, Dictionary<DependencyObject, Brush> dict)
        {
            Brush bg = null;
            if (obj is Border b) bg = b.Background;
            else if (obj is Panel p) bg = p.Background;
            else if (obj is Control c) bg = c.Background;
            else return;

            if (!dict.ContainsKey(obj))
                dict[obj] = bg;

            var orig = dict[obj];
            if (orig == null) return;

            if (orig is SolidColorBrush sb)
            {
                var c = sb.Color;

                if (c == White)
                {
                    SetBg(obj, dark ? B(DarkCard) : B(White));
                    return;
                }
                if (c == LightBg)
                {
                    SetBg(obj, dark ? B(DarkBg) : B(LightBg));
                    return;
                }
                if (c == LightPanel)
                {
                    SetBg(obj, dark ? B(DarkPanel) : B(LightPanel));
                    return;
                }
            }
        }

        private static void ApplyFg(DependencyObject obj, bool dark, Dictionary<DependencyObject, Brush> dict)
        {
            Brush fg = null;
            if (obj is TextBlock t) fg = t.Foreground;
            else if (obj is Label l) fg = l.Foreground;
            else if (obj is Control c) fg = c.Foreground;
            else return;

            if (!dict.ContainsKey(obj))
                dict[obj] = fg;

            var orig = dict[obj];

            if (orig == null)
            {
                SetFg(obj, dark ? B(TextLight) : Brushes.Black);
                return;
            }

            if (orig is SolidColorBrush sb)
            {
                var c = sb.Color;

                if (c == TextDark || c == TextDarker || c == TextBlack || c == Colors.Black)
                {
                    SetFg(obj, dark ? B(TextLight) : sb);
                    return;
                }

                if (dark && (c == Gray1 || c == Gray2 || c == Colors.Gray))
                {
                    SetFg(obj, B(TextMuted));
                    return;
                }

                if (!dark) SetFg(obj, sb);
            }
        }

        private static void SetBg(DependencyObject obj, Brush brush)
        {
            if (obj is Border b) b.Background = brush;
            else if (obj is Panel p) p.Background = brush;
            else if (obj is Control c) c.Background = brush;
        }

        private static void SetFg(DependencyObject obj, Brush brush)
        {
            if (obj is TextBlock t) t.Foreground = brush;
            else if (obj is Label l) l.Foreground = brush;
            else if (obj is Control c) c.Foreground = brush;
        }
    }
}