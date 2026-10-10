# File Extractor

[![Build and test](https://github.com/vhandziuk/file-extractor/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/vhandziuk/file-extractor/actions/workflows/build-and-test.yml)

Command line utility for selective unpacking of archived files.

## Description

The tool uses a CSV configuration file that provides file names and relative locations of the entries to extract. It checks all the supported archives in the given `source` path and tries to extract specific archived entries as defined in the configuration.

Supported archive types: **Zip, Rar, 7z, Tar, Bz2, Gz, Lz, Xz**

**_NOTE:_** The tool does not support password-protected archives.

**_NOTE:_** The application is built with .NET 10. The respective .NET runtime (if missing) will be deployed as a prerequisite by the application installer.

## Configuration file structure

Configuration file must contain 3 columns, separated by a comma. The default file name is `configuration.csv`, and the default file location is the same as the `source` folder.

The first column is the name of the archive entry to be extracted (e.g., test1.txt). If the file is found in any of the archives (in a specified subpath, defined by the third column), it is then unpacked to the destination path. This value also supports simple wildcards `*` and `?`.

The second column is a relative path (e.g., Root\Output). When specified, the file will be unpacked to the corresponding subfolder of the destination path.

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

The tool can be used from command line or `powershell` as follows:

``` powershell
  fileextractor -s "<source>" [-d "<destination>"] [-c "<configuration>"] [--cache-configuration] [--no-wait]
```

The application accepts following command line arguments:

```
  -c, --configuration      Path to a CSV configuration file

  -s, --source             Path to the source directory

  -d, --destination        Path to the destination directory

  --cache-configuration    Save current configuration as default

  --no-wait                Do not wait for a key press to exit the app

  --help                   Display this help screen.

  --version                Display version information.
```

`-s` or `--source` /mandatory/ — is the full path to the directory containing archived files. If the argument is not provided or provided directory does not exist, the application will print a warning message and exit.

`-d` or `--destination` /optional/ — is the full path to the output directory (where extracted files will be placed). If the argument is not provided, the default output location will be the same as `<source>`.

`-c` or `--configuration` /optional/ — is the full path to the configuration `.csv` file. If the argument is not provided, the default `<source>\configuration.csv` path is assumed. If the configuration file does not exist, the application will try to look for the cached `configuration.csv` file under `%programdata%\File Extractor` (see `--cache-configuration` below). If both the configuration file and cached configuration are not available, the application will print a warning message and exit.

`--cache-configuration` /optional/ — when provided, the application will cache the used configuration `.csv` file to `%programdata%\File Extractor\configuration.csv` upon successful extraction.

`--no-wait` /optional/ — when provided, the application exits right after the extraction instead of waiting for a key press. Useful for scripts and automation.

The help screen and application version can be displayed with the following commands:

``` powershell
  fileextractor --help
```

``` powershell
  fileextractor --version
```

**_NOTE:_** The tool creates the "File Extractor" folder context menu entry. When executed, the selected folder will be used as the `--source` argument for the application, `<destination>` and `<configuration>` will have their default values (see the description above).

**_NOTE:_** The tool generates log files under `%programdata%\File Extractor\Logs`. The files can be used to check extended application output and error messages.

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
