-- ============================================================
-- MIGRACIÓN: Soporte API Móvil MomosPOS
-- Ejecutar sobre la base de datos PostgreSQL existente.
-- ============================================================

-- 1. DISPOSITIVOS MÓVILES AUTORIZADOS
CREATE TABLE IF NOT EXISTS public.Dispositivos (
    Id               SERIAL PRIMARY KEY,
    Nombre           VARCHAR(150) NOT NULL,
    Token            VARCHAR(256) NOT NULL UNIQUE,
    Activo           BOOLEAN      NOT NULL DEFAULT TRUE,
    PrefixFolio      VARCHAR(20)  NOT NULL DEFAULT 'MOV',
    CreadoEn         TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UltimaConexion   TIMESTAMP
);

-- 2. CONSECUTIVOS DE FOLIOS POR DISPOSITIVO Y TIPO
--    Preparado para futuros tipos: COTIZACION, NOTA, etc.
CREATE TABLE IF NOT EXISTS public.DispositivoFolios (
    Id              SERIAL PRIMARY KEY,
    DispositivoId   INT          NOT NULL REFERENCES public.Dispositivos(Id) ON DELETE CASCADE,
    TipoDocumento   VARCHAR(50)  NOT NULL DEFAULT 'VENTA',
    Consecutivo     BIGINT       NOT NULL DEFAULT 0,
    Activo          BOOLEAN      NOT NULL DEFAULT TRUE,
    UNIQUE(DispositivoId, TipoDocumento)
);

-- 3. EXTENDER TABLA VENTAS PARA IDENTIFICAR ORIGEN
ALTER TABLE public.Ventas
    ADD COLUMN IF NOT EXISTS DispositivoId INT NULL REFERENCES public.Dispositivos(Id),
    ADD COLUMN IF NOT EXISTS Origen        VARCHAR(20) NOT NULL DEFAULT 'POS';

-- 4. ÍNDICES
CREATE INDEX IF NOT EXISTS IDX_Dispositivos_Token   ON public.Dispositivos(Token);
CREATE INDEX IF NOT EXISTS IDX_Ventas_Origen        ON public.Ventas(Origen);
CREATE INDEX IF NOT EXISTS IDX_Ventas_DispositivoId ON public.Ventas(DispositivoId);

-- 5. CONFIGURACIONES DEL API MÓVIL
INSERT INTO public.Configuracion (Clave, Valor) VALUES
    ('APIMovilActiva',        'false'),
    ('APIMovilVentasActivas', 'true'),
    ('APIMovilMaxProductos',  '5000'),
    ('APIMovilJwtSecretKey',  ''),
    ('APIMovilJwtExpHoras',   '24')
ON CONFLICT (Clave) DO NOTHING;
