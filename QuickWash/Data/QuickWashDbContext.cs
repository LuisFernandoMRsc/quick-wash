using Microsoft.EntityFrameworkCore;
using QuickWash.Models;

namespace QuickWash.Data;

public class QuickWashDbContext : DbContext
{
    public QuickWashDbContext(DbContextOptions<QuickWashDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Maquina> Maquinas => Set<Maquina>();
    public DbSet<Reserva> Reservas => Set<Reserva>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Maquina>(entity =>
        {
            entity.HasIndex(m => m.Numero).IsUnique();
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasIndex(r => r.CodigoReserva).IsUnique();

            entity.HasOne(r => r.Usuario)
                  .WithMany(u => u.Reservas)
                  .HasForeignKey(r => r.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Maquina)
                  .WithMany(m => m.Reservas)
                  .HasForeignKey(r => r.MaquinaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
