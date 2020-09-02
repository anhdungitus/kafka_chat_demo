using System.Collections.Generic;

namespace ChattingClient.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Message> Messages { get; set; }
    }
}