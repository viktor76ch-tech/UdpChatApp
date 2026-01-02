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
        public ICommand SendMessadgeCommand { get; }
        public ICommand ToggleConnectionCommand { get; }

        public MainViewModel()
        {
            Status = "Не подключен";
            IsConnected = false;
            _networkService = new NetworkService();
            _networkService.MessageReceived += OnMessageReceived;
            Messages.CollectionChanged += (s, e) => OnPropertyChanged(nameof(MessagesText));
            SendMessadgeCommand = new RelayCommand(execute: SendMessadge, 
                canExecute: CanSendMessadge);

            ToggleConnectionCommand = new RelayCommand(execute: ToggleConnection, 
                canExecute : () => true);
        }
        private bool CanSendMessadge()
        {
            return !string.IsNullOrWhiteSpace(MessageText) && IsConnected; //Проверим, что текст не пустой и не из пробелов и есть подключение к чату
        }

        private void SendMessadge()
        {
            var message = new Message
            {
                Text = MessageText,
                SenderName = "Я",
                Timestamp = DateTime.Now,
                Type = MessageType.Public
            };

            Messages.Add(message);
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
            });
        }
    }
}
