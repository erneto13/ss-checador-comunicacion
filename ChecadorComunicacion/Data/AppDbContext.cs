using ChecadorComunicacion.Services;
using Microsoft.EntityFrameworkCore;

namespace ChecadorComunicacion.Data;

public class AppDbContext : DbContext
{
    public DbSet<Persona> Personas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = "server=localhost;user=root;password=admin;database=checador_curn";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
}