-- ============================================================
-- SCRIPT DE ACTUALIZACIÓN (ALTER TABLES Y UPSERTS)
-- Útil para no perder datos en una base de datos ya existente
-- ============================================================

-- 1. Añadir las nuevas columnas a la tabla Productos
-- Usamos "IF NOT EXISTS" para que el script no falle si se ejecuta dos veces
ALTER TABLE Productos 
ADD COLUMN IF NOT EXISTS EsServicio BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS PrecioFijo BOOLEAN NOT NULL DEFAULT TRUE,
ADD COLUMN IF NOT EXISTS Activo BOOLEAN NOT NULL DEFAULT TRUE;

-- 2. Asegurar que exista la categoría 'SERVICIOS'
-- ON CONFLICT evita errores y duplicados si la categoría ya existe
INSERT INTO Categorias (Nombre) 
VALUES ('SERVICIOS')
ON CONFLICT (Nombre) DO NOTHING;

-- Tabla para integración de ventas pausadas y órdenes clínicas (MomosClinic)
CREATE TABLE IF NOT EXISTS public.OrdenesCobro (
    Id SERIAL PRIMARY KEY,
    Referencia VARCHAR(200) NOT NULL,
    ModuloOrigen VARCHAR(100) NOT NULL, -- Ej. 'MomosPOS' o 'MomosClinic'
    Estado VARCHAR(50) NOT NULL DEFAULT 'PENDIENTE', -- PENDIENTE, COBRADA, CANCELADA
    JsonDetalles TEXT NOT NULL, -- JSON con los productos/medicamentos
    Fecha TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 3. Añadir campos para Giro Farmacéutico y Caducidades
ALTER TABLE Productos 
ADD COLUMN IF NOT EXISTS AplicaCaducidad BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS RequiereReceta BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS SustanciaActiva VARCHAR(150);

-- 4. Crear tablas de Lotes
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

-- 5. Configuracion Giro Farmaceutico y Permisos
INSERT INTO Configuracion (Clave, Valor) VALUES ('GiroFarmaceutico', 'false') ON CONFLICT DO NOTHING;
INSERT INTO Configuracion (Clave, Valor) VALUES ('GiroPrincipal', 'General / Abarrotes') ON CONFLICT DO NOTHING;
INSERT INTO Configuracion (Clave, Valor) VALUES ('RequerirAutorizacionCancelacion', 'false') ON CONFLICT DO NOTHING;

-- 6. Receta Medica
ALTER TABLE Ventas 
ADD COLUMN IF NOT EXISTS MedicoNombre VARCHAR(150),
ADD COLUMN IF NOT EXISTS MedicoCedula VARCHAR(100),
ADD COLUMN IF NOT EXISTS RecetaRetenida BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS RecetaRutaImagen VARCHAR(500);

-- 7. Promociones Dinámicas
CREATE TABLE IF NOT EXISTS Promociones (
    Id SERIAL PRIMARY KEY,
    ProductoId INT NULL REFERENCES Productos(Id) ON DELETE CASCADE,
    Nombre VARCHAR(150) NOT NULL,
    Tipo VARCHAR(50) NOT NULL, -- 'NxM' (ej. 3x2), 'Porcentaje', 'TotalVenta'
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

ALTER TABLE Promociones 
ADD COLUMN IF NOT EXISTS AplicaTotalVenta BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS MontoMinimoVenta DECIMAL(18,6);

INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (19, 'Promociones', 'PromocionesView', 1, 3, '🎁') ON CONFLICT DO NOTHING;

-- 8. Mayoreo, Códigos, Imágenes y Cortesías
ALTER TABLE Productos 
ADD COLUMN IF NOT EXISTS PrecioMayoreo DECIMAL(18,6) NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS CantidadMayoreo DECIMAL(18,6) NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS ClaveProducto VARCHAR(100),
ADD COLUMN IF NOT EXISTS CodigoProveedor VARCHAR(100),
ADD COLUMN IF NOT EXISTS RutaImagen VARCHAR(500);

ALTER TABLE Ventas 
ADD COLUMN IF NOT EXISTS DescuentoTotal DECIMAL(18,6) NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS DescuentoManual DECIMAL(18,6) NOT NULL DEFAULT 0;

ALTER TABLE VentaDetalles 
ADD COLUMN IF NOT EXISTS DescuentoManual DECIMAL(18,6) NOT NULL DEFAULT 0;

INSERT INTO Configuracion (Clave, Valor) VALUES ('RutaRecursos', 'C:\MomosPos_Resources') ON CONFLICT DO NOTHING;
