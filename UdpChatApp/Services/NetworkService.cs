using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UdpChatApp.Models;

namespace UdpChatApp.Services
{
    public class NetworkService
    {
        // События для уведомления ViewModel
        public event EventHandler<Message> MessageReceived;

        public event EventHandler<string> StatusChanged;

        private UdpClient _udpClient;            // UDP-клиент для отправки и приема

        private int _listenPort = 5000;          // Порт для прослушивания

        private int _sendPort = 5000;            // Порт для отправки

        private IPAddress _sendAddress;          // IP-адрес для отправки

        private string _currentUserName;         // Имя текущего пользователя

        private CancellationTokenSource _cancellationTokenSource;        // Токен для отмены задачи прослушивания

        // Флаг подключения
        public bool IsConnected { get; private set; }

        // Конструктор
        public NetworkService()
        {
            _sendAddress = IPAddress.Loopback; // По умолчанию 127.0.0.1
        }

        public void Connect(string userName, string ipAddress, int port)
        {
            try
            {
                Console.WriteLine($"=== NetworkService.Connect() вызван ===");
                Console.WriteLine($"Параметры: {userName}, {ipAddress}, {port}");

                // Сохраняем параметры подключения
                _currentUserName = userName;
                _sendAddress = IPAddress.Parse(ipAddress);
                _sendPort = port;
                _listenPort = port; // Слушаем тот же порт, что и отправляем

                // Создаем UDP-клиент для прослушивания
                Console.WriteLine($"Создаем UdpClient на порту {_listenPort}");
                _udpClient = new UdpClient(_listenPort);

                // Настраиваем клиент
                _udpClient.Client.ReceiveTimeout = 1000; // Таймаут 1 секунда
                Console.WriteLine($"UdpClient создан и настроен");

                // Запускаем фоновую задачу для прослушивания сообщений
                Console.WriteLine($"Запускаем фоновую задачу прослушивания");
                _cancellationTokenSource = new CancellationTokenSource();
                Task.Run(() => ListenForMessages(_cancellationTokenSource.Token));

                // Устанавливаем флаг подключения
                IsConnected = true;

                // Уведомляем об успешном подключении
                OnStatusChanged($"Подключен как {userName} к {ipAddress}:{port}");
                Console.WriteLine($"=== Подключение успешно ===");
            }
            catch (FormatException ex)
            {
                OnStatusChanged($"Неверный IP-адрес: {ipAddress}");
                Console.WriteLine($"Ошибка формата IP: {ex.Message}");
                throw;
            }
            catch (SocketException ex)
            {
                OnStatusChanged($"Ошибка сети: {ex.SocketErrorCode}");
                Console.WriteLine($"Ошибка сокета: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка подключения: {ex.Message}");
                Console.WriteLine($"Общая ошибка: {ex.Message}");
                throw;
            }
        }

        private async Task ListenForMessages(CancellationToken cancellationToken)
        {
            Console.WriteLine($"=== ListenForMessages начал работу ===");

            while (!cancellationToken.IsCancellationRequested && _udpClient != null)
            {
                try
                {
                    Console.WriteLine($"Ожидаем входящее сообщение на порту {_listenPort}...");

                    // Асинхронно ждем входящее сообщение
                    var result = await _udpClient.ReceiveAsync(cancellationToken);
                    var jsonString = Encoding.UTF8.GetString(result.Buffer);

                    Console.WriteLine($"Получены данные: {jsonString}");

                    try
                    {
                        // Десериализуем JSON в объект Message
                        var receivedMessage = JsonSerializer.Deserialize<Message>(jsonString);

                        // Устанавливаем IP и порт отправителя
                        receivedMessage.SenderIp = result.RemoteEndPoint.Address.ToString();
                        receivedMessage.SenderPort = result.RemoteEndPoint.Port;

                        Console.WriteLine($"Сообщение от {receivedMessage.SenderName}: {receivedMessage.Text}");

                        // Вызываем событие для уведомления ViewModel
                        MessageReceived?.Invoke(this, receivedMessage);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Ошибка десериализации JSON: {ex.Message}");
                        Console.WriteLine($"Полученная строка: {jsonString}");
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"Операция прослушивания отменена");
                    break; // Выходим из цикла при отмене
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine($"UdpClient был закрыт, прекращаем прослушивание");
                    break; // Выходим из цикла
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    // Таймаут - нормальная ситуация, продолжаем слушать
                    continue;
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Ошибка сокета: {ex.SocketErrorCode} - {ex.Message}");
                    // Продолжаем слушать несмотря на ошибку
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Неожиданная ошибка при приеме сообщения: {ex.Message}");
                    // Продолжаем слушать
                }
            }

            Console.WriteLine($"=== ListenForMessages завершил работу ===");
        }

        public void SendMessage(string text)
        {
            if (!IsConnected || _udpClient == null)
            {
                OnStatusChanged("Не подключен. Сначала подключитесь к чату.");
                Console.WriteLine("Ошибка: попытка отправить сообщение без подключения");
                return;
            }

            try
            {
                Console.WriteLine($"=== Отправка сообщения ===");
                Console.WriteLine($"Текст: {text}");
                Console.WriteLine($"Получатель: {_sendAddress}:{_sendPort}");

                // Создаем объект сообщения для сериализации
                var message = new Message
                {
                    Text = text,
                    SenderName = _currentUserName,
                    Timestamp = DateTime.Now,
                    Type = MessageType.Public,
                    SenderIp = "127.0.0.1", // Временно, потом заменим на реальный IP
                    SenderPort = _listenPort
                };

                // Сериализуем в JSON
                var jsonString = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(jsonString);

                Console.WriteLine($"JSON для отправки: {jsonString}");
                Console.WriteLine($"Длина данных: {bytes.Length} байт");

                // Создаем конечную точку для отправки
                var endPoint = new IPEndPoint(_sendAddress, _sendPort);

                // Отправляем сообщение
                _udpClient.Send(bytes, bytes.Length, endPoint);

                Console.WriteLine($"Сообщение успешно отправлено");
                OnStatusChanged($"Отправлено: {text}");
            }
            catch (SocketException ex)
            {
                OnStatusChanged($"Ошибка сети при отправке: {ex.SocketErrorCode}");
                Console.WriteLine($"Ошибка сокета при отправке: {ex.Message}");
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка отправки: {ex.Message}");
                Console.WriteLine($"Общая ошибка при отправке: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            try
            {
                Console.WriteLine($"=== NetworkService.Disconnect() ===");

                // Отменяем задачу прослушивания
                if (_cancellationTokenSource != null)
                {
                    Console.WriteLine($"Отменяем задачу прослушивания...");
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }

                // Закрываем UDP-клиент
                if (_udpClient != null)
                {
                    Console.WriteLine($"Закрываем UdpClient...");
                    _udpClient.Close();
                    _udpClient = null;
                }

                // Сбрасываем флаг подключения
                IsConnected = false;

                OnStatusChanged("Отключен от чата");
                Console.WriteLine($"=== Отключение успешно ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отключении: {ex.Message}");
                OnStatusChanged($"Ошибка отключения: {ex.Message}");
            }
        }

        protected virtual void OnMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this, message);
        }

        protected virtual void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, status);
        }
    }
}