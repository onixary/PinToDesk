; PinToDesk_edit Inno Setup Installer Script

[Setup]
AppName=PinToDesk_edit
AppVersion=2.1
DefaultDirName={pf}\PinToDesk_edit
DefaultGroupName=PinToDesk_edit
OutputBaseFilename=PinToDesk_edit_Installer
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Files]
Source: "..\Publish\SelfContained_V2.1\*"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\PinToDesk_edit"; Filename: "{app}\PinToDesk_edit.exe"
Name: "{commondesktop}\PinToDesk_edit"; Filename: "{app}\PinToDesk_edit.exe"; Tasks: desktopicon

[Run]
; 安装完成后自动启动（可选）
Filename: "{app}\PinToDesk_edit.exe"; Flags: nowait postinstall skipifsilent

[Tasks]
Name: "desktopicon"; Description: "在桌面创建快捷方式"; GroupDescription: "选项";
