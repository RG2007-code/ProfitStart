using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ProfitStart
{
    public static class Chart
    {
        public static readonly string[] Palette =
        {
            "#5B5BF5", "#F59E0B", "#10B981", "#EF4444",
            "#8B5CF6", "#06B6D4", "#EC4899", "#84CC16"
        };

       
        public static void DrawBars(Canvas canvas,
                                    Dictionary<string, int> data)
        {
            canvas.Children.Clear();
            if (data == null || data.Count == 0)
            {
                DrawEmpty(canvas, "Нет данных");
                return;
            }

            double w = canvas.ActualWidth > 0 ? canvas.ActualWidth : 400;
            double h = canvas.ActualHeight > 0 ? canvas.ActualHeight : 200;

            int n = data.Count;
            double padBottom = 34, padTop = 22, padSides = 8;
            double chartH = h - padBottom - padTop;
            double slotW = (w - padSides * 2) / n;
            double barW = slotW * 0.6;
            double gap = (slotW - barW) / 2;

            int maxVal = data.Values.Max();
            if (maxVal <= 0) maxVal = 1;

            var brush = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#5B5BF5"));

            int idx = 0;
            foreach (var kv in data)
            {
                double val = kv.Value;
                double barH = chartH * (val / (double)maxVal);
                if (barH < 2 && val > 0) barH = 2;

                var rect = new Rectangle
                {
                    Width = barW,
                    Height = barH,
                    Fill = brush,
                    RadiusX = 4,
                    RadiusY = 4
                };
                Canvas.SetLeft(rect, padSides + idx * slotW + gap);
                Canvas.SetTop(rect, padTop + (chartH - barH));
                canvas.Children.Add(rect);

               
                var valTb = new TextBlock
                {
                    Text = val.ToString(),
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#374151")),
                    Width = slotW,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(valTb, padSides + idx * slotW);
                Canvas.SetTop(valTb, padTop + (chartH - barH) - 18);
                canvas.Children.Add(valTb);

                
                var lblTb = new TextBlock
                {
                    Text = kv.Key,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#6B7280")),
                    Width = slotW,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                Canvas.SetLeft(lblTb, padSides + idx * slotW);
                Canvas.SetTop(lblTb, h - padBottom + 4);
                canvas.Children.Add(lblTb);

                idx++;
            }
        }

        
        public static void DrawPie(Canvas canvas,
                                   Dictionary<string, int> data)
        {
            canvas.Children.Clear();
            if (data == null || data.Count == 0)
            {
                DrawEmpty(canvas, "Нет данных");
                return;
            }

            double w = canvas.ActualWidth > 0 ? canvas.ActualWidth : 240;
            double h = canvas.ActualHeight > 0 ? canvas.ActualHeight : 200;

            double size = Math.Min(w, h) - 30;
            double cx = w / 2;
            double cy = h / 2;
            double r = size / 2;

            int total = data.Values.Sum();
            if (total <= 0)
            {
                DrawEmpty(canvas, "Нет данных");
                return;
            }

            double startAngle = -90;
            int idx = 0;

            foreach (var kv in data)
            {
                double sweep = 360.0 * kv.Value / total;

                var path = new Path
                {
                    Fill = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString(
                            Palette[idx % Palette.Length]))
                };

                var geo = new PathGeometry();
                var fig = new PathFigure
                {
                    StartPoint = new Point(cx, cy),
                    IsClosed = true
                };

                double a1 = Math.PI * startAngle / 180.0;
                double a2 = Math.PI * (startAngle + sweep) / 180.0;

                fig.Segments.Add(new LineSegment(
                    new Point(cx, cy), true));
                fig.Segments.Add(new LineSegment(
                    new Point(cx + r * Math.Cos(a1),
                              cy + r * Math.Sin(a1)), true));

                fig.Segments.Add(new ArcSegment
                {
                    Point = new Point(cx + r * Math.Cos(a2),
                                      cy + r * Math.Sin(a2)),
                    Size = new Size(r, r),
                    IsLargeArc = sweep > 180,
                    SweepDirection = SweepDirection.Clockwise
                });

                fig.Segments.Add(new LineSegment(
                    new Point(cx, cy), true));

                geo.Figures.Add(fig);
                path.Data = geo;
                canvas.Children.Add(path);

                startAngle += sweep;
                idx++;
            }
        }

       
        public static void DrawHorizontalBars(Canvas canvas,
                                              Dictionary<string, int> data)
        {
            canvas.Children.Clear();
            if (data == null || data.Count == 0)
            {
                DrawEmpty(canvas, "Нет данных");
                return;
            }

            double w = canvas.ActualWidth > 0 ? canvas.ActualWidth : 400;
            double h = canvas.ActualHeight > 0 ? canvas.ActualHeight : 200;

            int n = data.Count;
            double padLeft = 130, padRight = 40, padTop = 10, padBottom = 10;
            double chartW = w - padLeft - padRight;
            double rowH = (h - padTop - padBottom) / n;
            double barH = rowH * 0.65;

            int maxVal = data.Values.Max();
            if (maxVal <= 0) maxVal = 1;

            int idx = 0;
            foreach (var kv in data)
            {
                double val = kv.Value;
                double barW = chartW * (val / (double)maxVal);
                if (barW < 2 && val > 0) barW = 2;

                double y = padTop + idx * rowH + (rowH - barH) / 2;

                
                var lbl = new TextBlock
                {
                    Text = kv.Key,
                    FontSize = 11,
                    Width = padLeft - 8,
                    TextAlignment = TextAlignment.Right,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#374151"))
                };
                Canvas.SetLeft(lbl, 0);
                Canvas.SetTop(lbl, y + 2);
                canvas.Children.Add(lbl);

                
                var rect = new Rectangle
                {
                    Width = barW,
                    Height = barH,
                    Fill = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString(
                            Palette[idx % Palette.Length])),
                    RadiusX = 4,
                    RadiusY = 4
                };
                Canvas.SetLeft(rect, padLeft);
                Canvas.SetTop(rect, y);
                canvas.Children.Add(rect);

                
                var valTb = new TextBlock
                {
                    Text = val.ToString(),
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#374151"))
                };
                Canvas.SetLeft(valTb, padLeft + barW + 6);
                Canvas.SetTop(valTb, y + 2);
                canvas.Children.Add(valTb);

                idx++;
            }
        }

        
        public static void DrawEmpty(Canvas canvas, string message)
        {
            canvas.Children.Clear();

            double w = canvas.ActualWidth > 0 ? canvas.ActualWidth : 400;
            double h = canvas.ActualHeight > 0 ? canvas.ActualHeight : 200;

            var tb = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#9CA3AF")),
                FontSize = 13,
                Width = w,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetTop(tb, h / 2 - 10);
            canvas.Children.Add(tb);
        }
    }
}