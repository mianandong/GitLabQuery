using System;
using System.IO;
using System.Text;

namespace GitLabQuery
{
    public static class ExcelHelper
    {
        public static readonly string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            "送测影响模块说明.csv");

        public static void Write(string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                File.AppendAllLines(FilePath, new[] { content }, Encoding.UTF8);
            }
        }

        public static void DeleteFile()
        {
            File.Delete(FilePath);
        }
    }
}
