#preproc ispp

#define FileExtractorAppName "File Extractor"
#define FileExtractorAppVersion "1.0.0.0"
#define FileExtractorAppPublisher "vhandziuk"
#define FileExtractorAppExeName "FileExtractor.exe"

[Registry]
; the single "File Extractor" entry of earlier versions, which ran the configuration mode
Root: HKCR; Subkey: "Directory\Background\shell\File_Extractor"; ValueType: none; Flags: deletekey
Root: HKCR; Subkey: "Directory\shell\File_Extractor"; ValueType: none; Flags: deletekey

; the archive types of ArchiveTypeResolver
#dim ArchiveExtensions[8] {".zip", ".rar", ".7z", ".tar", ".bz2", ".gz", ".lz", ".xz"}
#dim FolderShellKeys[2] {"Directory\shell", "Directory\Background\shell"}
#define i

; "%1\.." is the folder of the archive: the application resolves the output path to a full path
#sub RegisterArchiveVerbs
Root: HKCR; Subkey: "SystemFileAssociations\{#ArchiveExtensions[i]}\shell\FileExtractor.ExtractHere"; ValueType: string; ValueData: "File Extractor: Extract here"; Flags: uninsdeletekey
Root: HKCR; Subkey: "SystemFileAssociations\{#ArchiveExtensions[i]}\shell\FileExtractor.ExtractHere"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\{#FileExtractorAppExeName}"
Root: HKCR; Subkey: "SystemFileAssociations\{#ArchiveExtensions[i]}\shell\FileExtractor.ExtractHere\command"; ValueType: string; ValueData: """{app}\{#FileExtractorAppExeName}"" x ""%1"" -o ""%1\.."""
Root: HKCR; Subkey: "SystemFileAssociations\{#ArchiveExtensions[i]}\shell\FileExtractor.ExtractToFolder"; ValueType: string; ValueData: "File Extractor: Extract to subfolder"; Flags: uninsdeletekey
Root: HKCR; Subkey: "SystemFileAssociations\{#ArchiveExtensions[i]}\shell\FileExtractor.ExtractToFolder"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\{#FileExtractorAppExeName}"
Root: HKCR; Subkey: "SystemFileAssociations\{#ArchiveExtensions[i]}\shell\FileExtractor.ExtractToFolder\command"; ValueType: string; ValueData: """{app}\{#FileExtractorAppExeName}"" x ""%1"" -o ""%1\.."" --archive-folder"
#endsub

#sub RegisterFolderVerbs
Root: HKCR; Subkey: "{#FolderShellKeys[i]}\FileExtractor.ExtractArchivesHere"; ValueType: string; ValueData: "File Extractor: Extract archives here"; Flags: uninsdeletekey
Root: HKCR; Subkey: "{#FolderShellKeys[i]}\FileExtractor.ExtractArchivesHere"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\{#FileExtractorAppExeName}"
Root: HKCR; Subkey: "{#FolderShellKeys[i]}\FileExtractor.ExtractArchivesHere\command"; ValueType: string; ValueData: """{app}\{#FileExtractorAppExeName}"" x ""%V"" -o ""%V"""
Root: HKCR; Subkey: "{#FolderShellKeys[i]}\FileExtractor.ExtractWithConfiguration"; ValueType: string; ValueData: "File Extractor: Extract using configuration"; Flags: uninsdeletekey
Root: HKCR; Subkey: "{#FolderShellKeys[i]}\FileExtractor.ExtractWithConfiguration"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\{#FileExtractorAppExeName}"
Root: HKCR; Subkey: "{#FolderShellKeys[i]}\FileExtractor.ExtractWithConfiguration\command"; ValueType: string; ValueData: """{app}\{#FileExtractorAppExeName}"" config ""%V"""
#endsub

#for {i = 0; i < DimOf(ArchiveExtensions); i++} RegisterArchiveVerbs
#for {i = 0; i < DimOf(FolderShellKeys); i++} RegisterFolderVerbs

[Code]
#include 'Utils.pas'
#include 'CodeDependencies.iss'

[CustomMessages]
NameAndVersion=%1 %2

[Setup]
AppId={{D02E9EB9-BBB4-427B-B14D-0C85C8F1A929}
AppName={#FileExtractorAppName}
AppVersion={#FileExtractorAppVersion}
AppVerName={cm:NameAndVersion,{#FileExtractorAppName},{#FileExtractorAppVersion}}
AppPublisher={#FileExtractorAppPublisher}
ChangesEnvironment=yes
DefaultDirName={autopf64}\{#FileExtractorAppName}
DisableDirPage=yes
DefaultGroupName={#FileExtractorAppName}
DisableProgramGroupPage=yes
OutputDir=..\Output
OutputBaseFilename=FileExtractorSetup_x64
Compression=lzma
SolidCompression=yes
WizardStyle=modern

; dependencies are installed for all users, which needs administrative rights
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

SetupIconFile=icon.ico
UninstallDisplayIcon={app}\icon.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "icon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\THIRD-PARTY-NOTICES.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\FileExtractor\bin\release\net10.0\win-x64\publish\*"; Excludes: "*.pdb"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#FileExtractorAppName}"; Filename: "{app}\{#FileExtractorAppExeName}"

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
    if CurStep = ssInstall then
    begin
        AddDirectoryToSystemPath(ExpandConstant('{app}'));
    end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
    if CurUninstallStep = usPostUninstall then
    begin
        RemoveDirectoryFromSystemPath(ExpandConstant('{app}'));
    end;
end;

function InitializeSetup: Boolean;
begin
  // the application is published for x64 only, so install the x64 runtime on ARM64 Windows too
  Dependency_ForceX64 := True;
  Dependency_AddDotNet100;

  Result := True;
end;
