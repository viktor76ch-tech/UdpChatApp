using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UdpChatApp.Models;

namespace UdpChatApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string _messageText;
        private string _status;
        private string _isConnected;

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
        public string IsConnected
        {
            get => _isConnected;
            set { SetProperty(ref _isConnected, value); }
        }

        //команды
        public ICommand SendMessadgeCommand { get; }
        public ICommand ToggleConnectionCommand { get; }
    }
}
