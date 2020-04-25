using System.Data.Entity;

namespace TelegramBotTranslate.Users
{

    class UserContext : DbContext
    {
        public UserContext()
            : base("DbConnect")
        { }

        public DbSet<User> Users { get; set; }
    }

}
