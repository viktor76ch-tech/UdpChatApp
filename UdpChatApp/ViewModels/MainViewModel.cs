using System;
using System.Collections.ObjectModel;
using UdpChatApp.Models;

namespace UdpChatApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string _messageText;
        private string _status;
        private string _isConnected;

        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();
    }
}
