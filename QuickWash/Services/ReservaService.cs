using Microsoft.EntityFrameworkCore;
using QuickWash.Data;
using QuickWash.Models;
using QuickWash.Models.ViewModels;

namespace QuickWash.Services;

public class ReservaService : IReservaService
{
    private readonly QuickWashDbContext _context;
    public const string DominioEstudianteValido = "@est.univalle.edu";

    public ReservaService(QuickWashDbContext context)
    {
        _context = context;
    }

    public bool ValidarCorreoEstudiantil(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return email.Trim().EndsWith(DominioEstudianteValido, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<(bool Exito, string Mensaje, Reserva? Reserva)> CrearReservaAsync(int estudianteId, NuevaReservaViewModel model)
    {
        // Validación de fecha y horas
        if (model.HoraFin <= model.HoraInicio)
        {
            return (false, "La hora de fin debe ser posterior a la hora de inicio.", null);
        }

        if (model.Fecha.Date < DateTime.Today)
        {
            return (false, "No se pueden realizar reservas para fechas pasadas.", null);
        }

        // Validación de cantidad de prendas
        if (model.CantidadPrendas < 1 || model.CantidadPrendas > 100)
        {
            return (false, "La cantidad de prendas debe ser entre 1 y 100.", null);
        }

        // Verificar que la máquina exista y esté operativa
        var maquina = await _context.Maquinas.FindAsync(model.MaquinaId);
        if (maquina == null)
        {
            return (false, "La máquina seleccionada no existe.", null);
        }

        if (maquina.EstadoOperativo != EstadosMaquina.Operativa)
        {
            return (false, $"La máquina #{maquina.Numero} no está disponible actualmente ({maquina.EstadoOperativo}).", null);
        }

        // REGLA DE NEGOCIO 2: Un estudiante puede tener reservadas hasta un total de 3 máquinas (reservas activas: Pendiente o Proceso)
        var reservasActivasEstudiante = await _context.Reservas
            .Where(r => r.UsuarioId == estudianteId &&
                       (r.Estado == EstadosReserva.Pendiente || r.Estado == EstadosReserva.Proceso))
            .CountAsync();

        if (reservasActivasEstudiante >= 3)
        {
            return (false, "Regla de Negocio: Has alcanzado el límite máximo permitido de 3 reservas activas. Espera a que finalicen o cancela alguna pendiente.", null);
        }

        // REGLA DE NEGOCIO 1: Una máquina no puede ser reservada por más de 1 estudiante en el mismo horario
        // Comprobar solapamiento de horarios en la misma máquina y fecha
        // Solapamiento: (NuevoInicio < ExistenteFin) && (NuevoFin > ExistenteInicio)
        var fechaFiltro = model.Fecha.Date;
        var reservasExistentesDia = await _context.Reservas
            .Where(r => r.MaquinaId == model.MaquinaId
                        && r.Fecha >= fechaFiltro
                        && r.Fecha < fechaFiltro.AddDays(1)
                        && r.Estado != EstadosReserva.Cancelada)
            .ToListAsync();

        var solapamiento = reservasExistentesDia.Any(r =>
            model.HoraInicio < r.HoraFin && model.HoraFin > r.HoraInicio);

        if (solapamiento)
        {
            return (false, $"Regla de Negocio: La máquina #{maquina.Numero} ya se encuentra reservada en el horario {model.HoraInicio:hh\\:mm} - {model.HoraFin:hh\\:mm}. Elige otro horario o máquina.", null);
        }

        // Generación de código único de reserva (ej. QW-A8F12B)
        var codigo = "QW-" + Guid.NewGuid().ToString()[..6].ToUpper();

        var reserva = new Reserva
        {
            CodigoReserva = codigo,
            UsuarioId = estudianteId,
            MaquinaId = model.MaquinaId,
            Fecha = model.Fecha.Date,
            HoraInicio = model.HoraInicio,
            HoraFin = model.HoraFin,
            CantidadPrendas = model.CantidadPrendas,
            Estado = EstadosReserva.Pendiente,
            FechaCreacion = DateTime.UtcNow,
            Notas = model.Notas
        };

        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();

        return (true, $"¡Reserva confirmada con éxito! Código: {codigo}", reserva);
    }

    public async Task<(bool Exito, string Mensaje)> CancelarReservaEstudianteAsync(int estudianteId, int reservaId)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Maquina)
            .FirstOrDefaultAsync(r => r.Id == reservaId && r.UsuarioId == estudianteId);

        if (reserva == null)
        {
            return (false, "No se encontró la reserva especificada o no te pertenece.");
        }

        // REGLA DE NEGOCIO 4: Un estudiante solo puede cancelar sus reservas si están en estado 'Pendiente'
        if (reserva.Estado != EstadosReserva.Pendiente)
        {
            return (false, $"Regla de Negocio: Solo puedes cancelar reservas en estado 'Pendiente'. Tu reserva actual está en estado '{reserva.Estado}'.");
        }

        reserva.Estado = EstadosReserva.Cancelada;
        await _context.SaveChangesAsync();

        return (true, $"La reserva #{reserva.CodigoReserva} para la máquina #{reserva.Maquina.Numero} ha sido cancelada exitosamente.");
    }

    public async Task<(bool Exito, string Mensaje)> ModificarEstadoPersonalAsync(int reservaId, string nuevoEstado)
    {
        // REGLA DE NEGOCIO 3: El personal puede modificar únicamente el estado de las reservas (Pendiente, Proceso, Finalizada, Cancelada)
        if (!EstadosReserva.Todos.Contains(nuevoEstado))
        {
            return (false, $"Estado no válido. Los estados permitidos son: {string.Join(", ", EstadosReserva.Todos)}.");
        }

        var reserva = await _context.Reservas
            .Include(r => r.Maquina)
            .Include(r => r.Usuario)
            .FirstOrDefaultAsync(r => r.Id == reservaId);

        if (reserva == null)
        {
            return (false, "Reserva no encontrada.");
        }

        var estadoAnterior = reserva.Estado;
        reserva.Estado = nuevoEstado;
        await _context.SaveChangesAsync();

        return (true, $"Estado de la reserva #{reserva.CodigoReserva} modificado exitosamente de '{estadoAnterior}' a '{nuevoEstado}'.");
    }

    public async Task<List<Reserva>> ObtenerReservasEstudianteAsync(int estudianteId)
    {
        var lista = await _context.Reservas
            .Include(r => r.Maquina)
            .Where(r => r.UsuarioId == estudianteId)
            .OrderByDescending(r => r.Fecha)
            .ToListAsync();

        return lista
            .OrderByDescending(r => r.Fecha)
            .ThenByDescending(r => r.HoraInicio)
            .ToList();
    }

    public async Task<List<Reserva>> ObtenerTodasLasReservasAsync(DateTime? fecha = null, string? estado = null)
    {
        var query = _context.Reservas
            .Include(r => r.Maquina)
            .Include(r => r.Usuario)
            .AsQueryable();

        if (fecha.HasValue)
        {
            var fechaVal = fecha.Value.Date;
            var fechaSiguiente = fechaVal.AddDays(1);
            query = query.Where(r => r.Fecha >= fechaVal && r.Fecha < fechaSiguiente);
        }

        if (!string.IsNullOrWhiteSpace(estado) && estado != "Todos")
        {
            query = query.Where(r => r.Estado == estado);
        }

        var lista = await query
            .OrderByDescending(r => r.Fecha)
            .ToListAsync();

        return lista
            .OrderByDescending(r => r.Fecha)
            .ThenBy(r => r.HoraInicio)
            .ToList();
    }

    public async Task<DisponibilidadViewModel> ObtenerDisponibilidadAsync(DateTime fecha)
    {
        var fechaFiltro = fecha.Date;
        var fechaSiguiente = fechaFiltro.AddDays(1);

        var maquinas = await _context.Maquinas
            .OrderBy(m => m.Numero)
            .ToListAsync();

        var reservas = await _context.Reservas
            .Include(r => r.Usuario)
            .Where(r => r.Fecha >= fechaFiltro && r.Fecha < fechaSiguiente && r.Estado != EstadosReserva.Cancelada)
            .ToListAsync();

        // Generar franjas de 8:00 a 20:00 (bloques de 1 hora)
        var franjas = new List<FranjaHorariaDto>();
        for (int hora = 8; hora < 20; hora++)
        {
            franjas.Add(new FranjaHorariaDto
            {
                Inicio = new TimeSpan(hora, 0, 0),
                Fin = new TimeSpan(hora + 1, 0, 0)
            });
        }

        return new DisponibilidadViewModel
        {
            Fecha = fechaFiltro,
            Maquinas = maquinas,
            ReservasDelDia = reservas,
            Franjas = franjas
        };
    }
}
