using System.ComponentModel.DataAnnotations;

namespace QuickWash.Models.ViewModels;

public class RegistroViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código de estudiante es obligatorio.")]
    [Display(Name = "Código de Estudiante")]
    public string CodigoEstudiante { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo no válido.")]
    [Display(Name = "Correo Institucional")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Contraseña")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo no válido.")]
    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordarme en este equipo")]
    public bool Recordarme { get; set; }
}

public class NuevaReservaViewModel
{
    [Required(ErrorMessage = "Debe seleccionar una máquina.")]
    [Display(Name = "Máquina")]
    public int MaquinaId { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de Reserva")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
    [Display(Name = "Hora de Inicio")]
    public TimeSpan HoraInicio { get; set; } = new TimeSpan(8, 0, 0);

    [Required(ErrorMessage = "La hora de fin es obligatoria.")]
    [Display(Name = "Hora de Fin")]
    public TimeSpan HoraFin { get; set; } = new TimeSpan(9, 0, 0);

    [Required(ErrorMessage = "Debe ingresar la cantidad de prendas.")]
    [Range(1, 100, ErrorMessage = "La cantidad de prendas debe estar entre 1 y 100.")]
    [Display(Name = "Cantidad de Prendas")]
    public int CantidadPrendas { get; set; } = 10;

    [MaxLength(250, ErrorMessage = "Las notas no pueden superar 250 caracteres.")]
    [Display(Name = "Notas / Tipo de Ropa")]
    public string? Notas { get; set; }
}

public class CambiarEstadoViewModel
{
    [Required]
    public int ReservaId { get; set; }

    [Required]
    [Display(Name = "Nuevo Estado")]
    public string NuevoEstado { get; set; } = EstadosReserva.Pendiente;

    public string? Motivo { get; set; }
}

public class DisponibilidadViewModel
{
    public DateTime Fecha { get; set; } = DateTime.Today;
    public List<Maquina> Maquinas { get; set; } = new();
    public List<Reserva> ReservasDelDia { get; set; } = new();
    public List<FranjaHorariaDto> Franjas { get; set; } = new();
}

public class FranjaHorariaDto
{
    public TimeSpan Inicio { get; set; }
    public TimeSpan Fin { get; set; }
    public string Etiqueta => $"{Inicio:hh\\:mm} - {Fin:hh\\:mm}";
}
