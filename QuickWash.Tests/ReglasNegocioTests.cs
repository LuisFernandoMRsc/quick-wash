using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QuickWash.Data;
using QuickWash.Models;
using QuickWash.Models.ViewModels;
using QuickWash.Services;
using Xunit;

namespace QuickWash.Tests;

public class ReglasNegocioTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly QuickWashDbContext _context;
    private readonly ReservaService _reservaService;

    public ReglasNegocioTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<QuickWashDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new QuickWashDbContext(options);
        _context.Database.EnsureCreated();

        // Crear datos base para pruebas
        var estudiante = new Usuario
        {
            Id = 1,
            Nombre = "Carlos",
            Apellido = "Vargas",
            Email = "carlos.vargas@est.univalle.edu",
            CodigoEstudiante = "EST-11111",
            Rol = Roles.Estudiante,
            PasswordHash = "hash123"
        };

        var maquina1 = new Maquina
        {
            Id = 1,
            Numero = 1,
            Nombre = "Lavadora 01",
            EstadoOperativo = EstadosMaquina.Operativa,
            CapacidadKg = 10
        };

        var maquina2 = new Maquina
        {
            Id = 2,
            Numero = 2,
            Nombre = "Lavadora 02",
            EstadoOperativo = EstadosMaquina.Operativa,
            CapacidadKg = 12
        };

        _context.Usuarios.Add(estudiante);
        _context.Maquinas.AddRange(maquina1, maquina2);
        _context.SaveChanges();

        _reservaService = new ReservaService(_context);
    }

    [Theory]
    [InlineData("carlos.vargas@est.univalle.edu", true)]
    [InlineData("estudiante.prueba@est.univalle.edu", true)]
    [InlineData("usuario@gmail.com", false)]
    [InlineData("usuario@univalle.edu", false)] // No es dominio estudiantil
    [InlineData("estudiante@hotmail.com", false)]
    [InlineData("", false)]
    public void ValidarCorreoEstudiantil_SoloAceptaDominioEstUnivalleEdu(string email, bool esperado)
    {
        var resultado = _reservaService.ValidarCorreoEstudiantil(email);
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public async Task RN1_NoPermiteSolapamientoDeHorario_EnMismaMaquina()
    {
        // Arrange: Crear primera reserva de 08:00 a 09:00
        var hoy = DateTime.Today.AddDays(1);
        var r1 = new NuevaReservaViewModel
        {
            MaquinaId = 1,
            Fecha = hoy,
            HoraInicio = new TimeSpan(8, 0, 0),
            HoraFin = new TimeSpan(9, 0, 0),
            CantidadPrendas = 10
        };
        var (exito1, _, _) = await _reservaService.CrearReservaAsync(1, r1);
        Assert.True(exito1);

        // Act: Intentar reservar la misma máquina con solapamiento (08:30 a 09:30)
        var rSolapada = new NuevaReservaViewModel
        {
            MaquinaId = 1,
            Fecha = hoy,
            HoraInicio = new TimeSpan(8, 30, 0),
            HoraFin = new TimeSpan(9, 30, 0),
            CantidadPrendas = 15
        };
        var (exitoSolapada, mensajeError, _) = await _reservaService.CrearReservaAsync(1, rSolapada);

        // Assert: Debe ser rechazada
        Assert.False(exitoSolapada);
        Assert.Contains("Regla de Negocio", mensajeError);
        Assert.Contains("ya se encuentra reservada", mensajeError);
    }

    [Fact]
    public async Task RN2_EstudianteNoPuedeTenerMasDe3ReservasActivas()
    {
        var hoy = DateTime.Today.AddDays(1);

        // Crear 3 reservas activas en diferentes horarios/máquinas
        var r1 = new NuevaReservaViewModel { MaquinaId = 1, Fecha = hoy, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(9, 0, 0), CantidadPrendas = 5 };
        var r2 = new NuevaReservaViewModel { MaquinaId = 1, Fecha = hoy, HoraInicio = new TimeSpan(9, 0, 0), HoraFin = new TimeSpan(10, 0, 0), CantidadPrendas = 8 };
        var r3 = new NuevaReservaViewModel { MaquinaId = 2, Fecha = hoy, HoraInicio = new TimeSpan(10, 0, 0), HoraFin = new TimeSpan(11, 0, 0), CantidadPrendas = 12 };

        var (e1, _, _) = await _reservaService.CrearReservaAsync(1, r1);
        var (e2, _, _) = await _reservaService.CrearReservaAsync(1, r2);
        var (e3, _, _) = await _reservaService.CrearReservaAsync(1, r3);

        Assert.True(e1);
        Assert.True(e2);
        Assert.True(e3);

        // Act: Intentar crear la 4ta reserva
        var r4 = new NuevaReservaViewModel { MaquinaId = 2, Fecha = hoy, HoraInicio = new TimeSpan(11, 0, 0), HoraFin = new TimeSpan(12, 0, 0), CantidadPrendas = 7 };
        var (e4, mensaje, _) = await _reservaService.CrearReservaAsync(1, r4);

        // Assert: Debe rechazarse por límite de 3
        Assert.False(e4);
        Assert.Contains("límite máximo permitido de 3 reservas activas", mensaje);
    }

    [Fact]
    public async Task RN3_PersonalSoloPuedeModificarAEstadosValidos()
    {
        var hoy = DateTime.Today.AddDays(1);
        var model = new NuevaReservaViewModel { MaquinaId = 1, Fecha = hoy, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(9, 0, 0), CantidadPrendas = 8 };
        var (_, _, reserva) = await _reservaService.CrearReservaAsync(1, model);
        Assert.NotNull(reserva);

        // Act 1: Cambiar a 'Proceso' (válido)
        var (exitoProceso, _) = await _reservaService.ModificarEstadoPersonalAsync(reserva.Id, EstadosReserva.Proceso);
        Assert.True(exitoProceso);

        // Act 2: Cambiar a 'Finalizada' (válido)
        var (exitoFinalizada, _) = await _reservaService.ModificarEstadoPersonalAsync(reserva.Id, EstadosReserva.Finalizada);
        Assert.True(exitoFinalizada);

        // Act 3: Intentar asignar un estado inválido inventado
        var (exitoInvalido, mensaje) = await _reservaService.ModificarEstadoPersonalAsync(reserva.Id, "EstadoInvalidoCualquiera");
        Assert.False(exitoInvalido);
        Assert.Contains("Estado no válido", mensaje);
    }

    [Fact]
    public async Task RN4_EstudianteSoloPuedeCancelarSiEstadoEsPendiente()
    {
        var hoy = DateTime.Today.AddDays(1);
        var model = new NuevaReservaViewModel { MaquinaId = 1, Fecha = hoy, HoraInicio = new TimeSpan(14, 0, 0), HoraFin = new TimeSpan(15, 0, 0), CantidadPrendas = 10 };
        var (_, _, reserva) = await _reservaService.CrearReservaAsync(1, model);
        Assert.NotNull(reserva);
        Assert.Equal(EstadosReserva.Pendiente, reserva.Estado);

        // Act 1: El personal pasa la reserva a 'Proceso'
        await _reservaService.ModificarEstadoPersonalAsync(reserva.Id, EstadosReserva.Proceso);

        // Act 2: El estudiante intenta cancelarla cuando ya está en 'Proceso'
        var (exitoCancelar, mensaje) = await _reservaService.CancelarReservaEstudianteAsync(1, reserva.Id);

        // Assert: Debe ser rechazada
        Assert.False(exitoCancelar);
        Assert.Contains("Solo puedes cancelar reservas en estado 'Pendiente'", mensaje);
    }

    [Fact]
    public async Task CantidadPrendas_SeRegistraCorrectamenteEnLaReserva()
    {
        var hoy = DateTime.Today.AddDays(1);
        var model = new NuevaReservaViewModel
        {
            MaquinaId = 1,
            Fecha = hoy,
            HoraInicio = new TimeSpan(16, 0, 0),
            HoraFin = new TimeSpan(17, 0, 0),
            CantidadPrendas = 23,
            Notas = "Prendas de mezclilla y toallas"
        };

        var (exito, _, reserva) = await _reservaService.CrearReservaAsync(1, model);

        Assert.True(exito);
        Assert.NotNull(reserva);
        Assert.Equal(23, reserva.CantidadPrendas);
        Assert.Equal("Prendas de mezclilla y toallas", reserva.Notas);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
