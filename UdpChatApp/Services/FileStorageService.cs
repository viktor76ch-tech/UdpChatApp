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

        // Загружает историю сообщений из файла
        //Список строк с историей сообщений
        public List<string> LoadHistory()
        {
            var history = new List<string>();

            // Проверяем, существует ли файл
            if (!File.Exists(_historyFilePath))
            {
                Console.WriteLine("Файл истории не найден. Будет создан новый.");
                return history; // Возвращаем пустой список
            }

            try
            {
                // Используем StreamReader для чтения файла
                using (StreamReader reader = new StreamReader(_historyFilePath))
                {
                    string line;
                    // Читаем файл построчно до конца
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Добавляем каждую строку в список
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            history.Add(line);
                        }
                    }
                }

                Console.WriteLine($"Загружено {history.Count} сообщений из истории");
                return history;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке истории: {ex.Message}");
                return history; // Возвращаем пустой список в случае ошибки
            }
        }
    }
}
