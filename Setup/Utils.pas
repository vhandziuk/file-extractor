// Adds/removes the application directory to/from the system PATH variable
// Source: https://stackoverflow.com/a/46609047 by Wojciech Mleczek
// Licensed under CC BY-SA 3.0 (https://creativecommons.org/licenses/by-sa/3.0/), see THIRD-PARTY-NOTICES.md
// Modified: comments removed

const EnvironmentKey = 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment';

procedure EnvAddPath(Path: string);
var
    Paths: string;
begin
    if not RegQueryStringValue(HKEY_LOCAL_MACHINE, EnvironmentKey, 'Path', Paths)
    then Paths := '';

    if Pos(';' + Uppercase(Path) + ';', ';' + Uppercase(Paths) + ';') > 0 then exit;

    Paths := Paths + ';'+ Path +';'

    RegWriteStringValue(HKEY_LOCAL_MACHINE, EnvironmentKey, 'Path', Paths);
end;

procedure EnvRemovePath(Path: string);
var
    Paths: string;
    P: Integer;
begin
    if not RegQueryStringValue(HKEY_LOCAL_MACHINE, EnvironmentKey, 'Path', Paths) then
        exit;

    P := Pos(';' + Uppercase(Path) + ';', ';' + Uppercase(Paths) + ';');
    if P = 0 then exit;

    Delete(Paths, P - 1, Length(Path) + 1);

    RegWriteStringValue(HKEY_LOCAL_MACHINE, EnvironmentKey, 'Path', Paths)
end;