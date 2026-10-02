\# WindowChrome Native TitleBar (.NET 10 WPF)



A modern, public-release-ready .NET 10 WPF application featuring a dynamic, toggleable custom title bar using `WindowChrome` vs. native title bar, complete with Windows 11-style control box animations, vector-based icons, and native hit-testing for snap layouts (`HTMIN`, `HTMAX`, `HTCLOSE`).



\## Features

\* \*\*Dynamic Title Bar Toggle\*\*: Switch seamlessly between a custom styled title bar and the OS-native title bar at runtime.

\* \*\*Persistent Settings\*\*: Uses standard Win32 API (`GetPrivateProfileString` / `WritePrivateProfileString`) for INI-based configuration management.

\* \*\*Windows 11 Control Box\*\*: Smooth `VisualStateManager` animations, hover color fading, and native Snap Layouts support via customized `WM\_NCHITTEST` hooks.

\* \*\*Pure Vector \& Glyph Icons\*\*: High-fidelity UI using WPF `Path` and `Segoe MDL2 Assets`.



\## Requirements

\* .NET 10 SDK

\* Windows 11 / 10

