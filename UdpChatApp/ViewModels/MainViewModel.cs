using System.Collections.ObjectModel;
using System.Windows.Input;
using UdpChatApp.Models;


namespace UdpChatApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string _messageText;
        private string _status;
        private bool _isConnected;

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

        //команды
        public ICommand SendMessadgeCommand { get; }
        public ICommand ToggleConnectionCommand { get; }

        public MainViewModel()
        {
            Status = "Не подключен";
            IsConnected = false;

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
            MessageText = string.Empty;
        }

        private void ToggleConnection()
        {
            if (IsConnected)
            {
                Status = "Не подключен";
                IsConnected = false;
            }
            else
            {
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
    }
}
