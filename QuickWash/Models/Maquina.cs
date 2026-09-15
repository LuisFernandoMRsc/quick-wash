using System.ComponentModel.DataAnnotations;

namespace QuickWash.Models;

public class Maquina
{
    public int Id { get; set; }

    [Required]
    public int Numero { get; set; }

    [Required]
    [MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Modelo { get; set; } = "Industrial Eco-Speed";

    public int CapacidadKg { get; set; } = 10;

    [Required]
    [MaxLength(30)]
    public string EstadoOperativo { get; set; } = EstadosMaquina.Operativa;

    [MaxLength(250)]
    public string Descripcion { get; set; } = "Lavadora de carga frontal de alta eficiencia";

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
