using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProfitStart
{
    public static class Image
    {
       
        public static void LoadIntoEllipse(Ellipse ellipse, byte[] data)
        {
            if (data == null || data.Length == 0) return;

            var bmp = BytesToBitmap(data);
            if (bmp == null) return;

            ellipse.Fill = new ImageBrush(bmp)
            {
                Stretch = Stretch.UniformToFill
            };
        }

       
        public static void LoadIntoBorder(Border border, byte[] data)
        {
            if (data == null || data.Length == 0) return;

            var bmp = BytesToBitmap(data);
            if (bmp == null) return;

            border.Background = new ImageBrush(bmp)
            {
                Stretch = Stretch.UniformToFill
            };
        }

        
        public static void LoadIntoBorderFull(Border border, byte[] data)
        {
            if (data == null || data.Length == 0) return;

            var bmp = BytesToBitmap(data);
            if (bmp == null) return;

            border.Background = new ImageBrush(bmp)
            {
                Stretch = Stretch.Uniform
            };
        }

        
        private static BitmapImage BytesToBitmap(byte[] data)
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = new MemoryStream(data);
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        
        public static byte[] FileToBytes(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            return File.ReadAllBytes(filePath);
        }
    }
}