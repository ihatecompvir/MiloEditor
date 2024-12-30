# MiloEditor

*Can we have Milo.exe?*

*Mom: We have Milo.exe at home*

*Milo.exe at home:*

MiloEditor is an editor for Milo scenes. Currently suited for Rock Band 3 (and Dance Central 1) on Xbox 360 and PS3, however support is planned for additional games.

Also included is MiloLib, the .NET Standard library powering MiloEditor.

## Features

- Parsing and viewing of complex Milo scene hierarchies, including inlined subdirs, *their* inlined subdirs, and directories as entries

- Barebones viewer and editor for a lot of common Milo assets

- Removing and renaming assets inside of scenes

### In the Future...

* Rich editors for certain types such as textures and fonts

* Converting assets between engine versions and platforms, when applicable

* Performance optimization

* A cross-platform UI, since the actual loading/saving logic is in its own .NET Standard library

## Credits

- [PikminGuts92 (Cisco)](https://github.com/PikminGuts92) - for creating the 010 Editor templates, Mackiloha, and Grim that have all been a gigantic help in understanding Milo scenes

- [RB3 Decomp team and contributors](https://github.com/DarkRTA/rb3/tree/master) - for decompiling the game into clean, human readable source code, making it significantly easier to write this
