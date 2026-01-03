using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UdpChatApp.Models;

namespace UdpChatApp.Services
{
    public class FileStorageService
    {
        private readonly string _historyFilePath;
        public FileStorageService()
        {
            try
            {
                // Получаем путь к папке AppData пользователя
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                // Создаем подпапку для нашего приложения
                string appFolder = Path.Combine(appDataPath, "UdpChatApp");

                // Создаем папку, если она не существует (безопасный метод)
                Directory.CreateDirectory(appFolder);

                // Задаем полный путь к файлу истории
                _historyFilePath = Path.Combine(appFolder, "message_history.txt");

                // Дополнительная проверка: создаем файл, если он не существует
                if (!File.Exists(_historyFilePath))
                {
                    File.WriteAllText(_historyFilePath, "# История сообщений UDP Чата\n", Encoding.UTF8);
                    Console.WriteLine("Создан новый файл истории");
                }

                Console.WriteLine($"Файл истории: {_historyFilePath}");
            }
            catch (Exception ex)
            {
                // В случае ошибки используем резервный путь
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("Используем резервный путь в текущей папке");
                _historyFilePath = "message_history_backup.txt";
            }
        }


        public void SaveMessage(Message message)
        {
            try
            {
                // 1. Форматируем строку для записи
                // Формат: [Время] Отправитель: Текст
                string messageLine = $"[{message.Timestamp:yyyy-MM-dd HH:mm:ss}] [{message.Type}] {message.SenderName}: {message.Text}";

                // 2. Используем StreamWriter с append: true (добавляем в конец файла)
                // using гарантирует, что файл будет закрыт даже при ошибке
                using (StreamWriter writer = new StreamWriter(_historyFilePath, true))
                {
                    writer.WriteLine(messageLine);
                }

                Console.WriteLine($"[OK] Сообщение сохранено: {message.SenderName}: {message.Text}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка при сохранении: {ex.Message}");
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
