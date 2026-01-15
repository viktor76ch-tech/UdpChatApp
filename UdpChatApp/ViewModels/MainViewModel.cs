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
            set => SetProperty(ref _messageText, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public string MessagesText
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var msg in Messages)
                {
                    string prefix = msg.SenderName == "Вы" ? "[Вы]" : $"[{msg.SenderName}]";
                    sb.AppendLine($"[{msg.Timestamp:HH:mm:ss}] {prefix}: {msg.Text}");
                }
                return sb.ToString();
            }
        }

        public ICommand SendMessageCommand { get; }
        public ICommand ToggleConnectionCommand { get; }

        public MainViewModel()
        {
            Status = "Не подключен";
            IsConnected = false;

            _networkService = new NetworkService();
            _networkService.MessageReceived += OnMessageReceived;
            _networkService.StatusChanged += OnNetworkStatusChanged;

            _fileStorageService = new FileStorageService();

            Messages.CollectionChanged += (s, e) => OnPropertyChanged(nameof(MessagesText));

            SendMessageCommand = new RelayCommand(
                execute: SendMessage,
                canExecute: CanSendMessage);

            ToggleConnectionCommand = new RelayCommand(
                execute: ToggleConnection,
                canExecute: () => true);
        }

        private bool CanSendMessage()
        {
            return !string.IsNullOrWhiteSpace(MessageText) && IsConnected;
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageText))
                return;

            _networkService.SendMessage(MessageText);
            MessageText = string.Empty;
        }

        private void ToggleConnection()
        {
            if (IsConnected)
            {
                _networkService.Disconnect();
                IsConnected = false;
            }
            else
            {
                // Для теста используем разные порты
                int listenPort = 5000;
                int sendPort = 5001;

                _networkService.Connect("Пользователь1", "127.0.0.1", listenPort, sendPort);
                IsConnected = true;

                AddSystemMessage("Подключение установлено");
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

        private void OnNetworkStatusChanged(object sender, string status)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Status = status;
            });
        }

        private void AddSystemMessage(string text)
        {
            Messages.Add(new Message
            {
                Text = text,
                SenderName = "Система",
                Timestamp = DateTime.Now,
                Type = MessageType.System
            });
        }
    }
}