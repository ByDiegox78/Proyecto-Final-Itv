using Microsoft.EntityFrameworkCore;

namespace ITV_Avanzado.Entity;
/// <summary>
///     Contexto de Entity Framework Core para la base de datos de citas.
/// </summary>
public class AppDbContext : DbContext {
    public DbSet<CitaEntity> Citas { get; set; } = null!;

    private readonly string _connectionString;

    public AppDbContext(string connectionString) {
        _connectionString = connectionString;
    }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        _connectionString = "";
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder op) {
        if (!op.IsConfigured) op.UseSqlite(_connectionString);
    }
    public void EnsurceCreated() {
        Database.EnsureCreated();
    }
}