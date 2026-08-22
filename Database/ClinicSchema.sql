-- Esquema para la Clínica (independiente del esquema public de MomosPOS)
CREATE SCHEMA IF NOT EXISTS clinic;

-- NOTA: Los Usuarios, Roles y Permisos ahora se administran de forma centralizada en el esquema public (MomosPOS).
-- Ya no se utiliza clinic.Usuarios.


-- Pacientes
CREATE TABLE IF NOT EXISTS clinic.Pacientes (
    Id SERIAL PRIMARY KEY,
    NombreCompleto VARCHAR(200) NOT NULL,
    FechaNacimiento DATE,
    Genero VARCHAR(20),
    Telefono VARCHAR(20),
    Email VARCHAR(100),
    Direccion TEXT,
    Alergias TEXT,
    AntecedentesFamiliares TEXT,
    AntecedentesPatologicos TEXT,
    TipoSangre VARCHAR(10),
    Activo BOOLEAN NOT NULL DEFAULT TRUE,
    MotivoBaja VARCHAR(500),
    BajaPor VARCHAR(100),
    CreadoPor VARCHAR(100),
    ModificadoPor VARCHAR(100),
    CreadoEn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Agenda / Citas
CREATE TABLE IF NOT EXISTS clinic.Citas (
    Id SERIAL PRIMARY KEY,
    PacienteId INT REFERENCES clinic.Pacientes(Id) ON DELETE CASCADE,
    FechaHora TIMESTAMP NOT NULL,
    Motivo TEXT,
    Estado VARCHAR(50) NOT NULL DEFAULT 'Programada', -- Programada, Confirmada, Completada, Cancelada
    Notas TEXT,
    CreadoEn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Consultas / Expediente
CREATE TABLE IF NOT EXISTS clinic.Consultas (
    Id SERIAL PRIMARY KEY,
    CitaId INT NULL REFERENCES clinic.Citas(Id) ON DELETE SET NULL,
    PacienteId INT NOT NULL REFERENCES clinic.Pacientes(Id) ON DELETE CASCADE,
    -- Signos Vitales
    Peso DECIMAL(5,2), -- kg
    Talla DECIMAL(5,2), -- m
    Temperatura DECIMAL(5,2), -- C
    PresionArterial VARCHAR(20), -- ej. 120/80
    FrecuenciaCardiaca INT, -- lpm
    FrecuenciaRespiratoria INT, -- rpm
    SaturacionOxigeno INT, -- %
    IMC DECIMAL(5,2),
    -- SOAP
    MotivoConsulta TEXT, -- S (Subjetivo)
    ExploracionFisica TEXT, -- O (Objetivo)
    Analisis TEXT, -- A (Análisis)
    Diagnostico TEXT, -- CIE-10 u texto libre
    PlanTratamiento TEXT, -- P (Plan)
    -- Finanzas
    CobroGenerado BOOLEAN NOT NULL DEFAULT FALSE,
    FolioCobro VARCHAR(50) NULL, -- Referencia cruzada con public.Ventas si se paga
    CreadoEn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Recetas
CREATE TABLE IF NOT EXISTS clinic.Recetas (
    Id SERIAL PRIMARY KEY,
    ConsultaId INT NOT NULL REFERENCES clinic.Consultas(Id) ON DELETE CASCADE,
    PacienteId INT NOT NULL REFERENCES clinic.Pacientes(Id) ON DELETE CASCADE,
    IndicacionesGenerales TEXT,
    FechaEmision TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Receta Detalles (Medicamentos)
CREATE TABLE IF NOT EXISTS clinic.RecetaDetalles (
    Id SERIAL PRIMARY KEY,
    RecetaId INT NOT NULL REFERENCES clinic.Recetas(Id) ON DELETE CASCADE,
    ProductoId INT NULL, -- Puede apuntar a public.Productos(Id) si el medicamento existe en la farmacia
    NombreMedicamento VARCHAR(200) NOT NULL, -- En caso de no usar ProductoId o si es libre
    Dosis VARCHAR(100),
    Frecuencia VARCHAR(100),
    Duracion VARCHAR(100),
    Cantidad INT NOT NULL DEFAULT 1,
    Instrucciones TEXT
);
