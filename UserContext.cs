using System.Data.Entity;

namespace TelegramBotTranslate
{
    class UserContext : DbContext
    {
        public UserContext()
            : base("TranslateDb")
        { }

        public DbSet<User> Users { get; set; }
    }
}
