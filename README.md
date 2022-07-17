# File Extractor

Command line utility for selective unpacking of archived files.

## Description

The tool uses a CSV configuration file that provides file names and relative locations of the files to extract. It looks for all supported archives in a given path and tries to extract files as specified in the configuration.

Supported archive types:
- Zip
- Rar
- 7z
- Tar
- Bz2
- Gz
- Lz
- Xz

Built with .NET 6.

## Command line options

```
  -c, --configuration      Path to a CSV configuration file

  -s, --source             Path to the source directory

  -d, --destination        Path to the destination directory

  --cache-configuration    Save current configuration as default

  --help                   Display this help screen.

  --version                Display version information.
```

## Structure of a configuration file

Configuration file must contain 2 values, separated by a comma.

The first value is the name of the archive entry to be extracted (e.g., test1.txt). If the file is found in any of the archives in a specified path, it is then unpacked to the destination path. File name can optionally be prefixed by a relative path, (e.g., Output\test1.txt). In this case, the file will be unpacked to the corresponding sub-folder of the destination path.

The second value is a part of the relative sub-path of the entry in archive. If the entry is located in the archive root, the value should be left blank.

´´´
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
´´´

## Basic use
