#preproc ispp

#define FileExtractorAppName "file-extractor"
#define FileExtractorAppVersion "1.0.0"
#define FileExtractorAppPublisher "vhandziuk"
#define FileExtractorAppExeName "FileExtractor.exe"

[Code]
#include 'Utils.pas'

[Setup]
AppId={{D02E9EB9-BBB4-427B-B14D-0C85C8F1A929}
AppName={#FileExtractorAppName}
AppVersion={#FileExtractorAppVersion}
AppPublisher={#FileExtractorAppPublisher}
ChangesEnvironment=yes
DefaultDirName={autopf32}\{#FileExtractorAppName}
DisableDirPage=yes
DefaultGroupName={#FileExtractorAppName}
DisableProgramGroupPage=yes
OutputDir=..\Output
OutputBaseFilename=FileExtractorSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\FileExtractor\bin\release\net6.0\win10-x86\publish\{#FileExtractorAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\FileExtractor\bin\release\net6.0\win10-x86\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#FileExtractorAppName}"; Filename: "{app}\{#FileExtractorAppExeName}"

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
    if CurStep = ssInstall then
    begin
        EnvAddPath(ExpandConstant('{app}'));
    end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
    if CurUninstallStep = usPostUninstall then
    begin
        EnvRemovePath(ExpandConstant('{app}'));
    end;
end;