# File Extractor – coding instructions

Distilled for this repository from the [awesome-copilot](https://github.com/github/awesome-copilot) C#, .NET architecture, Clean Code and Object Calisthenics guidance (Copyright GitHub, Inc., MIT License; see `THIRD-PARTY-NOTICES.md`). Only rules that apply to this codebase are kept.

## Project overview

- Windows console app (`FileExtractor.exe`) that selectively extracts files from archives based on a CSV configuration. Ships as x86/x64 Inno Setup installers.
- .NET 10 (LTS), C# 14 (`LangVersion=latest`). Shared build settings live in `Directory.Build.props`; project files contain only what differs.
- Projects:
  - `FileExtractor` – entry point (`Program.cs`), command line verbs (CommandLineParser; `Application/Options`, with `config` as the default verb), one command per verb (`Application/Commands`), `App` dispatch, DI container.
  - `FileExtractor.Common` – logging abstraction (`ILogger<T>` over Serilog) and threading helpers (`ITaskRunner`, bounded parallel `ForEachAsync`).
  - `FileExtractor.Data` – CSV configuration parsing (`ICsvFileInfoProvider`, `FileInfoData`) over the `ITextFile` wrapper, and command-line file patterns (`IFilePatternParser`).
  - `FileExtractor.Utils` – archive extraction (System.IO.Compression for zip, SharpCompress for rar/7z/tar/gz/bz2/lz/xz), file system and environment wrappers.
  - `FileExtractor.UnitTest`, `FileExtractor.Common.UnitTest`, `FileExtractor.Data.UnitTest`, `FileExtractor.Utils.UnitTest` – unit tests.
- Dependency injection uses StrongInject (compile-time). Each project registers its types in a `DiRegistration/*Module.cs` class with `[Register]` attributes; the root `Container` in `FileExtractor/DiRegistration` imports the modules. Register new services there instead of creating them manually.
- Don't add static helper classes. Put shared logic, even pure functions such as `IArchiveEntryMatcher`, behind an interface with a sealed implementation that is injected, so callers can mock it. The only exception is `SerilogLoggerFactory`, which configures the process-wide Serilog logger behind `ILogger<T>`; it and `Program.cs` create the Windows hardening classes in `FileExtractor.Common/Security` (`DllSearchPathHardener`, `MediumIntegrityFileSystem`) directly, because they must run before the DI container exists.
- Archives and configuration files are untrusted. Destination paths go through `IPathValidator` (contained in the output path, plain entry file names and, when entry paths are preserved, plain folder names) and `IReparsePointGuard` (no symbolic link or junction below the output path, unless `--follow-symlinks` sets `IExtractionContext.FollowSymbolicLinks`; logging and the configuration cache never follow links), files are written without overwriting, and logs and the cached configuration live in the per-user `%LocalAppData%\File Extractor`. Keep these checks when touching the extraction pipeline.
- External APIs are hidden behind small interfaces so the code stays testable: `IFileSystemUtils`, `IEnvironment`, `IConsole`, `ITextFile`, `ITaskRunner`, `I*File` / `I*FileUtils`, `IGenericArchive`, `IGenericArchiveEntry`. Do not call `File`, `Directory`, `Environment`, `Parallel` or SharpCompress directly from business logic; extend or add a wrapper instead.
- Extraction pipeline: `App` runs the command of the parsed verb; the command finds the archives (`IArchiveFinder`), resolves paths and the files to extract (configuration read once, or command-line patterns) and builds the `ExtractionSettings` through `IExtractionSettingsFactory`; `ArchiveExtractor` owns the parallelism and runs one `IArchiveFileExtractor` (synchronous, one archive) per archive through `ITaskRunner.ForEachAsync`. Every archive type writes entries through the shared `IArchiveEntryExtractor`, which claims each destination path in the per-run `IExtractionContext` so a file has exactly one writer. `IGenericArchive.Entries` is single-pass (solid archives are read forward-only); never enumerate it twice.

## C# language

Use modern C# where it makes the code clearer:

- File-scoped namespaces and one type per file; the file name matches the type name.
- Primary constructors for services that only capture injected dependencies (C# 12).
- Collection expressions (`[]`, `[a, b]`, `[.. items]`) instead of `new[] { }`, `new List<T> { }` or `Array.Empty<T>()` (C# 12).
- `field` keyword for properties that need logic in an accessor, instead of a manual backing field (C# 14).
- Extension members (`extension(T source) { ... }` blocks) when a group of helpers extends one type (C# 14).
- Null-conditional assignment (`target?.Property = value`) (C# 14).
- `nameof(...)`, also for unbound generic types such as `nameof(List<>)` (C# 14).
- Pattern matching and switch expressions instead of `if`/`else` chains and conditional cascades.
- `is null` / `is not null` instead of `== null` / `!= null`.
- Records for immutable data (`FileInfoData`), `sealed` for classes that are not designed for inheritance.
- Nullable reference types: declare values as non-nullable, validate at the entry points (command line, configuration file), and trust the annotations elsewhere. Keep each project's existing `<Nullable>` setting unless the task is to enable it.

When editing existing code, match the surrounding style. Don't refactor unrelated code to new language features unless asked.

## Naming and formatting

- Follow `.editorconfig` (UTF-8 without BOM, CRLF, 4-space indentation, 2 for XML/YAML, tabs in `.sln`, C# style and naming rules). `.gitattributes` keeps LF in the repository and CRLF in working copies.
- PascalCase for types, methods, properties and constants; camelCase for locals and parameters; `_camelCase` for private fields; `I` prefix for interfaces.
- Allman braces: the opening brace goes on its own line for types, members and blocks.
- Return statements that end a method go on their own line.
- Intention-revealing names without abbreviations (`archiveEntry`, not `ae`).
- Comments explain *why* (design decisions, non-obvious constraints, third-party quirks), not *what*. Add XML doc comments to public members whose behavior is not obvious from the name.

## Clean code

- Small methods with a single responsibility and one level of abstraction; extract a well-named private method rather than nesting loops and conditions.
- Guard clauses and early returns instead of deep nesting and `else` after a `return`.
- SOLID: depend on the interfaces above (DIP), keep interfaces focused (ISP), keep classes focused on one reason to change (SRP).
- DRY, KISS, YAGNI: reuse the shared archive extraction base (`GenericArchiveExtractorBase`, `GenericArchiveFileUtilsBase`) for new archive types instead of duplicating logic, and don't add abstractions or options nobody needs yet.
- Object calisthenics (one indentation level, no `else`, small classes, few fields) is guidance for application logic, not a hard rule. Wrappers, DI modules and data records are exempt.

## Error handling, logging and async

- Use `async`/`await` for I/O and long-running work; CPU-bound extraction goes through `ITaskRunner`.
- Throw specific exception types with actionable messages (for example `InvalidDataException` for a malformed configuration file) instead of `Exception`.
- Top-level failures are caught and logged in `App.RunAsync`; don't swallow exceptions in lower layers.
- Log through the injected `ILogger<T>` with Serilog message templates and named properties (`_logger.Information("Extracting {File} to {Path}", name, path)`), never string interpolation.

## Tests

- xUnit v3 with Moq, running on Microsoft Testing Platform (`global.json`). Run them with `dotnet test`.
- One test class per production class, named `<ClassName>Test`, in a folder mirroring the production namespace.
- Test method names follow `MethodName_Condition_ExpectedResult`, for example `RunAsync_SourcePathDoesNotExist_DoesNotExtractFiles`.
- The class under test is the `_sut` field, created in the constructor from `Mock<T>` fields.
- Express setup with small helpers named `Let…`, for example `LetDirectoryExist(path)` and `LetFileNotExist(path)`, and `GetMocked…` factories, as in the existing tests.
- Don't write `// Arrange`, `// Act`, `// Assert` comments; separate the phases with blank lines.
- Use `[Theory]` with `[InlineData]` for variations of the same behavior.
- Cover the critical paths and edge cases (missing source, missing configuration, empty archives, existing files, wildcard matching) for every behavior change.

## Build, packaging and licensing

```powershell
dotnet build
dotnet test
dotnet publish .\FileExtractor\FileExtractor.csproj --configuration release --framework net10.0 --runtime win-x64 --no-self-contained
iscc.exe /Qp .\Setup\Setup_x64.iss   # Inno Setup 7; same for win-x86 / Setup_x86.iss
```

- Keep `Setup_x86.iss` and `Setup_x64.iss` in sync; they differ only in architecture settings.
- `Setup/CodeDependencies.iss` is vendored from DomGries/InnoDependencyInstaller. Update it by replacing it with a newer upstream version and adjusting the commit in its header and in `THIRD-PARTY-NOTICES.md`; don't edit it in place.
- When adding, removing or upgrading a NuGet package or third-party code, update `THIRD-PARTY-NOTICES.md` with its version, copyright and license.
- After changing the target framework or the CLI arguments, update `README.md`.
