using System.Windows;
using UdpChatApp.ViewModels;


namespace UdpChatApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}