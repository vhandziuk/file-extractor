// Maintains the application directory in the system PATH environment variable:
// it is added on install and removed again on uninstall.

const
  SystemEnvironmentKey = 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment';
  SystemPathValueName = 'Path';

function NormalizePathEntry(const Entry: String): String;
begin
  Result := RemoveBackslashUnlessRoot(Trim(Entry));
end;

// PATH entries are compared as whole entries, ignoring case and a trailing backslash,
// so that a directory is never matched by another directory that merely contains its name
function IsSamePathEntry(const Entry, Directory: String): Boolean;
begin
  Result := CompareText(NormalizePathEntry(Entry), NormalizePathEntry(Directory)) = 0;
end;

function ReadSystemPath: String;
var
  Value: String;
begin
  if not RegQueryStringValue(HKLM, SystemEnvironmentKey, SystemPathValueName, Value) then
    Value := '';
  Result := Value;
end;

// The system PATH is written as REG_EXPAND_SZ (the Windows default for this value),
// so that entries referencing variables such as %SystemRoot% keep expanding
procedure WriteSystemPath(const Value: String);
begin
  if not RegWriteExpandStringValue(HKLM, SystemEnvironmentKey, SystemPathValueName, Value) then
    Log('Failed to update the system PATH environment variable');
end;

function ContainsPathEntry(const Entries: TArrayOfString; const Directory: String): Boolean;
var
  Index: Integer;
begin
  Result := False;
  for Index := 0 to GetArrayLength(Entries) - 1 do
  begin
    if IsSamePathEntry(Entries[Index], Directory) then
    begin
      Result := True;
      exit;
    end;
  end;
end;

procedure AddDirectoryToSystemPath(const Directory: String);
var
  Path: String;
begin
  Path := ReadSystemPath;
  if ContainsPathEntry(StringSplit(Path, [';'], stAll), Directory) then
    exit;

  if (Path <> '') and (Path[Length(Path)] <> ';') then
    Path := Path + ';';
  WriteSystemPath(Path + Directory);
end;

procedure RemoveDirectoryFromSystemPath(const Directory: String);
var
  Entries: TArrayOfString;
  RemainingPath: String;
  Index: Integer;
  Removed: Boolean;
begin
  Entries := StringSplit(ReadSystemPath, [';'], stAll);
  RemainingPath := '';
  Removed := False;

  for Index := 0 to GetArrayLength(Entries) - 1 do
  begin
    if IsSamePathEntry(Entries[Index], Directory) then
      Removed := True
    else
      RemainingPath := RemainingPath + ';' + Entries[Index];
  end;

  // every kept entry was prefixed with a separator, so the first character is dropped
  if Removed then
    WriteSystemPath(Copy(RemainingPath, 2, Length(RemainingPath)));
end;
