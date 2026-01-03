using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using UdpChatApp.Models;
using UdpChatApp.Services;


namespace UdpChatApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string _messageText;
        private string _status;
        private bool _isConnected;
        private readonly NetworkService _networkService;
        private readonly FileStorageService _fileStorageService;

        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        public string MessageText
        {
            get => _messageText;
            set { SetProperty(ref _messageText, value); }
        }
        public string Status
        { 
            get => _status;
            set { SetProperty(ref _status, value); }
        }
        public bool IsConnected
        {
            get => _isConnected;
            set { SetProperty(ref _isConnected, value); }
        }
        public string MessagesText
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var msg in Messages)
                {
                    sb.AppendLine($"[{msg.Timestamp:HH:mm:ss}] {msg.SenderName}: {msg.Text}");
                }
                return sb.ToString();
            }
        }

        //команды
        public ICommand SendMessageCommand { get; }
        public ICommand ToggleConnectionCommand { get; }

        public MainViewModel()
        {
            Status = "Не подключен";
            IsConnected = false;

            _networkService = new NetworkService();
            _networkService.MessageReceived += OnMessageReceived;

            _fileStorageService = new FileStorageService();

            Messages.CollectionChanged += (s, e) => OnPropertyChanged(nameof(MessagesText));

            SendMessageCommand = new RelayCommand(execute: SendMessage, 
                canExecute: CanSendMessage);

            ToggleConnectionCommand = new RelayCommand(execute: ToggleConnection, 
                canExecute : () => true);

            LoadMessageHistory();
        }
        private bool CanSendMessage()
        {
            return !string.IsNullOrWhiteSpace(MessageText) && IsConnected; //Проверим, что текст не пустой и не из пробелов и есть подключение к чату
        }

        private void SendMessage()
        {
            var message = new Message
            {
                Text = MessageText,
                SenderName = "Я",
                Timestamp = DateTime.Now,
                Type = MessageType.Public
            };

            Messages.Add(message);
            _fileStorageService.SaveMessage(message);
            _networkService.SendMessage(MessageText);
            MessageText = string.Empty;
        }

        private void ToggleConnection()
        {
            if (IsConnected)
            {
                _networkService.Disconnect();
                Status = "Не подключен";
                IsConnected = false;
            }
            else
            {
                _networkService.Connect("TestUser", "127.0.0.1", 5000);
                Status = "Подключен";
                IsConnected = true;
                Messages.Add(new Message
                {
                    Text = "Подключение установлено",
                    SenderName = "Система",
                    Timestamp = DateTime.Now,
                    Type = MessageType.System
                });
            }
        }
        private void OnMessageReceived(object sender, Message message)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(message);
                _fileStorageService.SaveMessage(message);
            });
        }
        // Метод для загрузки истории сообщений из файла
        private void LoadMessageHistory()
        {
            try
            {
                var history = _fileStorageService.LoadHistory();

                if (history.Count == 0)
                {
                    Console.WriteLine("История сообщений пуста");
                    return;
                }

                foreach (var historyLine in history)
                {
                    // Пропускаем пустые строки
                    if (string.IsNullOrWhiteSpace(historyLine))
                        continue;

                    Messages.Add(new Message
                    {
                        Text = historyLine,
                        SenderName = "История",
                        Timestamp = DateTime.Now,
                        Type = MessageType.System
                    });
                }

                Console.WriteLine($"[OK] Загружено {history.Count} сообщений из истории");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка при загрузке истории: {ex.Message}");
            }
        }
    }
}
