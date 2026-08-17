-- ============================================================
-- ESQUEMA REDUCIDO POS (Estilo Eleventa)
-- ============================================================

-- 1. USUARIOS
CREATE TABLE IF NOT EXISTS Usuarios (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(150) NOT NULL,
    Usuario VARCHAR(80) UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,
    EsAdmin BOOLEAN NOT NULL DEFAULT FALSE,
    Estado VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
    CreadoEn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 2. CAJAS Y TURNOS
CREATE TABLE IF NOT EXISTS Cajas (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(120) NOT NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'ACTIVO'
);

CREATE TABLE IF NOT EXISTS CajaSesiones (
    Id SERIAL PRIMARY KEY,
    CajaId INT NOT NULL REFERENCES Cajas(Id),
    UsuarioAperturaId INT NOT NULL REFERENCES Usuarios(Id),
    UsuarioCierreId INT REFERENCES Usuarios(Id),
    FechaApertura TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaCierre TIMESTAMP,
    FondoInicial DECIMAL(18,2) NOT NULL DEFAULT 0,
    EfectivoEsperado DECIMAL(18,2) NOT NULL DEFAULT 0,
    EfectivoContado DECIMAL(18,2),
    Diferencia DECIMAL(18,2),
    Estado VARCHAR(20) NOT NULL DEFAULT 'ABIERTA' -- ABIERTA, CERRADA
);

CREATE TABLE IF NOT EXISTS CajaMovimientos (
    Id SERIAL PRIMARY KEY,
    CajaSesionId INT NOT NULL REFERENCES CajaSesiones(Id),
    Tipo VARCHAR(30) NOT NULL, -- INGRESO, RETIRO, VENTA
    Fecha TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Importe DECIMAL(18,2) NOT NULL,
    Concepto VARCHAR(180) NOT NULL,
    UsuarioId INT NOT NULL REFERENCES Usuarios(Id)
);

-- 3. PRODUCTOS
CREATE TABLE IF NOT EXISTS Categorias (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(120) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS UnidadesMedida (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(80) NOT NULL,
    Abreviatura VARCHAR(20) NOT NULL,
    PermiteFraccion BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS Productos (
    Id SERIAL PRIMARY KEY,
    CodigoBarras VARCHAR(80) UNIQUE,
    Nombre VARCHAR(180) NOT NULL,
    Descripcion TEXT,
    CategoriaId INT REFERENCES Categorias(Id),
    UnidadMedidaId INT REFERENCES UnidadesMedida(Id),
    PrecioCompra DECIMAL(18,2) NOT NULL DEFAULT 0,
    PrecioVenta DECIMAL(18,2) NOT NULL DEFAULT 0,
    StockActual DECIMAL(18,6) NOT NULL DEFAULT 0,
    StockMinimo DECIMAL(18,6) NOT NULL DEFAULT 0,
    EsServicio BOOLEAN NOT NULL DEFAULT FALSE,
    PrecioFijo BOOLEAN NOT NULL DEFAULT TRUE,
    AplicaCaducidad BOOLEAN NOT NULL DEFAULT FALSE,
    RequiereReceta BOOLEAN NOT NULL DEFAULT FALSE,
    SustanciaActiva VARCHAR(150),
    CreadoEn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ProductoLotes (
    Id SERIAL PRIMARY KEY,
    ProductoId INT NOT NULL REFERENCES Productos(Id) ON DELETE CASCADE,
    NumeroLote VARCHAR(100) NOT NULL,
    FechaCaducidad DATE,
    StockActual DECIMAL(18,6) NOT NULL DEFAULT 0,
    CreadoEn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS InventarioMovimientos (
    Id SERIAL PRIMARY KEY,
    ProductoId INT NOT NULL REFERENCES Productos(Id),
    Tipo VARCHAR(30) NOT NULL, -- ENTRADA, SALIDA, AJUSTE, VENTA
    Cantidad DECIMAL(18,6) NOT NULL,
    Fecha TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioId INT NOT NULL REFERENCES Usuarios(Id),
    Observaciones TEXT
);

-- 4. CLIENTES
CREATE TABLE IF NOT EXISTS Clientes (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(180) NOT NULL,
    Telefono VARCHAR(30),
    Correo VARCHAR(150),
    LimiteCredito DECIMAL(18,2) NOT NULL DEFAULT 0,
    Saldo DECIMAL(18,2) NOT NULL DEFAULT 0,
    Estado VARCHAR(20) NOT NULL DEFAULT 'ACTIVO'
);

-- 5. VENTAS
CREATE TABLE IF NOT EXISTS Ventas (
    Id SERIAL PRIMARY KEY,
    Folio VARCHAR(30) UNIQUE NOT NULL,
    CajaSesionId INT NOT NULL REFERENCES CajaSesiones(Id),
    ClienteId INT REFERENCES Clientes(Id),
    Fecha TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Total DECIMAL(18,2) NOT NULL,
    Pagado DECIMAL(18,2) NOT NULL DEFAULT 0,
    Cambio DECIMAL(18,2) NOT NULL DEFAULT 0,
    Estado VARCHAR(20) NOT NULL DEFAULT 'CONFIRMADO',
    UsuarioId INT NOT NULL REFERENCES Usuarios(Id),
    MedicoNombre VARCHAR(150) NULL,
    MedicoCedula VARCHAR(100) NULL
);

CREATE TABLE IF NOT EXISTS VentaDetalles (
    Id SERIAL PRIMARY KEY,
    VentaId INT NOT NULL REFERENCES Ventas(Id) ON DELETE CASCADE,
    ProductoId INT NOT NULL REFERENCES Productos(Id),
    Descripcion VARCHAR(220) NOT NULL,
    Cantidad DECIMAL(18,6) NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL
);

CREATE TABLE IF NOT EXISTS VentaDetalleLotes (
    Id SERIAL PRIMARY KEY,
    VentaDetalleId INT NOT NULL REFERENCES VentaDetalles(Id) ON DELETE CASCADE,
    ProductoLoteId INT NOT NULL REFERENCES ProductoLotes(Id),
    Cantidad DECIMAL(18,6) NOT NULL
);

CREATE TABLE IF NOT EXISTS VentaPagos (
    Id SERIAL PRIMARY KEY,
    VentaId INT NOT NULL REFERENCES Ventas(Id) ON DELETE CASCADE,
    MetodoPago VARCHAR(30) NOT NULL, -- EFECTIVO, TARJETA, CREDITO
    Importe DECIMAL(18,2) NOT NULL,
    Fecha TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS VentasAbortadas (
    Id SERIAL PRIMARY KEY,
    Fecha TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioId INT NOT NULL REFERENCES Usuarios(Id),
    TotalEsperado DECIMAL(18,2) NOT NULL,
    Motivo VARCHAR(250) NOT NULL
);

-- 6. SEGURIDAD Y ROLES
CREATE TABLE IF NOT EXISTS Roles (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(80) NOT NULL UNIQUE,
    Descripcion VARCHAR(200)
);

CREATE TABLE IF NOT EXISTS Modulos (
    Id SERIAL PRIMARY KEY,
    Nombre VARCHAR(80) NOT NULL,
    Clave VARCHAR(80) UNIQUE NOT NULL,
    PadreId INT REFERENCES Modulos(Id) ON DELETE CASCADE,
    Orden INT NOT NULL DEFAULT 0,
    Icono VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS RolModulos (
    RolId INT NOT NULL REFERENCES Roles(Id) ON DELETE CASCADE,
    ModuloId INT NOT NULL REFERENCES Modulos(Id) ON DELETE CASCADE,
    PRIMARY KEY (RolId, ModuloId)
);

CREATE TABLE IF NOT EXISTS UsuarioRoles (
    UsuarioId INT NOT NULL REFERENCES Usuarios(Id) ON DELETE CASCADE,
    RolId INT NOT NULL REFERENCES Roles(Id) ON DELETE CASCADE,
    PRIMARY KEY (UsuarioId, RolId)
);

CREATE TABLE IF NOT EXISTS UsuarioModulos (
    UsuarioId INT NOT NULL REFERENCES Usuarios(Id) ON DELETE CASCADE,
    ModuloId INT NOT NULL REFERENCES Modulos(Id) ON DELETE CASCADE,
    Concedido BOOLEAN NOT NULL DEFAULT true, 
    PRIMARY KEY (UsuarioId, ModuloId)
);

-- 7. AUTORIZACIONES DE CANCELACIÓN
CREATE TABLE IF NOT EXISTS VentaCancelaciones (
    Id SERIAL PRIMARY KEY,
    VentaId INT NOT NULL REFERENCES Ventas(Id),
    Motivo TEXT NOT NULL,
    UsuarioSolicitaId INT NOT NULL REFERENCES Usuarios(Id),
    UsuarioAutorizaId INT REFERENCES Usuarios(Id),
    FechaSolicitud TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaAutorizacion TIMESTAMP,
    Estado VARCHAR(20) NOT NULL DEFAULT 'PENDIENTE' -- PENDIENTE, APROBADA, RECHAZADA
);

-- 8. CONFIGURACION
CREATE TABLE IF NOT EXISTS Configuracion (
    Clave VARCHAR(50) PRIMARY KEY, 
    Valor TEXT
);

-- ============================================================
-- ÍNDICES (Optimización de Búsquedas)
-- ============================================================
CREATE INDEX IF NOT EXISTS IDX_Productos_Nombre ON Productos(Nombre);
CREATE INDEX IF NOT EXISTS IDX_Productos_CodigoBarras ON Productos(CodigoBarras);
CREATE INDEX IF NOT EXISTS IDX_Clientes_Nombre ON Clientes(Nombre);
CREATE INDEX IF NOT EXISTS IDX_Ventas_Fecha ON Ventas(Fecha);
CREATE INDEX IF NOT EXISTS IDX_VentaCancelaciones_Estado ON VentaCancelaciones(Estado);

-- ============================================================
-- SEEDERS (Datos Iniciales por Defecto)
-- ============================================================

-- Roles Básicos
INSERT INTO Roles (Nombre, Descripcion) VALUES 
('Administrador', 'Acceso total al sistema'),
('Cajero', 'Acceso a ventas y cortes de caja')
ON CONFLICT DO NOTHING;

INSERT INTO Usuarios (Nombre, Usuario, PasswordHash, EsAdmin) 
VALUES 
('Administrador', 'admin', 'admin', true),
('Gerente General', 'gerente', '12345', true),
('Cajero 1', 'cajero1', '1234', false)
ON CONFLICT DO NOTHING;

-- Asignar rol a Cajero 1 (Asumiendo Id 3 y RolId 2)
INSERT INTO UsuarioRoles (UsuarioId, RolId) VALUES (3, 2) ON CONFLICT DO NOTHING;

-- Módulos Jerárquicos
-- 1. Padre: Ventas (Id=1)
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES 
(0, 'Dashboard', 'DashboardView', NULL, 0, '📈'),
(1, 'Ventas', 'MenuVentas', NULL, 1, '🛒'),
(2, 'Punto de Venta', 'VentasView', 1, 1, '💲')
ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (3, 'Cuentas x Cobrar', 'CuentasCobrarView', 1, 2, '💳') ON CONFLICT DO NOTHING;

-- 2. Padre: Inventario (Id=4)
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (4, 'Inventario', 'MenuInventario', NULL, 2, '📦') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (5, 'Productos', 'ProductosView', 4, 1, '📝') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (6, 'Categorías', 'CategoriasView', 4, 2, '🏷️') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (7, 'Entradas (Compras)', 'ComprasView', 4, 3, '📥') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (8, 'Ajuste/Mermas', 'MermasView', 4, 4, '📉') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (18, 'Estado Existencias', 'ReporteExistenciasView', 4, 5, '📊') ON CONFLICT DO NOTHING;

-- 3. Padre: Contactos (Id=9)
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (9, 'Personas', 'MenuPersonas', NULL, 3, '👥') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (10, 'Clientes', 'ClientesView', 9, 1, '🙍‍♂️') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (11, 'Usuarios', 'UsuariosView', 9, 2, '👤') ON CONFLICT DO NOTHING;

-- 4. Padre: Administración (Id=12)
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (12, 'Administración', 'MenuAdmin', NULL, 4, '⚙️') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (13, 'Reportes', 'ReportesView', 12, 1, '📊') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (14, 'Autorizaciones', 'AutorizacionesView', 12, 2, '🛡️') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (15, 'Configuración', 'ConfiguracionView', 12, 3, '🔧') ON CONFLICT DO NOTHING;
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (16, 'Roles y Permisos', 'SeguridadView', 12, 4, '🔑') ON CONFLICT DO NOTHING;

-- Permisos Extra / Especiales (se consideran módulos lógicos)
INSERT INTO Modulos (Id, Nombre, Clave, PadreId, Orden, Icono) VALUES (17, 'Cancelar Ventas', 'CANCELAR_VENTAS', NULL, 99, '❌') ON CONFLICT DO NOTHING;

-- Rol Cajero (2) por defecto ve Punto de Venta (1,2)
INSERT INTO RolModulos (RolId, ModuloId) VALUES (2, 1) ON CONFLICT DO NOTHING;
INSERT INTO RolModulos (RolId, ModuloId) VALUES (2, 2) ON CONFLICT DO NOTHING;

-- Resetear secuencias
SELECT setval('modulos_id_seq', (SELECT MAX(Id) FROM Modulos));
SELECT setval('roles_id_seq', (SELECT MAX(Id) FROM Roles));

INSERT INTO Cajas (Nombre) VALUES ('Caja Principal') ON CONFLICT DO NOTHING;

-- Seed de Ejemplo para Lotes y Farmacia
INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, CategoriaId, UnidadMedidaId, PrecioCompra, PrecioVenta, StockActual, StockMinimo, AplicaCaducidad, RequiereReceta, SustanciaActiva) 
VALUES ('7501234567890', 'Paracetamol 500mg 10 Tabletas', 'Medicamento genérico dolor y fiebre', 1, 1, 10.00, 25.00, 80, 20, true, false, 'Paracetamol') 
ON CONFLICT DO NOTHING;

-- Asumimos que se acaba de crear y tiene el Id correspondiente. Si quieres asegurar el insert de lotes:
INSERT INTO ProductoLotes (ProductoId, NumeroLote, FechaCaducidad, StockActual)
SELECT p.Id, 'LOTE-A123', '2026-12-31', 50 FROM Productos p WHERE p.CodigoBarras = '7501234567890'
ON CONFLICT DO NOTHING;

INSERT INTO ProductoLotes (ProductoId, NumeroLote, FechaCaducidad, StockActual)
SELECT p.Id, 'LOTE-B456', '2027-06-30', 30 FROM Productos p WHERE p.CodigoBarras = '7501234567890'
ON CONFLICT DO NOTHING;

INSERT INTO UnidadesMedida (Nombre, Abreviatura, PermiteFraccion) VALUES 
('Pieza', 'PZA', false),
('Kilogramo', 'KG', true),
('Litro', 'L', true)
ON CONFLICT DO NOTHING;

INSERT INTO Categorias (Nombre) VALUES 
('General'),
('SERVICIOS')
ON CONFLICT DO NOTHING;

INSERT INTO Configuracion (Clave, Valor) VALUES 
('NombreNegocio', 'Mi Tienda POS'), 
('RFC', 'XAXX010101000'), 
('Direccion', 'Calle Falsa 123'), 
('MensajeTicket', '¡Gracias por su preferencia!'),
('GiroFarmaceutico', 'false') 
ON CONFLICT DO NOTHING;
