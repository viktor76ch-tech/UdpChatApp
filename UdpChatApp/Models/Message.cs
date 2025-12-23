using System;
using System.Net.WebSockets;

namespace UdpChatApp.Models
{
    public class Message : ObservableObject
    {
        private string _text;
        private string _senderName;
        private string _recipient;
        private MessageType _type;
        private DateTime _timestamp;
    }
}
