-- ============================================================
-- SCRIPT DE ACTUALIZACIÃ“N (ALTER TABLES Y UPSERTS)
-- EspecÃ­fico para el esquema clinic y MomosClinic
-- ============================================================

-- 1. Campos de Baja y AuditorÃ­a para Pacientes
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

-- 2. Registrar MÃ³dulos de ClÃ­nica en la tabla centralizada de MomosPOS (public.Modulos)
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (20, 'ClÃ­nica', 'MenuClinic', NULL, 5, 'ðŸ¥') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (21, 'Dashboard ClÃ­nica', 'DashboardView', 20, 1, 'ðŸ“Š') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (22, 'Agenda', 'AgendaView', 20, 2, 'ðŸ“…') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (23, 'Pacientes', 'PacientesView', 20, 3, 'ðŸ‘¥') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (24, 'Consultas', 'ConsultasView', 20, 4, 'ðŸ©º') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (25, 'Recetas', 'RecetasView', 20, 5, 'ðŸ’Š') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (26, 'Servicios MÃ©dicos', 'ServiciosView', 20, 6, 'ðŸ’¼') ON CONFLICT DO NOTHING;
SELECT setval('modulos_id_seq', (SELECT MAX(Id) FROM Modulos));

-- 3. Crear Rol "MÃ©dico" si no existe (Admin = 1, Cajero = 2)
INSERT INTO Roles (Id, Nombre, Descripcion, Activo) VALUES (3, 'MÃ©dico', 'Acceso a mÃ³dulos clÃ­nicos', TRUE) ON CONFLICT DO NOTHING;
SELECT setval('roles_id_seq', (SELECT MAX(Id) FROM Roles));

-- 4. Asignar todos los mÃ³dulos de ClÃ­nica al Rol MÃ©dico (Id=3) y al Rol Admin (Id=1)
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
