[Setup]
; Información básica de la aplicación
AppName=MomosPOS
AppVersion=1.0.5.0
AppPublisher=Tu Empresa
AppPublisherURL=https://github.com/blitzpc17/momospos_desk.git
AppSupportURL=https://github.com/blitzpc17/momospos_desk.git
AppUpdatesURL=https://github.com/blitzpc17/momospos_desk.git
DefaultDirName={autopf}\MomosPOS
DisableProgramGroupPage=yes
; Nombre del archivo instalador generado
OutputBaseFilename=MomosPOS_Setup
; Ícono del instalador
SetupIconFile=Resources\logo.ico
; Compresión (hace el instalador más pequeño)
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Archivo ejecutable principal
Source: "bin\Release\net48\momospos.exe"; DestDir: "{app}"; Flags: ignoreversion
; Incluye todas las dependencias (dlls, carpetas, etc.) de Release
Source: "bin\Release\net48\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Instalador de PostgreSQL
Source: "Prerequisites\postgresql-15-windows-x64.exe"; DestDir: "{tmp}"; Flags: ignoreversion deleteafterinstall
; NOTA: Asegúrate de compilar el proyecto en modo "Release" antes de compilar este script.

[Icons]
; Acceso directo en el menú de inicio
Name: "{autoprograms}\MomosPOS"; Filename: "{app}\momospos.exe"; IconFilename: "{app}\Resources\logo.ico"
; Acceso directo en el escritorio
Name: "{autodesktop}\MomosPOS"; Filename: "{app}\momospos.exe"; Tasks: desktopicon; IconFilename: "{app}\Resources\logo.ico"

[Run]
; Instalar PostgreSQL silenciosamente si no está instalado
Filename: "{tmp}\postgresql-15-windows-x64.exe"; Parameters: "--mode unattended --unattendedmodeui none --superpassword ""123456"" --serverport 5432"; StatusMsg: "Instalando servidor de Base de Datos (puede tardar unos minutos)..."; Flags: waituntilterminated; Check: not IsPostgresInstalled
; Crear la base de datos (ignora error si ya existe)
Filename: "cmd.exe"; Parameters: "/c ""set PGPASSWORD=123456&& ""{commonpf64}\PostgreSQL\15\bin\psql.exe"" -U postgres -d postgres -c ""CREATE DATABASE momospos_db;"""""; StatusMsg: "Configurando Base de Datos..."; Flags: waituntilterminated runhidden
; Ejecutar el sistema después de la instalación
Filename: "{app}\momospos.exe"; Description: "{cm:LaunchProgram,MomosPOS}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Borrar la base de datos y matar conexiones (PostgreSQL 13+)
Filename: "cmd.exe"; Parameters: "/c ""set PGPASSWORD=123456&& ""{commonpf64}\PostgreSQL\15\bin\psql.exe"" -U postgres -d postgres -c ""DROP DATABASE IF EXISTS momospos_db WITH (FORCE);"""""; RunHidden: yes; StatusMsg: "Borrando base de datos..."

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
Type: filesandordirs; Name: "C:\MomosPos_Resources"
Type: filesandordirs; Name: "{localappdata}\MomosPOS"
Type: filesandordirs; Name: "{localappdata}\momospos"
Type: filesandordirs; Name: "{userappdata}\MomosPOS"
Type: filesandordirs; Name: "{userappdata}\momospos"

[Code]
function IsPostgresInstalled: Boolean;
begin
  if FileExists(ExpandConstant('{commonpf64}\PostgreSQL\15\bin\psql.exe')) then
    Result := True
  else
    Result := False;
end;
