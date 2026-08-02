-- ============================================================
-- SEEDER: Insertar 2000 Productos (Abarrotes Reales) y Servicios
-- ============================================================

DO $$
DECLARE
    i INT;
    catAbarrotesId INT;
    catServiciosId INT;
    unidadPzaId INT;
    codigo VARCHAR;
    nombreProducto VARCHAR;
    descProducto TEXT;
    isServicio BOOLEAN;
    precioC DECIMAL(18,2);
    precioV DECIMAL(18,2);
    
    -- Arrays para generar abarrotes aleatorios
    tipos TEXT[] := ARRAY['Refresco', 'Galletas', 'Papas', 'Jabón', 'Aceite', 'Frijoles', 'Arroz', 'Leche', 'Yogur', 'Cereal', 'Pan', 'Atún', 'Mayonesa', 'Salsa', 'Jugo', 'Agua', 'Cerveza', 'Detergente', 'Papel Higiénico', 'Servilletas', 'Limpiador', 'Cloro', 'Shampoo', 'Pasta Dental'];
    marcas TEXT[] := ARRAY['Coca-Cola', 'Pepsi', 'Bimbo', 'Marinela', 'Sabritas', 'Barcel', 'La Costeña', 'Herdez', 'Del Fuerte', 'Knorr', 'Lala', 'Alpura', 'Nutrioli', '123', 'Zote', 'Ariel', 'Fabuloso', 'Pinol', 'Cloralex', 'Colgate', 'Crest', 'Palmolive', 'Dove', 'Caprice'];
    variantes TEXT[] := ARRAY['600ml', '1L', '1.5L', '2L', '3L', '500g', '1Kg', '900g', '250g', 'Regular', 'Light', 'Zero', 'Fresa', 'Chocolate', 'Vainilla', 'Limón', 'Original', 'Extra Picante', 'Antibacterial', 'Blancos', 'Color', 'Clásico', 'Premium', 'Económico'];
    
    -- Arrays para generar servicios
    tiposSrv TEXT[] := ARRAY['Recarga', 'Pago de Recibo', 'Copias', 'Impresión', 'Enmicado', 'Engargolado', 'Reparación', 'Mantenimiento'];
    proveedoresSrv TEXT[] := ARRAY['Telcel', 'Movistar', 'AT&T', 'CFE', 'Telmex', 'Megacable', 'Genérico', 'B/N', 'Color'];
    variantesSrv TEXT[] := ARRAY['$20', '$50', '$100', '$200', 'Básico', 'Completo', 'Urgente', 'Normal'];
    
    idxTipo INT;
    idxMarca INT;
    idxVar INT;
BEGIN
    -- Obtener o crear Categoría 'Abarrotes'
    SELECT Id INTO catAbarrotesId FROM Categorias WHERE Nombre = 'Abarrotes' LIMIT 1;
    IF catAbarrotesId IS NULL THEN
        INSERT INTO Categorias (Nombre) VALUES ('Abarrotes') RETURNING Id INTO catAbarrotesId;
    END IF;

    -- Obtener o crear Categoría 'SERVICIOS'
    SELECT Id INTO catServiciosId FROM Categorias WHERE Nombre = 'SERVICIOS' LIMIT 1;
    IF catServiciosId IS NULL THEN
        INSERT INTO Categorias (Nombre) VALUES ('SERVICIOS') RETURNING Id INTO catServiciosId;
    END IF;

    -- Obtener o crear Unidad 'PZA'
    SELECT Id INTO unidadPzaId FROM UnidadesMedida WHERE Abreviatura = 'PZA' LIMIT 1;
    IF unidadPzaId IS NULL THEN
        INSERT INTO UnidadesMedida (Nombre, Abreviatura, PermiteFraccion) VALUES ('Pieza', 'PZA', FALSE) RETURNING Id INTO unidadPzaId;
    END IF;

    -- Loop para insertar 2,000 registros
    FOR i IN 1..2000 LOOP
        -- 5% serán Servicios
        isServicio := (i % 20 = 0);
        
        IF isServicio THEN
            codigo := 'SRV' || LPAD(i::TEXT, 5, '0');
            idxTipo := floor(random() * array_length(tiposSrv, 1) + 1);
            idxMarca := floor(random() * array_length(proveedoresSrv, 1) + 1);
            idxVar := floor(random() * array_length(variantesSrv, 1) + 1);
            
            nombreProducto := tiposSrv[idxTipo] || ' ' || proveedoresSrv[idxMarca] || ' ' || variantesSrv[idxVar];
            descProducto := 'Servicio de ' || nombreProducto;
            
            INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, CategoriaId, UnidadMedidaId, PrecioCompra, PrecioVenta, StockActual, StockMinimo, EsServicio, PrecioFijo)
            VALUES (codigo, nombreProducto, descProducto, catServiciosId, unidadPzaId, 0, 0, 0, 0, TRUE, TRUE)
            ON CONFLICT (CodigoBarras) DO NOTHING;
        ELSE
            codigo := '750' || LPAD(i::TEXT, 10, '0'); -- Simular un EAN-13 mexicano real (750...)
            idxTipo := floor(random() * array_length(tipos, 1) + 1);
            idxMarca := floor(random() * array_length(marcas, 1) + 1);
            idxVar := floor(random() * array_length(variantes, 1) + 1);
            
            nombreProducto := tipos[idxTipo] || ' ' || marcas[idxMarca] || ' ' || variantes[idxVar];
            descProducto := 'Producto de abarrotes: ' || nombreProducto;
            
            -- Generar un precio de compra aleatorio entre 10 y 100
            precioC := round((random() * 90 + 10)::numeric, 2);
            -- Precio de venta con un 30% a 50% de margen
            precioV := round((precioC * (1.3 + random() * 0.2))::numeric, 2);
            
            INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, CategoriaId, UnidadMedidaId, PrecioCompra, PrecioVenta, StockActual, StockMinimo, EsServicio, PrecioFijo)
            VALUES (codigo, nombreProducto, descProducto, catAbarrotesId, unidadPzaId, precioC, precioV, floor(random() * 50 + 10), 5, FALSE, TRUE)
            ON CONFLICT (CodigoBarras) DO NOTHING;
        END IF;
    END LOOP;
END $$;
