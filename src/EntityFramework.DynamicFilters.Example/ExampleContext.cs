﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.DynamicFilters.Example
{
    public class ExampleContext : DbContext
    {
        //  A static/globally scoped value that will be used to restrict queries against the BlogEntries table.
        public static Guid CurrentAccountID { get; set; }

        public ExampleContext()
        {
            Database.SetInitializer<ExampleContext>(new ContentInitializer());
            Database.Log = log => System.Diagnostics.Debug.WriteLine(log);
            Database.Initialize(false);
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<BlogEntry> BlogEntries { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Initialize the Dynamic Filters
            this.InitializeDynamicFilters();

            //  Filter to restrict all queries on BlogEntries to only those for the current user.
            //  Global filter is used here with a delegate so that it's evaluated every time it's needed.
            //  ** IMPORTANT: Make sure if a delegate/Func<object> is used for the value that it has *GLOBAL*
            //  scope in your application.  In this example, CurrentAccountID is static.  If it were not static,
            //  setting the CurrentAccountID would have no effect since the Func would be using the instance of
            //  the DbContext that was used in the call to OnModelCreating - which only happens once per app lifetime.
            modelBuilder.Filter("BlogEntriesForCurrentUser", (BlogEntry b) => b.AccountID, () => CurrentAccountID);

            //  Global filter on any class implementing ISoftDelete to match on records with IsDeleted=false
            modelBuilder.Filter("IsDeleted", (ISoftDelete d) => d.IsDeleted, false);
        }
    }

    public class ContentInitializer : DropCreateDatabaseAlways<ExampleContext>
    {
        protected override void Seed(ExampleContext context)
        {
            System.Diagnostics.Debug.Print("Seeding db");

            //  Seeds 2 accounts with 9 blog entries, 4 of which are deleted

            var homer = new Account
            {
                UserName = "homer",
                BlogEntries = new List<BlogEntry>
                {
                    new BlogEntry { Body="Homer's first blog entry", IsDeleted=false},
                    new BlogEntry { Body="Homer's second blog entry", IsDeleted=false},
                    new BlogEntry { Body="Homer's third blog entry (deleted)", IsDeleted=true},
                    new BlogEntry { Body="Homer's fourth blog entry (deleted)", IsDeleted=true},
                }
            };
            context.Accounts.Add(homer);

            var bart = new Account
            {
                UserName = "bart",
                BlogEntries = new List<BlogEntry>
                {
                    new BlogEntry { Body="Bart's first blog entry", IsDeleted=false},
                    new BlogEntry { Body="Bart's second blog entry", IsDeleted=false},
                    new BlogEntry { Body="Bart's third blog entry", IsDeleted=false},
                    new BlogEntry { Body="Bart's fourth blog entry (deleted)", IsDeleted=true},
                    new BlogEntry { Body="Bart's fifth blog entry (deleted)", IsDeleted=true},
                }
            };
            context.Accounts.Add(bart);

            context.SaveChanges();
        }
    }
}
