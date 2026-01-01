using System;
using UdpChatApp.Models;

namespace UdpChatApp.Services
{
    public class NetworkService      //Пока сделаем заглушку
    {
        public event EventHandler<Message> MessageReceived;

        //Метод для подключения к чату. Пока просто запускает таймер, который имитирует получение сообщений.
        public void Connect(string userName, string ipAddress, int port)
        {
            Console.WriteLine($"Заглушка: Подключение как {userName} к {ipAddress}:{port}");
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer(_ =>
            {
                var testMessage = new Message
                {
                    Text = "Привет! Это тестовое сообщение от другого пользователя.",
                    SenderName = "Другой пользователь",
                    Timestamp = DateTime.Now,
                    Type = MessageType.Public
                };

                OnMessageReceived(testMessage);

                timer?.Dispose();

            }, null, 2000, System.Threading.Timeout.Infinite);
        }
        public void Disconnect()
        {
            Console.WriteLine("Заглушка: Отключение от чата.");
        }

        public void SendMessage(string text)
        {
            Console.WriteLine($"Заглушка: Отправка сообщения: {text}");
        }

        protected virtual void OnMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this, message);
        }
    }
}
