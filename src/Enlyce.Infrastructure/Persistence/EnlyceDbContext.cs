using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence;

public class EnlyceDbContext : DbContext
{
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Inmueble> Inmuebles => Set<Inmueble>();
    public DbSet<Propietario> Propietarios => Set<Propietario>();
    public DbSet<Asesor> Asesores => Set<Asesor>();

    public EnlyceDbContext(DbContextOptions<EnlyceDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnlyceDbContext).Assembly);
    }
}
