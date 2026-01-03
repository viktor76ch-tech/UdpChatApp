using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UdpChatApp.Models;

namespace UdpChatApp.Services
{
    public class FileStorageService
    {
        private readonly string _historyFilePath;
        public FileStorageService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "UdpChatApp");

            Directory.CreateDirectory(appFolder);

            _historyFilePath = Path.Combine(appFolder, "message_history.txt");
            Console.WriteLine($"Файл истории: {_historyFilePath}");
        }

        public void SaveMessage(Message message)
        {
            try
            {
                // 1. Форматируем строку для записи
                // Формат: [Время] Отправитель: Текст
                string messageLine = $"[{message.Timestamp:yyyy-MM-dd HH:mm:ss}] {message.SenderName}: {message.Text}";

                // 2. Используем StreamWriter с append: true (добавляем в конец файла)
                // using гарантирует, что файл будет закрыт даже при ошибке
                using (StreamWriter writer = new StreamWriter(_historyFilePath, true))
                {
                    writer.WriteLine(messageLine);
                }

                Console.WriteLine($"Сообщение сохранено в файл: {messageLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении сообщения: {ex.Message}");
            }
        }
        //остановка
    }
}
