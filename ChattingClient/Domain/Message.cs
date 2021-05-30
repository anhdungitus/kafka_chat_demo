using System;

namespace ChattingClient.Domain
{
    public class Message
    {
        public int Id { get; set; }
        public DateTime SendTime { get; set; }
        public string Content { get; set; }

        public int? SenderId { get; set; }
        public int? ReceiverId { get; set; }

        public virtual User Sender { get; set; }
        public virtual User Receiver { get; set; }
    }
}