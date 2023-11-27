using System;
using Microsoft.EntityFrameworkCore;
using PandaDotNet.Tests.Repo.Drivers._TestSetup;

namespace PandaDotNet.Tests.Repo.Drivers.EntityFramework._TestSetup
{
    public class TestDbContext : DbContext
    {
        public TestDbContext() : base(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(Setup.CreateDbConnection())
            .Options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.LogTo((_, _) => true, Console.WriteLine);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contact>()
                .ToTable("contacts")
                .HasKey(i => i.Id);
        }

        public virtual DbSet<Contact> Contacts { get; set; }

        public static TestDbContext Create()
        {
            var context = new TestDbContext();
            context.Database.EnsureCreated();
            return context;
        }
    }
}