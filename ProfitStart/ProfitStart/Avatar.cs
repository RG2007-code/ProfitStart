using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProfitStart
{
    public static class Avatar
    {
        
        private static readonly System.Collections.Generic.Dictionary<char, byte[]> _cache
            = new System.Collections.Generic.Dictionary<char, byte[]>();

        
        public static byte[] GetPlaceholder(string emailOrName)
        {
            char letter = '?';

            if (!string.IsNullOrEmpty(emailOrName))
            {
                var first = emailOrName.Trim()[0];
                if (char.IsLetter(first))
                    letter = char.ToUpper(first);
            }

            if (_cache.ContainsKey(letter))
                return _cache[letter];

            try
            {
                const int size = 128;

                var bmp = new RenderTargetBitmap(
                    size, size, 96, 96,
                    PixelFormats.Pbgra32);

                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                {
                    dc.DrawEllipse(
                        new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#5B5BF5")),
                        null,
                        new System.Windows.Point(size / 2.0, size / 2.0),
                        size / 2.0 - 2, size / 2.0 - 2);

                    var text = new FormattedText(
                        letter.ToString(),
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Windows.FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"),
                        64,
                        Brushes.White,
                        96);

                    dc.DrawText(text,
                        new System.Windows.Point(
                            (size - text.Width) / 2,
                            (size - text.Height) / 2 - 4));
                }

                bmp.Render(visual);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    bytes = ms.ToArray();
                }

                _cache[letter] = bytes;
                return bytes;
            }
            catch
            {
                return null;
            }
        }

       
        public static byte[] GetOrDefault(byte[] avatarData, string emailOrName)
        {
            if (avatarData != null && avatarData.Length > 0)
                return avatarData;

            return GetPlaceholder(emailOrName);
        }
    }
}