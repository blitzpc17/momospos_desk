-- ============================================================
-- SCRIPT DE ACTUALIZACIÓN (ALTER TABLES Y UPSERTS)
-- Útil para no perder datos en una base de datos ya existente
-- ============================================================

-- 1. Añadir las nuevas columnas a la tabla Productos
-- Usamos "IF NOT EXISTS" para que el script no falle si se ejecuta dos veces
ALTER TABLE Productos 
ADD COLUMN IF NOT EXISTS EsServicio BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS PrecioFijo BOOLEAN NOT NULL DEFAULT TRUE;

-- 2. Asegurar que exista la categoría 'SERVICIOS'
-- ON CONFLICT evita errores y duplicados si la categoría ya existe
INSERT INTO Categorias (Nombre) 
VALUES ('SERVICIOS')
ON CONFLICT (Nombre) DO NOTHING;
