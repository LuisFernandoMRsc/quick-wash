using System.ComponentModel.DataAnnotations;

namespace QuickWash.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? CodigoEstudiante { get; set; }

    [Required]
    [MaxLength(30)]
    public string Rol { get; set; } = Roles.Estudiante;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public string NombreCompleto => $"{Nombre} {Apellido}";

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
