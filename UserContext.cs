using System.Data.Entity;

namespace TelegramBotTranslate
{
    class UserContext : DbContext
    {
        public UserContext()
            : base("TranslateBotDb")
        { }

        public DbSet<User> Users { get; set; }
    }
}
