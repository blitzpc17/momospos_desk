-- ============================================================
-- SCRIPT DE ACTUALIZACIÓN (ALTER TABLES Y UPSERTS)
-- Específico para el esquema clinic y MomosClinic
-- ============================================================

-- 1. Campos de Baja y Auditoría para Pacientes
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
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (20, 'Clínica', 'MenuClinic', NULL, 5, '🏥') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (21, 'Dashboard Clínica', 'DashboardView', 20, 1, '📊') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (22, 'Agenda', 'AgendaView', 20, 2, '📅') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (23, 'Pacientes', 'PacientesView', 20, 3, '👥') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (24, 'Consultas', 'ConsultasView', 20, 4, '🩺') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (25, 'Recetas', 'RecetasView', 20, 5, '💊') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (26, 'Servicios Médicos', 'ServiciosView', 20, 6, '💼') ON CONFLICT DO NOTHING;
SELECT setval('modulos_id_seq', (SELECT MAX(Id) FROM Modulos));

-- 3. Crear Rol "Médico" si no existe (Admin = 1, Cajero = 2)
INSERT INTO Roles (Id, Nombre, Descripcion, Activo) VALUES (3, 'Médico', 'Acceso a módulos clínicos', TRUE) ON CONFLICT DO NOTHING;
SELECT setval('roles_id_seq', (SELECT MAX(Id) FROM Roles));

-- 4. Asignar todos los módulos de Clínica al Rol Médico (Id=3) y al Rol Admin (Id=1)
DO $$ 
DECLARE 
    mId INT;
BEGIN
    FOR mId IN 20..26 LOOP
        INSERT INTO RolModulos (RolId, ModuloId) VALUES (1, mId) ON CONFLICT DO NOTHING; -- Admin
        INSERT INTO RolModulos (RolId, ModuloId) VALUES (3, mId) ON CONFLICT DO NOTHING; -- Médico
    END LOOP;
END $$;

