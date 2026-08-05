[Setup]
; Información básica de la aplicación
AppName=MomosPOS
AppVersion=1.0.0
AppPublisher=Tu Empresa
AppPublisherURL=https://github.com/blitzpc17/momospos_desk.git
AppSupportURL=https://github.com/blitzpc17/momospos_desk.git
AppUpdatesURL=https://github.com/blitzpc17/momospos_desk.git
DefaultDirName={autopf}\MomosPOS
DisableProgramGroupPage=yes
; Nombre del archivo instalador generado
OutputBaseFilename=MomosPOS_Setup
; Ícono del instalador
SetupIconFile=bin\Debug\net48\Resources\logo.ico
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
; NOTA: Asegúrate de compilar el proyecto en modo "Release" antes de compilar este script.

[Icons]
; Acceso directo en el menú de inicio
Name: "{autoprograms}\MomosPOS"; Filename: "{app}\momospos.exe"; IconFilename: "{app}\Resources\logo.ico"
; Acceso directo en el escritorio
Name: "{autodesktop}\MomosPOS"; Filename: "{app}\momospos.exe"; Tasks: desktopicon; IconFilename: "{app}\Resources\logo.ico"

[Run]
; Ejecutar el sistema después de la instalación
Filename: "{app}\momospos.exe"; Description: "{cm:LaunchProgram,MomosPOS}"; Flags: nowait postinstall skipifsilent
