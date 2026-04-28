using Microsoft.EntityFrameworkCore;

namespace ITV_Avanzado.Entity;

public class AppDbContext : DbContext {
    public DbSet<VehiculoEntity> Vehiculos { get; set; } = null!;

    private readonly string _connectionString;

    public AppDbContext(string connectionString) {
        _connectionString = "";
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