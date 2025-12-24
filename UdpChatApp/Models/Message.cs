using CommunityToolkit.Mvvm.Messaging;
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

        //Пустой конструктор
        public Message()
        {
             _text = string.Empty;
             _senderName = string.Empty;
             _recipient = string.Empty;
             _type = MessageType.Public;
             _timestamp = DateTime.Now;
        }
        
        // Конструктрор с параметрами
        public Message(string text, string senderName, MessageType type = MessageType.Public, string recipient = "")
        {
            _text = text;
            _senderName = senderName;
            _recipient = recipient;
            _type = type;
            _timestamp = DateTime.Now;
        }
    }
}
