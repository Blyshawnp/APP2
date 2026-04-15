; ============================================================
;  Mock Testing Suite — Inno Setup 6 installer script
;  Build with: ISCC.exe /DAppVersion=1.0.0 installer\MTS.iss
;  Or via:     .\build.ps1
; ============================================================

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#define AppName      "Mock Testing Suite"
#define AppPublisher "ACD"
#define AppExeName   "MTS.UI.exe"
#define AppId        "{E22872F6-3DB1-44D5-A314-AAB4610A6B6C}"
#define PublishDir   "..\publish\win-x64"
#define OutputDir    "..\dist"

[Setup]
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
OutputDir={#OutputDir}
OutputBaseFilename=MTS-Setup-{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
WizardResizable=yes
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}

; Run without admin rights — app data goes to %APPDATA%\MTS
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; Windows 10 minimum (matches .NET 8 requirements)
MinVersion=10.0.17763

; Prevent running installer while app is open
CloseApplications=yes
CloseApplicationsFilter=*.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; \
  Description: "{cm:CreateDesktopIcon}"; \
  GroupDescription: "{cm:AdditionalIcons}"

[Files]
; All files from the publish output (typically just MTS.UI.exe for single-file publish)
Source: "{#PublishDir}\*"; \
  DestDir: "{app}"; \
  Flags: ignoreversion recursesubdirs createallsubdirs; \
  Excludes: "*.pdb"

[Icons]
; Start menu shortcut
Name: "{group}\{#AppName}";           Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"

; Desktop shortcut (optional, selected during install)
Name: "{autodesktop}\{#AppName}"; \
  Filename: "{app}\{#AppExeName}"; \
  Tasks: desktopicon

[Run]
; Offer to launch immediately after install
Filename: "{app}\{#AppExeName}"; \
  Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; \
  Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Remove the app data folder on uninstall (optional — comment out to preserve history)
; Type: filesandordirs; Name: "{userappdata}\MTS"
