using ChattingClient.Domain;
using System.Data.Entity;

namespace ChattingClient
{
    public class ChatDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}