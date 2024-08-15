using System.Data;
using System.Diagnostics.Eventing.Reader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Server.Data;

public class DataBaseConnection : DbContext
{
    public DbSet<UserData> Users { get; set; }
    public DbSet<AuthorizationTokens> AuthorizationTokens { get; set; }
    public DbSet<Roles> Roles { get; set; }
    public DbSet<Chats> Chats { get; set; }
    public DbSet<Message> Message { get; set; }
    public DbSet<DBItems> Items { get; set; }
    public DbSet<Item> Item { get; set; }

    public DataBaseConnection()
    {
        //подключение к бд
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //строка подключения к бд
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=RTS;Username=postgres;Password=123");
    }
}