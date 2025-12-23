using System;
using System.ComponentModel;

namespace UdpChatApp.Models
{
    public class User : ObservableObject
    {
        private string _name;
        private string _status;
        private string _ipAddress;
        private int _port;
        private DateTime _lastSeen;

        public User() 
        {
            _name = string.Empty;
            _status = "Offline";
            _ipAddress = string.Empty;
            _port = 0;
            _lastSeen = DateTime.Now;
        }

        public User(string name, string ipAddress, int port)
        {
            _name = name;
            _status = "Online";
            _ipAddress = ipAddress;
            _port = port;
            _lastSeen = DateTime.Now;
        }

        public int Id { get; set; }
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }
        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public DateTime LartSeen
        {
            get => _lastSeen;
            set => SetProperty(ref _lastSeen, value);
        }
    }
}
