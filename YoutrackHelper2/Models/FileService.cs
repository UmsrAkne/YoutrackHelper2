using System.Diagnostics;
using System.IO;

namespace YoutrackHelper2.Models
{
    public static class FileService
    {
        public static void OpenTextFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var startInfo = new ProcessStartInfo(filePath)
                {
                    UseShellExecute = true,
                };

                Process.Start(startInfo);
            }
            else
            {
                throw new FileNotFoundException("指定されたファイルが見つかりません。", filePath);
            }
        }
    }
}