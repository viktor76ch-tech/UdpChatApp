using System;

namespace UdpChatApp.Models
{
    public class Message : ObservableObject
    {
        private string _text;
        private string _senderName;
        private string _recipient;
        private MessageType _type;
        private DateTime _timestamp;

        // Пустой конструктор
        public Message()
        {
            _text = string.Empty;
            _senderName = string.Empty;
            _recipient = string.Empty;
            _type = MessageType.Public;
            _timestamp = DateTime.Now;
        }

        // Конструктор с параметрами
        public Message(string text, string senderName, MessageType type = MessageType.Public, string recipient = "")
        {
            _text = text;
            _senderName = senderName;
            _recipient = recipient;
            _type = type;
            _timestamp = DateTime.Now;
        }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public string SenderName
        {
            get => _senderName;
            set => SetProperty(ref _senderName, value);
        }

        public string SenderIp { get; set; }
        public int SenderPort { get; set; }

        public string Recipient
        {
            get => _recipient;
            set => SetProperty(ref _recipient, value);
        }

        public MessageType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }
    }
}