using System.Data;
using System.Diagnostics.Eventing.Reader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Server.Data;

public class DataBaseConnection : DbContext
{
    public DbSet<UserData> Users { get; set; }
    public DbSet<AuthorizationToken> Authorization { get; set; }
    public DbSet<RecoveryCodes> RecoveryCodes { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RoomChat> RoomChat { get; set; }
    public DbSet<PrivateChat> PrivateChat { get; set; }
    public DbSet<Message> Message { get; set; }
    public DbSet<Message> PrivateMessage { get; set; }
    public DbSet<Inventory> Inventory { get; set; }
    public DbSet<DBItems> Items { get; set; }
    public DbSet<Item> Item { get; set; }
    public DbSet<ChatsFilterWords> ChatsFilterWords { get; set; }
    public DbSet<Stats> Stats { get; set; }
    public DbSet<Room> Room { get; set; }
    public DbSet<RoomUsers> RoomUsers { get; set; }

    public DataBaseConnection()
    {
        //подключение к бд
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //строка подключения к бд
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=RoleGame;Username=postgres;Password=123");
    }
}