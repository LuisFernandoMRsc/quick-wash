using QuickWash.Models;
using QuickWash.Models.ViewModels;

namespace QuickWash.Services;

public interface IReservaService
{
    Task<(bool Exito, string Mensaje, Reserva? Reserva)> CrearReservaAsync(int estudianteId, NuevaReservaViewModel model);
    Task<(bool Exito, string Mensaje)> CancelarReservaEstudianteAsync(int estudianteId, int reservaId);
    Task<(bool Exito, string Mensaje)> ModificarEstadoPersonalAsync(int reservaId, string nuevoEstado);
    Task<List<Reserva>> ObtenerReservasEstudianteAsync(int estudianteId);
    Task<List<Reserva>> ObtenerTodasLasReservasAsync(DateTime? fecha = null, string? estado = null);
    Task<DisponibilidadViewModel> ObtenerDisponibilidadAsync(DateTime fecha);
    bool ValidarCorreoEstudiantil(string email);
}
