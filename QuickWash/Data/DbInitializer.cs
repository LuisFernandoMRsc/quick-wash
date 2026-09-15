using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuickWash.Models;

namespace QuickWash.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(QuickWashDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        var hasher = new PasswordHasher<Usuario>();

        // 1. Usuarios Semilla
        if (!await context.Usuarios.AnyAsync(u => u.Email == "personal@univalle.edu"))
        {
            var personal = new Usuario
            {
                Nombre = "Carlos",
                Apellido = "Gutiérrez",
                Email = "personal@univalle.edu",
                CodigoEstudiante = "PERS-001",
                Rol = Roles.Personal,
                FechaRegistro = DateTime.UtcNow
            };
            personal.PasswordHash = hasher.HashPassword(personal, "Personal123!");
            context.Usuarios.Add(personal);
        }

        if (!await context.Usuarios.AnyAsync(u => u.Email == "operador.lavanderia@univalle.edu"))
        {
            var operador2 = new Usuario
            {
                Nombre = "Mariela",
                Apellido = "Rojas",
                Email = "operador.lavanderia@univalle.edu",
                CodigoEstudiante = "PERS-002",
                Rol = Roles.Personal,
                FechaRegistro = DateTime.UtcNow
            };
            operador2.PasswordHash = hasher.HashPassword(operador2, "Operador2026!");
            context.Usuarios.Add(operador2);
        }

        if (!await context.Usuarios.AnyAsync(u => u.Email == "juan.perez@est.univalle.edu"))
        {
            var estudiante = new Usuario
            {
                Nombre = "Juan",
                Apellido = "Pérez",
                Email = "juan.perez@est.univalle.edu",
                CodigoEstudiante = "EST-89412",
                Rol = Roles.Estudiante,
                FechaRegistro = DateTime.UtcNow
            };
            estudiante.PasswordHash = hasher.HashPassword(estudiante, "Estudiante123!");
            context.Usuarios.Add(estudiante);
        }

        await context.SaveChangesAsync();

        // 2. Máquinas Lavadoras Semilla
        if (!await context.Maquinas.AnyAsync())
        {
            var maquinas = new List<Maquina>
            {
                new() { Numero = 1, Nombre = "Lavadora 01 (EcoSpeed)", CapacidadKg = 10, EstadoOperativo = EstadosMaquina.Operativa, Descripcion = "Carga frontal, ciclo rápido de 45 min" },
                new() { Numero = 2, Nombre = "Lavadora 02 (EcoSpeed)", CapacidadKg = 10, EstadoOperativo = EstadosMaquina.Operativa, Descripcion = "Carga frontal, ciclo rápido de 45 min" },
                new() { Numero = 3, Nombre = "Lavadora 03 (HeavyDuty)", CapacidadKg = 15, EstadoOperativo = EstadosMaquina.Operativa, Descripcion = "Gran capacidad para sábanas y toallas" },
                new() { Numero = 4, Nombre = "Lavadora 04 (HeavyDuty)", CapacidadKg = 15, EstadoOperativo = EstadosMaquina.Operativa, Descripcion = "Gran capacidad para cargas pesadas" },
                new() { Numero = 5, Nombre = "Lavadora 05 (SmartWash)", CapacidadKg = 12, EstadoOperativo = EstadosMaquina.Operativa, Descripcion = "Detección automática de peso y centrifugado" },
                new() { Numero = 6, Nombre = "Lavadora 06 (Mantenimiento)", CapacidadKg = 10, EstadoOperativo = EstadosMaquina.Mantenimiento, Descripcion = "En revisión técnica preventiva" }
            };

            context.Maquinas.AddRange(maquinas);
            await context.SaveChangesAsync();
        }

        // 3. Reservas iniciales de muestra
        if (!await context.Reservas.AnyAsync())
        {
            var estudiante = await context.Usuarios.FirstOrDefaultAsync(u => u.Rol == Roles.Estudiante);
            var maq1 = await context.Maquinas.FirstOrDefaultAsync(m => m.Numero == 1);
            var maq2 = await context.Maquinas.FirstOrDefaultAsync(m => m.Numero == 2);

            if (estudiante != null && maq1 != null && maq2 != null)
            {
                var hoy = DateTime.Today;
                var reservas = new List<Reserva>
                {
                    new()
                    {
                        CodigoReserva = "QW-" + Guid.NewGuid().ToString()[..6].ToUpper(),
                        UsuarioId = estudiante.Id,
                        MaquinaId = maq1.Id,
                        Fecha = hoy,
                        HoraInicio = new TimeSpan(9, 0, 0),
                        HoraFin = new TimeSpan(10, 0, 0),
                        CantidadPrendas = 14,
                        Estado = EstadosReserva.Pendiente,
                        Notas = "Ropa deportiva y camisetas"
                    },
                    new()
                    {
                        CodigoReserva = "QW-" + Guid.NewGuid().ToString()[..6].ToUpper(),
                        UsuarioId = estudiante.Id,
                        MaquinaId = maq2.Id,
                        Fecha = hoy,
                        HoraInicio = new TimeSpan(11, 0, 0),
                        HoraFin = new TimeSpan(12, 0, 0),
                        CantidadPrendas = 20,
                        Estado = EstadosReserva.Proceso,
                        Notas = "Pantalones y toallas"
                    }
                };

                context.Reservas.AddRange(reservas);
                await context.SaveChangesAsync();
            }
        }
    }
}
