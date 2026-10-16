using System;
using System.IO;
using System.Linq;

namespace ProfitStart
{
    public static class ResumeStorage
    {
        
        public static string Folder =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resumes");

        public static void EnsureFolder()
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);
        }

        
        public static string Save(string sourcePath, int userId)
        {
            EnsureFolder();

            string ext = Path.GetExtension(sourcePath);
            string safeName = $"resume_{userId}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            string dest = Path.Combine(Folder, safeName);

            File.Copy(sourcePath, dest, overwrite: true);
            return dest;
        }

        public static void Delete(string filePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch {  }
        }

        
        public static Resumes GetByUser(int userId)
        {
            return Connection.connect.Resumes
                .FirstOrDefault(r => r.UserId == userId);
        }
    }
}