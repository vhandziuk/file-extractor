# File Extractor

[![Build and test](https://github.com/vhandziuk/file-extractor/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/vhandziuk/file-extractor/actions/workflows/build-and-test.yml)

Command line utility for selective unpacking of archived files.

## Description

The tool extracts selected files from archives in two ways:

- with the archive and the files named on the command line, similar to `unzip` or `7z x` (the `extract` verb, used by default);
- with a CSV configuration file that provides file names and relative locations of the entries to extract. It checks all the supported archives in the given `source` path and tries to extract specific archived entries as defined in the configuration (the `config` verb).

The `list` verb shows the files in archives without extracting them.

Supported archive types: **Zip, Rar, 7z, Tar, Bz2, Gz, Lz, Xz**

**_NOTE:_** The tool does not support password-protected archives.

**_NOTE:_** The application is built with .NET 10. The respective .NET runtime (if missing) will be deployed as a prerequisite by the application installer.

## Configuration file structure

Configuration file must contain 3 columns, separated by a comma. The default file name is `configuration.csv`, and the default file location is the same as the `source` folder.

The first column is the name of the archive entry to be extracted (e.g., test1.txt). If the file is found in any of the archives (in a specified subpath, defined by the third column), it is then unpacked to the destination path. This value also supports simple wildcards `*` and `?`.

The second column is a relative path (e.g., Root\Output). When specified, the file will be unpacked to the corresponding subfolder of the destination path. The path must stay inside the destination path: absolute paths, drive letters and `..` segments are rejected as a malformed configuration.

The third column is a part of the relative subpath of the entry in archive. If the value is left blank, the entry is searched for in any folder of the archive, including the archive root.

### Example archive data and configuration file

Let's imagine we have an archive with the following structure:

<pre>
📦Archive
 ┣ 📂Subfolder01
 ┃ ┣ 📂Nested
 ┃ ┃ ┣ 📜test05.txt
 ┃ ┃ ┗ 📜test06.txt
 ┃ ┣ 📜test03.txt
 ┃ ┗ 📜test04.txt
 ┣ 📂Subfolder02
 ┃ ┣ 📜test07.txt
 ┃ ┗ 📜test08.txt
 ┣ 📜test01.txt
 ┗ 📜test02.txt
 </pre>

 Let's consider that we want to extract every even file to the root of the destination directory, and every odd file - to the `Output` subfolder of the destination directory. Then, the corresponding configuration file will look as follows:

 ```
test01.txt,,
test03.txt,,Subfolder01
test05.txt,,Nested
test07.txt,,Subfolder02
test02.txt,Output,
test04.txt,Output,Subfolder01
test06.txt,Output,Subfolder01\Nested
test08.txt,Output,Subfolder02
 ```

**_NOTE:_** It is not required to provide a relative subpath to archive entries. It is only necessary when archives contain multiple entries with the same file name but located in different subfolders of the archive.

**_NOTE:_** It is not necessary to provide the complete relative subpath to the entry in archive. When provided, however, the subpath must include at least the parent path of the entry to be extracted.

## Basic use

The tool can be used from command line or `powershell`. The first argument selects what it does:

| Verb | Alias | Description |
| --- | --- | --- |
| `extract` | `x` | Extract files from archives named on the command line. This is the default verb and can be omitted. |
| `config` | | Extract the files listed in a CSV configuration from the archives in a folder. |
| `list` | `l` | List the files in archives without extracting them. |

``` powershell
  fileextractor [extract] "<archive>" [<file>...] [-o "<output>"] [-x <file>...] [--flat] [--archive-folder] [-r] [-p <count>] [--follow-symlinks] [--no-wait]
  fileextractor config -s "<source>" [-d "<destination>"] [-r] [-c "<configuration>"] [--cache-configuration] [-p <count>] [--follow-symlinks] [--no-wait]
  fileextractor list "<archive>" [-r] [--no-wait]
```

**_NOTE:_** In earlier versions, the configuration mode was the default (`fileextractor -s "<source>"`). It now needs the `config` verb: `fileextractor config -s "<source>"`. An archive named exactly like a verb (for example, `list`) needs the explicit `x` verb: `fileextractor x list`.

The help screen and application version can be displayed with the following commands:

``` powershell
  fileextractor --help
  fileextractor <verb> --help
  fileextractor --version
```

Pressing `Ctrl+C` stops the extraction after the files currently being written; partially extracted files are removed. Pressing `Ctrl+C` again terminates the application immediately.

**_NOTE:_** The installer adds the following entries to the Explorer context menu (on Windows 11, under "Show more options"):

| Right-click on | Entry | Runs |
| --- | --- | --- |
| A supported archive | File Extractor: Extract here | `fileextractor x "<archive>" -o "<folder of the archive>"` |
| A supported archive | File Extractor: Extract to subfolder | the same with `--archive-folder`, so the files go to a subfolder named after the archive |
| A folder or the background of a folder | File Extractor: Extract archives here | `fileextractor x "<folder>" -o "<folder>"`: every archive located directly in the folder, into that folder |
| A folder or the background of a folder | File Extractor: Extract using configuration | `fileextractor config -s "<folder>"`, with the default `<destination>` and `<configuration>` |

**_NOTE:_** The tool generates log files under `%localappdata%\File Extractor\Logs` of the current user. The files can be used to check extended application output and error messages.

### Extracting with a configuration file (`config`)

``` powershell
  fileextractor config -s "<source>" [-d "<destination>"] [-r] [-c "<configuration>"] [--cache-configuration] [-p <count>] [--follow-symlinks] [--no-wait]
```

The verb accepts following command line arguments:

```
  -c, --configuration      Path to a CSV configuration file

  -s, --source             Path to the source directory

  -d, --destination        Path to the destination directory

  -r, --recursive          Search for archives in subdirectories of the source directory as well

  --cache-configuration    Save current configuration as default

  -p, --max-parallelism    Maximum number of archives extracted in parallel (default: number of processors)

  --follow-symlinks        Extract files into folders below the destination directory that are symbolic links or junctions

  --no-wait                Do not wait for a key press to exit the app
```

`-s` or `--source` /mandatory/ — is the full path to the directory containing archived files. If the argument is not provided or provided directory does not exist, the application will print a warning message and exit.

`-d` or `--destination` /optional/ — is the full path to the output directory (where extracted files will be placed). If the argument is not provided, the default output location will be the same as `<source>`.

`-r` or `--recursive` /optional/ — when provided, archives are searched in `<source>` and all its subdirectories. By default, only archives located directly in `<source>` are processed.

`-c` or `--configuration` /optional/ — is the full path to the configuration `.csv` file. If the argument is not provided, the default `<source>\configuration.csv` path is assumed. If the configuration file does not exist, the application will try to look for the cached `configuration.csv` file under `%localappdata%\File Extractor` of the current user (see `--cache-configuration` below). If both the configuration file and cached configuration are not available, the application will print a warning message and exit.

`--cache-configuration` /optional/ — when provided, the application will cache the used configuration `.csv` file to `%localappdata%\File Extractor\configuration.csv` upon successful extraction. The cached configuration is kept per user.

`-p` or `--max-parallelism` /optional/ — is the maximum number of archives extracted at the same time. If the argument is not provided or is not a positive number, the number of processors is used. Lower it (for example, `-p 1`) when the archives are on a slow hard drive.

`--follow-symlinks` /optional/ — when provided, files are also extracted into subfolders of `<destination>` that are symbolic links or junctions (for example, a subfolder linked to another drive). By default, such destinations are skipped with a warning, because a link planted below the destination could redirect the extraction anywhere. Use it only when you trust everyone who can write to `<destination>`. Destinations outside `<destination>` remain rejected, and the log and cached configuration folders are protected regardless of this option.

`--no-wait` /optional/ — when provided, the application exits right after the extraction instead of waiting for a key press. Useful for scripts and automation.

### Extracting files named on the command line (`extract`, `x`)

``` powershell
  fileextractor [extract] "<archive>" [<file>...] [-o "<output>"] [-x <file>...] [--flat] [--archive-folder] [-r] [-p <count>] [--follow-symlinks] [--no-wait]
```

This is the default verb, so `extract` (or its alias `x`) can be omitted. The verb accepts following command line arguments:

```
  archive (pos. 0)         Required. Archive file, folder with archives, or wildcard path such as C:\Archives\*.zip

  files (pos. 1)           Files to extract: names or paths in the archive, with * and ? wildcards in the name (default: all files)

  -o, --output             Path to the output directory, created when missing (default: current directory)

  -x, --exclude            Files not to extract, in the same format as the files to extract. Takes all the values that follow it, so put it after the archive and the files, for example: fileextractor a.zip *.txt -x *.tmp *.log

  --flat                   Extract all files directly into the output directory, without the folders of the archive

  --archive-folder         Extract each archive into a subdirectory of the output directory named after the archive

  -r, --recursive          Search for archives in subdirectories as well when the archive is a folder or wildcard path

  -p, --max-parallelism    Maximum number of archives extracted in parallel (default: number of processors)

  --follow-symlinks        Extract files into folders below the output directory that are symbolic links or junctions

  --no-wait                Do not wait for a key press to exit the app
```

`<archive>` /mandatory/ — is an archive file, a directory (all supported archives in it are processed) or a path with the `*` and `?` wildcards in its last segment (for example, `C:\Archives\*.7z`). With `-r`, directories and wildcard paths are searched in subdirectories as well. If no supported archive is found, the application will print a warning message and exit.

`<file>...` /optional/ — are the files to extract, in the same form as the first and third columns of the configuration file: a file name with optional `*` and `?` wildcards (for example, `*.dll`), optionally preceded by the folder it is located in inside the archive (for example, `docs/*.txt` or `docs\readme.txt`). A file without a folder is searched for in any folder of the archive; a folder matches when the parent path of the entry ends with it. Wildcards are not supported in the folder. When no file is provided, all files are extracted.

`-o` or `--output` /optional/ — is the path to the output directory. It is created when it does not exist. If the argument is not provided, the current directory is used.

`-x` or `--exclude` /optional/ — are files that must not be extracted, in the same format as `<file>`. The option takes all the values that follow it, so put it after the files to extract (for example, `fileextractor x a.zip *.txt -x *.tmp *.log`).

`--flat` /optional/ — when provided, all files are written directly into `<output>`. By default, the folders of the archive are recreated below `<output>`, like `unzip` and `7z x` do. With `--flat`, a file with the same name as an already extracted one is skipped with a warning.

`--archive-folder` /optional/ — when provided, each archive is extracted into a subdirectory of `<output>` named after the archive without its extension (for example, `<output>\Backup` for `Backup.zip`), so archives that contain files with the same names do not collide.

`-r`, `-p`, `--follow-symlinks` and `--no-wait` work the same way as for the `config` verb.

Examples:

``` powershell
  # extract everything from an archive into the current directory, keeping its folders
  fileextractor "C:\Downloads\package.zip"

  # extract the text files of the docs folder and all DLLs from every 7z archive in a folder
  fileextractor x "C:\Archives\*.7z" "docs/*.txt" "*.dll" -o "C:\Output"

  # extract all files except temporary ones, one subdirectory per archive, without the archive folders
  fileextractor x "C:\Archives" -o "C:\Output" --archive-folder --flat -x "*.tmp"
```

### Listing archive contents (`list`, `l`)

``` powershell
  fileextractor list "<archive>" [-r] [--no-wait]
```

The verb prints the path of each archive that `<archive>` denotes (a file, a directory or a wildcard path, as for the `extract` verb), followed by the paths of the files it contains. `-r` searches directories and wildcard paths in subdirectories as well. An archive that cannot be read is reported in the log and skipped.

## Security

Archives and configuration files are treated as untrusted input:

- Archive entries whose names are not plain file names (path separators, `..`, alternate data streams, reserved device names such as `CON`) are skipped, and no file is ever written outside the destination path.
- When the `extract` verb keeps the folders of an archive, every folder of an entry must be a plain folder name as well; entries with `..`, drive letters, alternate data streams or reserved device names in their path are skipped.
- Files are not written through a symbolic link or junction below the destination path unless `--follow-symlinks` is provided, and existing files are never overwritten.
- Logs and the cached configuration are kept per user, so another user cannot redirect them; the log folder is labeled with Medium integrity, even when the tool runs elevated.
- Native DLLs are not loaded from the current folder or from `PATH`.

## Building from source

Prerequisites:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Inno Setup 7](https://jrsoftware.org/isinfo.php) (only required to build the installers)

Build and run the unit tests:

``` powershell
  dotnet build
  dotnet test
```

The tests use xUnit v3 on Microsoft Testing Platform (enabled for `dotnet test` in `global.json`).

Publish the application and build the installers:

``` powershell
  dotnet publish .\FileExtractor\FileExtractor.csproj --configuration release --framework net10.0 --runtime win-x86 --no-self-contained
  dotnet publish .\FileExtractor\FileExtractor.csproj --configuration release --framework net10.0 --runtime win-x64 --no-self-contained
  iscc.exe /Qp .\Setup\Setup_x86.iss
  iscc.exe /Qp .\Setup\Setup_x64.iss
```

The installers are written to the `Output` folder. They install the .NET 10 runtime as a prerequisite when it is missing.

## License

File Extractor is licensed under the [MIT License](LICENSE).

The application and its installer use third-party packages and code. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for their copyright notices and licenses.
