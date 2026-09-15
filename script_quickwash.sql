-- =========================================================================
-- SISTEMA 'QUICK WASH' - LAVANDERÍA UNIVERSITARIA (UNIVALLE)
-- SCRIPT DE CREACIÓN Y POBLACIÓN DE BASE DE DATOS
-- Compatible con SQLite, PostgreSQL y adaptable a SQL Server / MySQL
-- =========================================================================

-- 1. TABLA: Usuarios (Estudiantes y Personal)
CREATE TABLE IF NOT EXISTS "Usuarios" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    "Apellido" TEXT NOT NULL,
    "Email" TEXT NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "CodigoEstudiante" TEXT NULL,
    "Rol" TEXT NOT NULL, -- 'Estudiante' o 'Personal'
    "FechaRegistro" TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Usuarios_Email" ON "Usuarios" ("Email");

-- 2. TABLA: Maquinas (Lavadoras del campus)
CREATE TABLE IF NOT EXISTS "Maquinas" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "Numero" INTEGER NOT NULL UNIQUE,
    "Nombre" TEXT NOT NULL,
    "Modelo" TEXT NOT NULL DEFAULT 'Industrial Eco-Speed',
    "CapacidadKg" INTEGER NOT NULL DEFAULT 10,
    "EstadoOperativo" TEXT NOT NULL DEFAULT 'Operativa', -- 'Operativa' o 'Mantenimiento'
    "Descripcion" TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Maquinas_Numero" ON "Maquinas" ("Numero");

-- 3. TABLA: Reservas (Turnos de lavado con control de reglas de negocio)
CREATE TABLE IF NOT EXISTS "Reservas" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "CodigoReserva" TEXT NOT NULL UNIQUE,
    "UsuarioId" INTEGER NOT NULL,
    "MaquinaId" INTEGER NOT NULL,
    "Fecha" TEXT NOT NULL,
    "HoraInicio" TEXT NOT NULL, -- Formato 'HH:MM:SS'
    "HoraFin" TEXT NOT NULL,    -- Formato 'HH:MM:SS'
    "CantidadPrendas" INTEGER NOT NULL CHECK ("CantidadPrendas" >= 1 AND "CantidadPrendas" <= 100),
    "Estado" TEXT NOT NULL DEFAULT 'Pendiente', -- 'Pendiente', 'Proceso', 'Finalizada', 'Cancelada'
    "FechaCreacion" TEXT NOT NULL DEFAULT (datetime('now')),
    "Notas" TEXT NULL,
    FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE RESTRICT,
    FOREIGN KEY ("MaquinaId") REFERENCES "Maquinas" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Reservas_CodigoReserva" ON "Reservas" ("CodigoReserva");
CREATE INDEX IF NOT EXISTS "IX_Reservas_UsuarioId" ON "Reservas" ("UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_Reservas_MaquinaId" ON "Reservas" ("MaquinaId");

-- =========================================================================
-- DATOS SEMILLA (SEED DATA)
-- =========================================================================

-- 1. Insertar Personal de Lavandería
-- Contraseña hash para 'Personal123!': AQAAAAIAAYagAAAAEG3... (gestionada por PasswordHasher)
INSERT OR IGNORE INTO "Usuarios" ("Id", "Nombre", "Apellido", "Email", "PasswordHash", "CodigoEstudiante", "Rol", "FechaRegistro")
VALUES 
(1, 'Carlos', 'Gutiérrez', 'personal@univalle.edu', 'AQAAAAIAAYagAAAAELq2n6bV0x00B2q/XWkZz2mB8l1bFj6u', 'PERS-001', 'Personal', datetime('now')),
(2, 'Mariela', 'Rojas', 'operador.lavanderia@univalle.edu', 'AQAAAAIAAYagAAAAELq2n6bV0x00B2q/XWkZz2mB8l1bFj6u', 'PERS-002', 'Personal', datetime('now'));

-- 2. Insertar Estudiantes de Prueba (Dominio estricto @est.univalle.edu)
INSERT OR IGNORE INTO "Usuarios" ("Id", "Nombre", "Apellido", "Email", "PasswordHash", "CodigoEstudiante", "Rol", "FechaRegistro")
VALUES 
(3, 'Juan', 'Pérez', 'juan.perez@est.univalle.edu', 'AQAAAAIAAYagAAAAELq2n6bV0x00B2q/XWkZz2mB8l1bFj6u', 'EST-89412', 'Estudiante', datetime('now')),
(4, 'María', 'López', 'maria.lopez@est.univalle.edu', 'AQAAAAIAAYagAAAAELq2n6bV0x00B2q/XWkZz2mB8l1bFj6u', 'EST-74219', 'Estudiante', datetime('now'));

-- 3. Insertar Máquinas Lavadoras
INSERT OR IGNORE INTO "Maquinas" ("Id", "Numero", "Nombre", "Modelo", "CapacidadKg", "EstadoOperativo", "Descripcion")
VALUES 
(1, 1, 'Lavadora 01 (EcoSpeed)', 'Industrial Eco-Speed', 10, 'Operativa', 'Carga frontal, ciclo rápido de 45 min'),
(2, 2, 'Lavadora 02 (EcoSpeed)', 'Industrial Eco-Speed', 10, 'Operativa', 'Carga frontal, ciclo rápido de 45 min'),
(3, 3, 'Lavadora 03 (HeavyDuty)', 'Industrial HeavyDuty', 15, 'Operativa', 'Gran capacidad para sábanas y toallas'),
(4, 4, 'Lavadora 04 (HeavyDuty)', 'Industrial HeavyDuty', 15, 'Operativa', 'Gran capacidad para cargas pesadas'),
(5, 5, 'Lavadora 05 (SmartWash)', 'SmartWash Pro', 12, 'Operativa', 'Detección automática de peso y centrifugado'),
(6, 6, 'Lavadora 06 (Mantenimiento)', 'Industrial Eco-Speed', 10, 'Mantenimiento', 'En revisión técnica preventiva');

-- 4. Insertar Reservas de Ejemplo con CantidadPrendas
INSERT OR IGNORE INTO "Reservas" ("Id", "CodigoReserva", "UsuarioId", "MaquinaId", "Fecha", "HoraInicio", "HoraFin", "CantidadPrendas", "Estado", "FechaCreacion", "Notas")
VALUES 
(1, 'QW-A8F12B', 3, 1, date('now'), '09:00:00', '10:00:00', 14, 'Pendiente', datetime('now'), 'Ropa deportiva y camisetas'),
(2, 'QW-C4D93E', 3, 2, date('now'), '11:00:00', '12:00:00', 20, 'Proceso', datetime('now'), 'Pantalones y toallas');
