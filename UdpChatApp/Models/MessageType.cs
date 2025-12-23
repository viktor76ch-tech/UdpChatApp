namespace UdpChatApp.Models
{
    public class MessageType
    {
        public enum Message
        {
            Public,
            Private,
            Group,
            System
        }
    }
}
