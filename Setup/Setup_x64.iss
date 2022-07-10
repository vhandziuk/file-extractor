#preproc ispp

#define FileExtractorAppName "file-extractor"
#define FileExtractorAppVersion "1.0.0"
#define FileExtractorAppPublisher "vhandziuk"
#define FileExtractorAppExeName "FileExtractor.exe"

[Code]
#include 'Utils.pas'
#include 'Dependencies.pas'

[Setup]
#ifndef NoDependencies

; comment out dependency defines to disable installing them
; #define UseDotNet35
; #define UseDotNet40
; #define UseDotNet45
; #define UseDotNet46
; #define UseDotNet47
; #define UseDotNet48

; requires netcorecheck.exe and netcorecheck_x64.exe (see download link below)
#define UseNetCoreCheck
#ifdef UseNetCoreCheck
;   #define UseNetCore31
;   #define UseNetCore31Asp
;   #define UseNetCore31Desktop
;   #define UseDotNet50
;   #define UseDotNet50Asp
;   #define UseDotNet50Desktop
   #define UseDotNet60
;   #define UseDotNet60Asp
;  #define UseDotNet60Desktop
#endif

; #define UseVC2005
; #define UseVC2008
; #define UseVC2010
; #define UseVC2012
; #define UseVC2013
; #define UseVC2015To2022

; requires dxwebsetup.exe (see download link below)
; #define UseDirectX

; #define UseSql2008Express
; #define UseSql2012Express
; #define UseSql2014Express
; #define UseSql2016Express
; #define UseSql2017Express
; #define UseSql2019Express

; #define UseWebView2

#endif

AppId={{D02E9EB9-BBB4-427B-B14D-0C85C8F1A929}
AppName={#FileExtractorAppName}
AppVersion={#FileExtractorAppVersion}
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

PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

SetupIconFile=icon.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
#ifdef UseNetCoreCheck
; download netcorecheck.exe: https://go.microsoft.com/fwlink/?linkid=2135256
; download netcorecheck_x64.exe: https://go.microsoft.com/fwlink/?linkid=2135504
Source: "bin\netcorecheck.exe"; Flags: dontcopy noencryption
Source: "bin\netcorecheck_x64.exe"; Flags: dontcopy noencryption
#endif

Source: "icon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\FileExtractor\bin\release\net6.0\win10-x64\publish\{#FileExtractorAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\FileExtractor\bin\release\net6.0\win10-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

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

function InitializeSetup: Boolean;
begin
#ifdef UseDotNet35
  Dependency_AddDotNet35;
#endif
#ifdef UseDotNet40
  Dependency_AddDotNet40;
#endif
#ifdef UseDotNet45
  Dependency_AddDotNet45;
#endif
#ifdef UseDotNet46
  Dependency_AddDotNet46;
#endif
#ifdef UseDotNet47
  Dependency_AddDotNet47;
#endif
#ifdef UseDotNet48
  Dependency_AddDotNet48;
#endif

#ifdef UseNetCore31
  Dependency_AddNetCore31;
#endif
#ifdef UseNetCore31Asp
  Dependency_AddNetCore31Asp;
#endif
#ifdef UseNetCore31Desktop
  Dependency_AddNetCore31Desktop;
#endif
#ifdef UseDotNet50
  Dependency_AddDotNet50;
#endif
#ifdef UseDotNet50Asp
  Dependency_AddDotNet50Asp;
#endif
#ifdef UseDotNet50Desktop
  Dependency_AddDotNet50Desktop;
#endif
#ifdef UseDotNet60
  Dependency_AddDotNet60;
#endif
#ifdef UseDotNet60Asp
  Dependency_AddDotNet60Asp;
#endif
#ifdef UseDotNet60Desktop
  Dependency_AddDotNet60Desktop;
#endif

#ifdef UseVC2005
  Dependency_AddVC2005;
#endif
#ifdef UseVC2008
  Dependency_AddVC2008;
#endif
#ifdef UseVC2010
  Dependency_AddVC2010;
#endif
#ifdef UseVC2012
  Dependency_AddVC2012;
#endif
#ifdef UseVC2013
  //Dependency_ForceX86 := True; // force 32-bit install of next dependencies
  Dependency_AddVC2013;
  //Dependency_ForceX86 := False; // disable forced 32-bit install again
#endif
#ifdef UseVC2015To2022
  Dependency_AddVC2015To2022;
#endif

#ifdef UseDirectX
  ExtractTemporaryFile('dxwebsetup.exe');
  Dependency_AddDirectX;
#endif

#ifdef UseSql2008Express
  Dependency_AddSql2008Express;
#endif
#ifdef UseSql2012Express
  Dependency_AddSql2012Express;
#endif
#ifdef UseSql2014Express
  Dependency_AddSql2014Express;
#endif
#ifdef UseSql2016Express
  Dependency_AddSql2016Express;
#endif
#ifdef UseSql2017Express
  Dependency_AddSql2017Express;
#endif
#ifdef UseSql2019Express
  Dependency_AddSql2019Express;
#endif

#ifdef UseWebView2
  Dependency_AddWebView2;
#endif

  Result := True;
end;
