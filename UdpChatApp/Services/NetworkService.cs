using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UdpChatApp.Models;
using System.Diagnostics;

namespace UdpChatApp.Services
{
    public class NetworkService
    {
        // События для уведомления ViewModel
        public event EventHandler<Message> MessageReceived;
        public event EventHandler<string> StatusChanged;

        private UdpClient _udpClient;
        private int _listenPort = 5001;
        private int _sendPort = 5000;
        private IPAddress _sendAddress;
        private string _currentUserName;
        private CancellationTokenSource _cancellationTokenSource;

        // Настройки для JSON сериализации
        private readonly JsonSerializerOptions _jsonOptions;

        public bool IsConnected { get; private set; }

        public NetworkService()
        {
            _sendAddress = IPAddress.Loopback;

            // Настраиваем JSON сериализатор
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = true,
                IncludeFields = false,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public void Connect(string userName, string ipAddress, int listenPort, int sendPort)
        {
            try
            {
                _currentUserName = userName;
                _sendAddress = IPAddress.Parse(ipAddress);
                _sendPort = sendPort;
                _listenPort = listenPort;

                // Создаем UDP-клиент для прослушивания
                _udpClient = new UdpClient(_listenPort);
                _udpClient.Client.ReceiveTimeout = 1000;

                // Запускаем фоновую задачу для прослушивания сообщений
                _cancellationTokenSource = new CancellationTokenSource();
                Task.Run(() => ListenForMessages(_cancellationTokenSource.Token));

                IsConnected = true;
                OnStatusChanged($"Подключен как {userName}. Слушаю порт: {listenPort}");
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка подключения: {ex.Message}");
                throw;
            }
        }

        private async Task ListenForMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _udpClient != null)
            {
                try
                {
                    // Асинхронно ждем входящее сообщение
                    var result = await _udpClient.ReceiveAsync(cancellationToken);
                    var jsonString = Encoding.UTF8.GetString(result.Buffer);

                    // Десериализуем JSON в объект Message
                    var receivedMessage = JsonSerializer.Deserialize<Message>(jsonString, _jsonOptions);

                    if (receivedMessage != null)
                    {
                        // Устанавливаем IP и порт отправителя
                        receivedMessage.SenderIp = result.RemoteEndPoint.Address.ToString();
                        receivedMessage.SenderPort = result.RemoteEndPoint.Port;

                        // Вызываем событие для уведомления ViewModel
                        OnMessageReceived(receivedMessage);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при приеме сообщения: {ex.Message}");
                }
            }
        }

        public void SendMessage(string text, string recipient = "", MessageType type = MessageType.Public)
        {
            if (!IsConnected || _udpClient == null)
            {
                OnStatusChanged("Не подключен. Сначала подключитесь к чату.");
                return;
            }

            try
            {
                // Создаем объект сообщения
                var message = new Message
                {
                    Text = text,
                    SenderName = _currentUserName,
                    Timestamp = DateTime.Now,
                    Type = type,
                    Recipient = recipient
                };

                // Сериализуем в JSON
                var jsonString = JsonSerializer.Serialize(message, _jsonOptions);
                var bytes = Encoding.UTF8.GetBytes(jsonString);

                // Создаем конечную точку для отправки
                var endPoint = new IPEndPoint(_sendAddress, _sendPort);

                // Отправляем сообщение
                _udpClient.Send(bytes, bytes.Length, endPoint);

                // Локально добавляем сообщение от себя
                var localMessage = new Message
                {
                    Text = text,
                    SenderName = "Вы",
                    Timestamp = DateTime.Now,
                    Type = type,
                    Recipient = recipient
                };

                OnMessageReceived(localMessage);
                OnStatusChanged($"Сообщение отправлено");
            }
            catch (Exception ex)
            {
                OnStatusChanged($"Ошибка отправки: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            try
            {
                // Отменяем задачу прослушивания
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }

                // Закрываем UDP-клиент
                _udpClient?.Close();
                _udpClient = null;

                IsConnected = false;
                OnStatusChanged("Отключен от чата");
            }
            catch (Exception ex)
            {
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