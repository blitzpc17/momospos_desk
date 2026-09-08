-- ============================================================
-- SCRIPT DE ACTUALIZACION (ALTER TABLES Y UPSERTS)
-- Especifico para el esquema clinic y MomosClinic
-- ============================================================

-- 1. Campos de Baja y Auditoria para Pacientes
DO $$ 
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'clinic' AND table_name = 'pacientes') THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'clinic' AND table_name = 'pacientes' AND column_name = 'motivobaja') THEN
            ALTER TABLE clinic.Pacientes ADD COLUMN MotivoBaja VARCHAR(500);
            ALTER TABLE clinic.Pacientes ADD COLUMN BajaPor VARCHAR(100);
        END IF;
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'clinic' AND table_name = 'pacientes' AND column_name = 'creadopor') THEN
            ALTER TABLE clinic.Pacientes ADD COLUMN CreadoPor VARCHAR(100);
            ALTER TABLE clinic.Pacientes ADD COLUMN ModificadoPor VARCHAR(100);
        END IF;
    END IF;
END $$;

-- 2. Registrar Módulos de Clínica en la tabla centralizada de MomosPOS (public.Modulos)
ALTER TABLE Modulos ADD COLUMN IF NOT EXISTS Sistema VARCHAR(50) DEFAULT 'POS';

INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono, Sistema) VALUES (20, 'Clínica', 'MenuClinic', NULL, 5, '🏥', 'CLINIC') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono, Sistema) VALUES (21, 'Dashboard Clínica', 'DashboardView', 20, 1, '📊', 'CLINIC') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono, Sistema) VALUES (22, 'Agenda', 'AgendaView', 20, 2, '📅', 'CLINIC') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono, Sistema) VALUES (23, 'Pacientes', 'PacientesView', 20, 3, '👥', 'CLINIC') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono, Sistema) VALUES (24, 'Consultas', 'ConsultasView', 20, 4, '🩺', 'CLINIC') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono, Sistema) VALUES (25, 'Recetas', 'RecetasView', 20, 5, '💊', 'CLINIC') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono, Sistema) VALUES (26, 'Servicios Médicos', 'ServiciosView', 20, 6, '💼', 'CLINIC') ON CONFLICT DO NOTHING;
SELECT setval('modulos_id_seq', (SELECT MAX(Id) FROM Modulos));

UPDATE Modulos SET Sistema = 'CLINIC' WHERE Clave IN ('MenuClinic', 'DashboardView', 'AgendaView', 'PacientesView', 'ConsultasView', 'RecetasView', 'ServiciosView', 'MedicosView');

-- 3. Crear Rol "Médico" si no existe (Admin = 1, Cajero = 2)
INSERT INTO Roles (Id, Nombre, Descripcion, Activo) VALUES (3, 'Médico', 'Acceso a módulos clínicos', TRUE) ON CONFLICT DO NOTHING;
SELECT setval('roles_id_seq', (SELECT MAX(Id) FROM Roles));

-- 4. Asignar todos los módulos de Clínica al Rol Médico (Id=3) y al Rol Admin (Id=1)
INSERT INTO RolModulos (RolId, ModuloId) 
SELECT 1, Id FROM Modulos WHERE Clave IN ('MenuClinic', 'DashboardView', 'AgendaView', 'PacientesView', 'ConsultasView', 'RecetasView', 'ServiciosView')
ON CONFLICT DO NOTHING;

INSERT INTO RolModulos (RolId, ModuloId) 
SELECT 3, Id FROM Modulos WHERE Clave IN ('MenuClinic', 'DashboardView', 'AgendaView', 'PacientesView', 'ConsultasView', 'RecetasView', 'ServiciosView')
ON CONFLICT DO NOTHING;

ALTER TABLE clinic.Pacientes ADD COLUMN IF NOT EXISTS Clave VARCHAR(50);
ALTER TABLE clinic.Consultas ADD COLUMN IF NOT EXISTS Folio VARCHAR(50);
ALTER TABLE clinic.Citas ADD COLUMN IF NOT EXISTS Folio VARCHAR(50);

UPDATE clinic.Pacientes SET Clave = 'PAC-' || LPAD(Id::TEXT, 5, '0') WHERE Clave IS NULL;
UPDATE clinic.Consultas SET Folio = 'CON-' || TO_CHAR(CreadoEn, 'YYYYMM') || '-' || LPAD(Id::TEXT, 4, '0') WHERE Folio IS NULL;
UPDATE clinic.Citas SET Folio = 'CIT-' || TO_CHAR(FechaHora, 'YYYYMM') || '-' || LPAD(Id::TEXT, 4, '0') WHERE Folio IS NULL;

-- 5. Catálogo de Especialidades Médicas
CREATE TABLE IF NOT EXISTS clinic.EspecialidadesMedicas (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(100) UNIQUE NOT NULL,
    Activo BOOLEAN NOT NULL DEFAULT TRUE,
    CreadoEn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO clinic.EspecialidadesMedicas (Nombre) VALUES 
('Medicina General'), ('Pediatría'), ('Ginecología y Obstetricia'), 
('Cardiología'), ('Dermatología'), ('Traumatología y Ortopedia'), 
('Gastroenterología'), ('Oftalmología'), ('Otorrinolaringología'), 
('Psiquiatría'), ('Neurología'), ('Urología'), ('Odontología') 
ON CONFLICT DO NOTHING;

-- 5.1 Módulo de Médicos
CREATE TABLE IF NOT EXISTS clinic.Medicos (
    Id SERIAL PRIMARY KEY,
    NombreCompleto VARCHAR(200) NOT NULL,
    Especialidad VARCHAR(150),
    Telefono VARCHAR(20),
    Correo VARCHAR(100),
    Activo BOOLEAN NOT NULL DEFAULT TRUE,
    CreadoEn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

DO $body$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'clinic' AND table_name = 'medicos' AND column_name = 'especialidadid') THEN
        ALTER TABLE clinic.Medicos ADD COLUMN EspecialidadId INT REFERENCES clinic.EspecialidadesMedicas(Id);
        
        -- Migrar datos básicos si coinciden
        UPDATE clinic.Medicos m SET EspecialidadId = e.Id 
        FROM clinic.EspecialidadesMedicas e 
        WHERE LOWER(m.Especialidad) = LOWER(e.Nombre);
        
        -- Asignar Medicina General (1) a los que no hicieron match o estaban nulos
        UPDATE clinic.Medicos SET EspecialidadId = 1 WHERE EspecialidadId IS NULL;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'clinic' AND table_name = 'medicos' AND column_name = 'rutaimagen') THEN
        ALTER TABLE clinic.Medicos ADD COLUMN RutaImagen VARCHAR(500);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'clinic' AND table_name = 'medicos' AND column_name = 'actualizadoen') THEN
        ALTER TABLE clinic.Medicos ADD COLUMN ActualizadoEn TIMESTAMP DEFAULT CURRENT_TIMESTAMP;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'clinic' AND table_name = 'medicos' AND column_name = 'fechabaja') THEN
        ALTER TABLE clinic.Medicos ADD COLUMN FechaBaja TIMESTAMP;
    END IF;
END $body$;

-- 6. Agregar MedicoId a Citas y Consultas
DO $body$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'clinic' AND table_name = 'citas' AND column_name = 'medicoid') THEN
        ALTER TABLE clinic.Citas ADD COLUMN MedicoId INT NULL REFERENCES clinic.Medicos(Id);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'clinic' AND table_name = 'consultas' AND column_name = 'medicoid') THEN
        ALTER TABLE clinic.Consultas ADD COLUMN MedicoId INT NULL REFERENCES clinic.Medicos(Id);
    END IF;
END $body$;

-- 7. Módulo UI de Médicos
INSERT INTO Modulos (Nombre, Clave, PadreId, Orden, Icono, Sistema) 
SELECT 'Médicos', 'MedicosView', 20, 7, '👨‍⚕️', 'CLINIC'
WHERE NOT EXISTS (SELECT 1 FROM Modulos WHERE Clave = 'MedicosView');

INSERT INTO RolModulos (RolId, ModuloId) 
SELECT 1, Id FROM Modulos WHERE Clave = 'MedicosView'
AND NOT EXISTS (SELECT 1 FROM RolModulos WHERE RolId = 1 AND ModuloId = (SELECT Id FROM Modulos WHERE Clave = 'MedicosView'));

-- 8. Configuraciones de Clínica Iniciales
INSERT INTO Configuracion (Clave, Valor) VALUES ('HoraAperturaClinica', '09:00:00') ON CONFLICT (Clave) DO NOTHING;
INSERT INTO Configuracion (Clave, Valor) VALUES ('HoraCierreClinica', '18:00:00') ON CONFLICT (Clave) DO NOTHING;
INSERT INTO Configuracion (Clave, Valor) VALUES ('DuracionPromedioCitaMinutos', '30') ON CONFLICT (Clave) DO NOTHING;
INSERT INTO Configuracion (Clave, Valor) VALUES ('AplicaTurnosMedicos', 'false') ON CONFLICT (Clave) DO NOTHING;
INSERT INTO Configuracion (Clave, Valor) VALUES ('MedicoPorDefectoId', '0') ON CONFLICT (Clave) DO NOTHING;
