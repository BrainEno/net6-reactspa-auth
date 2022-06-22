namespace ReactSpa_Backend.Helpers;

using Microsoft.EntityFrameworkCore;

public class SqliteDataContext : DataContext
{
    public SqliteDataContext(IConfiguration configuration) : base(configuration) { }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite(Configuration.GetConnectionString("Sqlite"));
    }
}