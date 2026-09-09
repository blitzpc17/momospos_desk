-- ============================================================
-- SCRIPT DE ACTUALIZACIÓN (ALTER TABLES Y UPSERTS)
-- Útil para no perder datos en una base de datos ya existente
-- Se puede ejecutar múltiples veces sin error (idempotente)
-- ============================================================

-- ============================================================
-- 1. ALTER TABLE: Columnas nuevas en tablas existentes
-- ============================================================

ALTER TABLE Productos 
ADD COLUMN IF NOT EXISTS EsServicio BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS PrecioFijo BOOLEAN NOT NULL DEFAULT TRUE,
ADD COLUMN IF NOT EXISTS Activo BOOLEAN NOT NULL DEFAULT TRUE,
ADD COLUMN IF NOT EXISTS AplicaCaducidad BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS RequiereReceta BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS SustanciaActiva VARCHAR(150),
ADD COLUMN IF NOT EXISTS PrecioMayoreo DECIMAL(18,6) NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS CantidadMayoreo DECIMAL(18,6) NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS Descuento DECIMAL(5,2) NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS ClaveProducto VARCHAR(100),
ADD COLUMN IF NOT EXISTS CodigoProveedor VARCHAR(100),
ADD COLUMN IF NOT EXISTS RutaImagen VARCHAR(500);

ALTER TABLE Roles
ADD COLUMN IF NOT EXISTS Activo BOOLEAN NOT NULL DEFAULT TRUE;

ALTER TABLE CajaSesiones
ADD COLUMN IF NOT EXISTS Observaciones TEXT DEFAULT '';

ALTER TABLE Modulos
ADD COLUMN IF NOT EXISTS Sistema VARCHAR(50) DEFAULT 'POS';

ALTER TABLE Ventas 
ADD COLUMN IF NOT EXISTS MedicoNombre VARCHAR(150),
ADD COLUMN IF NOT EXISTS MedicoCedula VARCHAR(100),
ADD COLUMN IF NOT EXISTS RecetaRetenida BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS RecetaRutaImagen VARCHAR(500),
ADD COLUMN IF NOT EXISTS DescuentoTotal DECIMAL(18,6) NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS DescuentoManual DECIMAL(18,6) NOT NULL DEFAULT 0;

ALTER TABLE VentaDetalles 
ADD COLUMN IF NOT EXISTS DescuentoManual DECIMAL(18,6) NOT NULL DEFAULT 0;

ALTER TABLE Promociones 
ADD COLUMN IF NOT EXISTS AplicaTotalVenta BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS MontoMinimoVenta DECIMAL(18,6);

ALTER TABLE CajaSesiones ADD COLUMN IF NOT EXISTS Observaciones TEXT DEFAULT '';

-- ============================================================
-- 2. CREATE TABLE IF NOT EXISTS: Tablas nuevas
-- ============================================================

CREATE TABLE IF NOT EXISTS ProductoLotes (
    Id SERIAL PRIMARY KEY,
    ProductoId INT NOT NULL REFERENCES Productos(Id) ON DELETE CASCADE,
    NumeroLote VARCHAR(100) NOT NULL,
    FechaCaducidad DATE,
    StockActual DECIMAL(18,6) NOT NULL DEFAULT 0,
    CreadoEn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS VentaDetalleLotes (
    Id SERIAL PRIMARY KEY,
    VentaDetalleId INT NOT NULL REFERENCES VentaDetalles(Id) ON DELETE CASCADE,
    ProductoLoteId INT NOT NULL REFERENCES ProductoLotes(Id),
    Cantidad DECIMAL(18,6) NOT NULL
);

CREATE TABLE IF NOT EXISTS Promociones (
    Id SERIAL PRIMARY KEY,
    ProductoId INT NULL REFERENCES Productos(Id) ON DELETE CASCADE,
    Nombre VARCHAR(150) NOT NULL,
    Tipo VARCHAR(50) NOT NULL,
    CantidadRequerida DECIMAL(18,6),
    CantidadRegalo DECIMAL(18,6),
    DescuentoPorcentaje DECIMAL(5,2),
    AplicaTotalVenta BOOLEAN NOT NULL DEFAULT FALSE,
    MontoMinimoVenta DECIMAL(18,6),
    FechaInicio TIMESTAMP NOT NULL,
    FechaFin TIMESTAMP NOT NULL,
    Activo BOOLEAN NOT NULL DEFAULT TRUE,
    CreadoEn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS public.OrdenesCobro (
    Id SERIAL PRIMARY KEY,
    Referencia VARCHAR(200) NOT NULL,
    ModuloOrigen VARCHAR(100) NOT NULL,
    Estado VARCHAR(50) NOT NULL DEFAULT 'PENDIENTE',
    JsonDetalles TEXT NOT NULL,
    Fecha TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS public.Excepciones (
    Id SERIAL PRIMARY KEY,
    FechaHora TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioId INT NULL REFERENCES Usuarios(Id) ON DELETE SET NULL,
    Modulo VARCHAR(150),
    Mensaje TEXT NOT NULL,
    StackTrace TEXT
);

CREATE TABLE IF NOT EXISTS public.MotivosCancelacionCita (
    Id SERIAL PRIMARY KEY,
    Motivo VARCHAR(200) NOT NULL,
    Activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS public.ProductosSugeridos (
    Id SERIAL PRIMARY KEY,
    NombreProducto VARCHAR(200) NOT NULL,
    CantidadSolicitada INT NOT NULL DEFAULT 1,
    SolicitadoPor INT NULL REFERENCES Usuarios(Id),
    FechaSolicitud TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Evaluado BOOLEAN NOT NULL DEFAULT FALSE
);

-- ============================================================
-- 3. INSERTS DE CATÁLOGOS (ON CONFLICT DO NOTHING = idempotente)
-- ============================================================

INSERT INTO Categorias (Nombre) VALUES ('SERVICIOS') ON CONFLICT DO NOTHING;
INSERT INTO Roles (Id, Nombre, Descripcion, Activo) VALUES (3, 'Médico', 'Acceso a módulos clínicos', TRUE) ON CONFLICT DO NOTHING;

INSERT INTO Configuracion (Clave, Valor) VALUES ('GiroFarmaceutico', 'false') ON CONFLICT DO NOTHING;
INSERT INTO Configuracion (Clave, Valor) VALUES ('GiroPrincipal', 'General / Abarrotes') ON CONFLICT DO NOTHING;
INSERT INTO Configuracion (Clave, Valor) VALUES ('RequerirAutorizacionCancelacion', 'false') ON CONFLICT DO NOTHING;
INSERT INTO Configuracion (Clave, Valor) VALUES ('RutaRecursos', 'C:\MomosPos_Resources') ON CONFLICT DO NOTHING;
INSERT INTO Configuracion (Clave, Valor) VALUES ('Clinic_UsoSecretario', 'true') ON CONFLICT DO NOTHING;

INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (19, 'Promociones', 'PromocionesView', 1, 3, '🎁') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (20, 'Cortes de Caja', 'CortesAdministracionView', 12, 1, '💰') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) SELECT (SELECT COALESCE(MAX(Id), 0) + 1 FROM Modulos), 'Excepciones (Log)', 'ExcepcionesView', 12, 99, '⚠️' WHERE NOT EXISTS (SELECT 1 FROM Modulos WHERE Clave = 'ExcepcionesView');
SELECT setval('modulos_id_seq', (SELECT MAX(Id) FROM Modulos));
SELECT setval('roles_id_seq', (SELECT MAX(Id) FROM Roles));

INSERT INTO MotivosCancelacionCita (Motivo) 
SELECT 'El paciente canceló' WHERE NOT EXISTS (SELECT 1 FROM MotivosCancelacionCita WHERE Motivo = 'El paciente canceló');
INSERT INTO MotivosCancelacionCita (Motivo) 
SELECT 'El paciente no llegó' WHERE NOT EXISTS (SELECT 1 FROM MotivosCancelacionCita WHERE Motivo = 'El paciente no llegó');
INSERT INTO MotivosCancelacionCita (Motivo) 
SELECT 'Falta de tiempo del médico' WHERE NOT EXISTS (SELECT 1 FROM MotivosCancelacionCita WHERE Motivo = 'Falta de tiempo del médico');
INSERT INTO MotivosCancelacionCita (Motivo) 
SELECT 'Reprogramación' WHERE NOT EXISTS (SELECT 1 FROM MotivosCancelacionCita WHERE Motivo = 'Reprogramación');
INSERT INTO MotivosCancelacionCita (Motivo) 
SELECT 'Emergencia médica' WHERE NOT EXISTS (SELECT 1 FROM MotivosCancelacionCita WHERE Motivo = 'Emergencia médica');
INSERT INTO MotivosCancelacionCita (Motivo) 
SELECT 'Otro' WHERE NOT EXISTS (SELECT 1 FROM MotivosCancelacionCita WHERE Motivo = 'Otro');

-- ============================================================
-- 4. ACTUALIZACIONES CONDICIONALES PARA ESQUEMA CLINIC
--    Solo se ejecutan si MomosClinic está instalado (schema 'clinic' existe)
-- ============================================================
DO $$ 
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'clinic') THEN
        ALTER TABLE clinic.Pacientes ADD COLUMN IF NOT EXISTS HistorialClinico TEXT;
        ALTER TABLE clinic.Pacientes ADD COLUMN IF NOT EXISTS Clave VARCHAR(50);
        ALTER TABLE clinic.Recetas ADD COLUMN IF NOT EXISTS Folio VARCHAR(50);
        ALTER TABLE clinic.Consultas ADD COLUMN IF NOT EXISTS Folio VARCHAR(50);
        ALTER TABLE clinic.Citas ADD COLUMN IF NOT EXISTS Folio VARCHAR(50);
        ALTER TABLE clinic.Recetas ALTER COLUMN PacienteId DROP NOT NULL;
        ALTER TABLE clinic.Recetas ALTER COLUMN ConsultaId DROP NOT NULL;
    END IF;
END $$;
