using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UdpChatApp.Models;

namespace UdpChatApp.Services
{
    public class FileStorageService
    {
        private readonly string _historyFilePath;

        public FileStorageService()
        {
            // Упрощенный путь к файлу
            string appFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "UdpChatApp");

            Directory.CreateDirectory(appFolder);
            _historyFilePath = Path.Combine(appFolder, "message_history.txt");
        }

        public void SaveMessage(Message message)
        {
            try
            {
                string messageLine = $"[{message.Timestamp:yyyy-MM-dd HH:mm:ss}] [{message.Type}] {message.SenderName}: {message.Text}";
                File.AppendAllText(_historyFilePath, messageLine + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception)
            {
                // В реальном приложении здесь должна быть обработка ошибок
            }
        }

        public List<string> LoadHistory()
        {
            var history = new List<string>();

            if (!File.Exists(_historyFilePath))
                return history;

            try
            {
                var lines = File.ReadAllLines(_historyFilePath);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        history.Add(line);
                }
            }
            catch (Exception)
            {
                // Обработка ошибок чтения
            }

            return history;
        }
    }
}