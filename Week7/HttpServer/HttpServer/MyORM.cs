using Microsoft.EntityFrameworkCore;
using HttpServer.Models;

namespace HttpServer
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().ToTable("Accounts");
            modelBuilder.Entity<Account>().HasKey(p => p.Id);
            modelBuilder.Entity<Account>().Property(p => p.Login).IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}
