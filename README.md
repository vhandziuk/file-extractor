# File Extractor

Command line utility for selective unpacking of archived files.

## Description

The tool uses a CSV configuration file that provides file names and relative locations of the entries to extract. It checks all the supported archives in the given `source` path and tries to extract specific archived entries as defined in the configuration.

Supported archive types:
- Zip
- Rar
- 7z
- Tar
- Bz2
- Gz
- Lz
- Xz

The application is built with .NET 6.

## Configuration file structure

Configuration file must contain 2 values, separated by a comma. The default file name is `configuration.csv`, and the default file location is the same as the `source` folder.

The first value is the name of the archive entry to be extracted (e.g., test1.txt). If the file is found in any of the archives in a specified path, it is then unpacked to the destination path. File name can optionally be prefixed by a relative path, (e.g., Output\test1.txt). In this case, the file will be unpacked to the corresponding sub-folder of the destination path.

The second value is a part of the relative sub-path of the entry in archive. If the entry is located in the archive root, the value should be left blank.

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

 Let's consider that we want to extract every even file to the root of the destination directory, and every odd file - to an `Output` subfolder of the destination directory. Then, the corresponding configuration file will look as follows:

 ```
test01.txt,
test03.txt,Subfolder01
test05.txt,Nested
test07.txt,Subfolder02
Output\test02.txt,
Output\test04.txt,Subfolder01
Output\test06.txt,Subfolder01\Nested
Output\test08.txt,Subfolder02
 ```

Note: It is not required to provide a relative sub-path to archive entries. It is only necessary when archives contain multiple entries with the same file name but located in different sub-folders of the archive.

Note: It is not necessary to provide the complete relative sub-path to the entry in archive. When provided, however, the sub-path must include at least the parent path of the entry to be extracted.

## Basic use

The tool accepts following command line arguments:

```
  -c, --configuration      Path to a CSV configuration file

  -s, --source             Path to the source directory

  -d, --destination        Path to the destination directory

  --cache-configuration    Save current configuration as default

  --help                   Display this help screen.

  --version                Display version information.
```
