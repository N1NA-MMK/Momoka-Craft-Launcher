[简体中文](README.md) | **English** | [繁體中文](README-ZH_TW.md)

<div align="center">

<img src="Plain Craft Launcher 2/Images/icon.ico" alt="Logo" width="80" height="80">

# Momoka Craft Launcher

[![Stars](https://img.shields.io/github/stars/N1NA-MMK/Momoka-Craft-Launcher?style=for-the-badge&logo=data:image/svg%2bxml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZlcnNpb249IjEiIHdpZHRoPSIxNiIgaGVpZ2h0PSIxNiI+PHBhdGggZD0iTTggLjI1YS43NS43NSAwIDAgMSAuNjczLjQxOGwxLjg4MiAzLjgxNSA0LjIxLjYxMmEuNzUuNzUgMCAwIDEgLjQxNiAxLjI3OWwtMy4wNDYgMi45Ny43MTkgNC4xOTJhLjc1MS43NTEgMCAwIDEtMS4wODguNzkxTDggMTIuMzQ3bC0zLjc2NiAxLjk4YS43NS43NSAwIDAgMS0xLjA4OC0uNzlsLjcyLTQuMTk0TC44MTggNi4zNzRhLjc1Ljc1IDAgMCAxIC40MTYtMS4yOGw0LjIxLS42MTFMNy4zMjcuNjY4QS43NS43NSAwIDAgMSA4IC4yNVoiIGZpbGw9IiNlYWM1NGYiLz48L3N2Zz4=&logoSize=auto&label=stars&labelColor=444444&color=eac54f)](https://github.com/N1NA-MMK/Momoka-Craft-Launcher/)
![GitHub Release](https://img.shields.io/github/v/release/N1NA-MMK/Momoka-Craft-Launcher?label=release&logo=github&style=for-the-badge)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/N1NA-MMK/Momoka-Craft-Launcher/build-test.yml?style=for-the-badge)

[![Issues](https://img.shields.io/github/issues/N1NA-MMK/Momoka-Craft-Launcher?style=for-the-badge&label=issues&labelColor=444444&color=1F883D&logo=github)](https://github.com/N1NA-MMK/Momoka-Craft-Launcher/issues)
[![Pull requests](https://img.shields.io/github/issues-pr/N1NA-MMK/Momoka-Craft-Launcher?style=for-the-badge&label=pull%20requests&labelColor=444444&color=1F883D&logo=github)](https://github.com/N1NA-MMK/Momoka-Craft-Launcher/pulls)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/N1NA-MMK/Momoka-Craft-Launcher/total?style=for-the-badge)
[![Bilibili](https://img.shields.io/badge/Threads-bilibili-00A4DB?style=for-the-badge&labelColor=444444&logo=bilibili)](https://space.bilibili.com/1528402807/dynamic) <br />

[download](https://github.com/N1NA-MMK/Momoka-Craft-Launcher/releases/latest) |
[Upstream repo](https://github.com/Meloong-Git/PCL)

[Submit issues](https://github.com/N1NA-MMK/Momoka-Craft-Launcher/issues/new/choose) |
[Contribution Guidelines](CONTRIBUTING.md)

</div>

Momoka Craft Launcher (MCL) is a secondary developed version based on [PCL Community Edition](https://github.com/PCL-Community/PCL-CE) (PCL CE), focusing mainly on visual and interaction adjustments without major changes to core functionality.

## Features

- **Game Management**: Multi-instance isolation, version isolation, modpack import/export, save management
- **Resource Download**: Mods, modpacks, resource packs, shaders, data packs one-click download (CurseForge / Modrinth)
- **Account System**: Microsoft login, offline login, third-party skin station (external) login
- **Java Management**: Auto-detection, manual addition, per-instance configuration
- **Theme System**: Customizable theme colors, background images/videos, blur effects
- **Multi-language**: Simplified Chinese, Traditional Chinese, English, Japanese, French, Spanish, etc.
- **Cross-architecture**: Supports x64 and ARM64 native execution

## Supported Platforms

| Operating System | Support Status | Requirements |
|---|---|---|
| Windows 10 1809 (17763) or later | ✅ Fully supported | [.NET 10 Desktop Runtime](https://get.dot.net/10) |
| Windows 8 to Windows 10 1809 (17763) | ⚠️ Expected to run; community support offered at discretion | [.NET 10 Desktop Runtime](https://get.dot.net/10) |
| Windows 7 or earlier | ❌ Not supported | N/A |
| macOS / Linux / Other OS | ⚠️ Cross-platform development only (cross-compilation) | [.NET 10 SDK](https://get.dot.net/10) |

> Always recommends using the latest version of your OS for the best experience.

## Build

### Requirements

- [.NET 10 SDK](https://get.dot.net/10)
- Windows 10 1809 or later (or set `EnableWindowsTargeting=true` for cross-compilation)
- Visual Studio 2026 or JetBrains Rider (optional)

### Command Line

```bash
# Debug build
dotnet build "Plain Craft Launcher 2\Plain Craft Launcher 2.csproj" -c Debug

# Release build
dotnet build "Plain Craft Launcher 2\Plain Craft Launcher 2.csproj" -c Release

# Publish as single file (x64)
dotnet publish "Plain Craft Launcher 2\Plain Craft Launcher 2.csproj" -c Release -r win-x64

# Publish as single file (ARM64)
dotnet publish "Plain Craft Launcher 2\Plain Craft Launcher 2.csproj" -c Release -r win-arm64
```

Build artifacts are located in `Plain Craft Launcher 2\bin\<Configuration>-<Platform>\`.

## License

- [`Plain Craft Launcher 2/`](Plain%20Craft%20Launcher%202/LICENCE) uses Custom License
- [All other directories](LICENSE) use Apache License 2.0

## Statistic

![Alt](https://repobeats.axiom.co/api/embed/3e46296e6e3a134991a783480fd2f62723bb0353.svg "Repobeats analytics image")

[![Star History Chart](https://api.star-history.com/svg?repos=N1NA-MMK/Momoka-Craft-Launcher&type=Date)](https://www.star-history.com/#N1NA-MMK/Momoka-Craft-Launcher&Date)

## Contributors

[![](https://contrib.rocks/image?repo=N1NA-MMK/Momoka-Craft-Launcher)](https://github.com/N1NA-MMK/Momoka-Craft-Launcher/graphs/contributors)

---

Momoka Craft Launcher is a secondary developed version based on PCL Community Edition, with no direct affiliation with the original PCL author 龙腾猫跃.

Special thanks to:
- [龙腾猫跃](https://github.com/Meloong-Git) — Development and open-source of the original PCL
- [PCL Community Edition](https://github.com/PCL-Community/PCL-CE) — Continued maintenance and improvements by the community
