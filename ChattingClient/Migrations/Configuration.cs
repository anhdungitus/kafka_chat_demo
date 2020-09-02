using System.Collections.Generic;
using ChattingClient.Domain;

namespace ChattingClient.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ChattingClient.ChatDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ChattingClient.ChatDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.

            IList<User> users = new List<User>();
            users.Add(new User() { Name = "Dzung Nguyen"});
            users.Add(new User() { Name = "Hoan Nguyen"});
            context.Users.AddRange(users);
            base.Seed(context);

        }
    }
}
