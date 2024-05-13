using System;
using System.IO;

namespace YoutrackHelper2.Models
{
    public static class Logger
    {
        public static void WriteMessageToFile(string message)
        {
            var dt = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            WriteMessageToFile($"{dt} {message}", "log.txt");
        }

        private static void WriteMessageToFile(string message, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("ファイル名が入力されていません");
            }

            try
            {
                // StreamWriterを使用してファイルにメッセージを書き込む
                using var writer = new StreamWriter(fileName, true);
                writer.WriteLine(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ファイルの書き込み中にエラー: {ex.Message}");
            }
        }
    }
}