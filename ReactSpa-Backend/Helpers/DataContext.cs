namespace ReactSpa_Backend.Helpers;

using Microsoft.EntityFrameworkCore;
using ReactSpa_Backend.Entities;

public class DataContext : DbContext
{


    protected readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(Configuration.GetConnectionString("Postgres"));
    }

    public DbSet<User> Users { get; set; }
}