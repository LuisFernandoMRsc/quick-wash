using System.ComponentModel.DataAnnotations;

namespace QuickWash.Models;

public class Reserva
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string CodigoReserva { get; set; } = string.Empty;

    [Required]
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    [Required]
    public int MaquinaId { get; set; }
    public Maquina Maquina { get; set; } = null!;

    [Required]
    public DateTime Fecha { get; set; }

    [Required]
    public TimeSpan HoraInicio { get; set; }

    [Required]
    public TimeSpan HoraFin { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "La cantidad de prendas debe ser entre 1 y 100.")]
    [Display(Name = "Cantidad de Prendas")]
    public int CantidadPrendas { get; set; }

    [Required]
    [MaxLength(30)]
    public string Estado { get; set; } = EstadosReserva.Pendiente;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [MaxLength(300)]
    public string? Notas { get; set; }
}
