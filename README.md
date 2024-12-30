# MiloEditor

*Can we have Milo.exe?*

*Mom: We have Milo.exe at home*

*Milo.exe at home:*

MiloEditor is a Windows-only editor for Milo scenes. There is currently varying levels of support for different Milo games, although Rock Band 3 and Dance Central 1 are the most supported. The end goal is to have support for every Milo game ever made, within reason.

Also included is MiloLib, the .NET Standard library powering MiloEditor, and MiloUtil, a cross-platform command line utility powered by MiloLib.

## Features

- Parsing and viewing of complex Milo scene hierarchies, including inlined subdirs, *their* inlined subdirs, and directories as entries

- Barebones viewer and editor for a lot of common Milo assets

- Exporting, importing, duplicating, and renaming assets

- Creation of entirely new Milo scenes

- The ability to add new inlined subdirectories to existing Milo scenes

### In the Future...

* Rich editors for certain types such as textures and fonts

* Converting assets between engine versions and platforms, when applicable

* Performance optimizations

* A cross-platform UI, since the actual loading/saving logic is in its own .NET Standard library

## Credits

- [PikminGuts92 (Cisco)](https://github.com/PikminGuts92) - for creating the 010 Editor templates, Mackiloha, and Grim that have all been a gigantic help in understanding Milo scenes

- [RB3 Decomp team and contributors](https://github.com/DarkRTA/rb3/tree/master) - for decompiling the game into clean, human readable source code, making it significantly easier to write this
