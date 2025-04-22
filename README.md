# MiloEditor

*Can we have Milo.exe?*

*Mom: We have Milo.exe at home*

*Milo.exe at home:*

MiloEditor is an editor for Milo scenes. There is currently varying levels of support for various Milo games, although Rock Band 3 and Dance Central 1 are the most supported. The end goal is to have support for every Milo game ever made, within reason.

Also included is MiloLib, the .NET Standard library powering MiloEditor, and MiloUtil, a cross-platform command line utility powered by MiloLib. MiloLib can be used to create your own tools and utilities to manipulate Milo scenes. While there is no documentation as of now, it is designed to be simple and straightforward to open, manipulate, and save scenes. Please see MiloUtil for an example of its usage.

## Features

- Parsing and viewing of complex Milo scene hierarchies, including inlined subdirs, *their* inlined subdirs, and directories as entries

- Viewer and editor for a lot of common Milo assets

- Searching and filtering to help you find the asset(s) you're looking for

- Exporting, importing, duplicating, and renaming assets

- Creation of entirely new Milo scenes

- The ability to add new inlined subdirectories to existing Milo scenes

- Cross-platform UI supporting MacOS, Linux, and Windows

- Texture viewer, exporter, and importer for PS3 and Xbox 360 formatted textures

### In the Future...

* More rich editors for assets such as fonts

* Converting assets between engine versions and platforms, when applicable

* Performance optimizations

* Documentation for using MiloLib to create a new tool or utility

## Credits

- [Sulfrix](https://github.com/Sulfrix) - for creating the cross-platform ImMilo GUI to replace the Windows-only WinForms UI

- [PikminGuts92 (Cisco)](https://github.com/PikminGuts92) - for creating the 010 Editor templates, Mackiloha, and Grim that have all been a gigantic help in understanding Milo scenes

- [RB3 Decomp team and contributors](https://github.com/DarkRTA/rb3/tree/master) - for decompiling the game into clean, human readable source code, making it significantly easier to write this
