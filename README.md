**简体中文** | [English](README-EN.md) | [繁體中文](README-ZH_TW.md)

<div align="center">

<img src="Plain Craft Launcher 2/Images/icon.ico" alt="Logo" width="80" height="80">

# PCL-Momoka

[![Stars](https://img.shields.io/github/stars/N1NA-MMK/Momoka-Craft-Launcher?style=for-the-badge&logo=data:image/svg%2bxml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZlcnNpb249IjEiIHdpZHRoPSIxNiIgaGVpZ2h0PSIxNiI+PHBhdGggZD0iTTggLjI1YS43NS43NSAwIDAgMSAuNjczLjQxOGwxLjg4MiAzLjgxNSA0LjIxLjYxMmEuNzUuNzUgMCAwIDEgLjQxNiAxLjI3OWwtMy4wNDYgMi45Ny43MTkgNC4xOTJhLjc1MS43NTEgMCAwIDEtMS4wODguNzkxTDggMTIuMzQ3bC0zLjc2NiAxLjk4YS43NS43NSAwIDAgMS0xLjA4OC0uNzlsLjcyLTQuMTk0TC44MTggNi4zNzRhLjc1Ljc1IDAgMCAxIC40MTYtMS4yOGw0LjIxLS42MTFMNy4zMjcuNjY4QS43NS43NSAwIDAgMSA4IC4yNVoiIGZpbGw9IiNlYWM1NGYiLz48L3N2Zz4=&logoSize=auto&label=stars&labelColor=444444&color=eac54f)](https://github.com/N1NA-MMK/Momoka-Craft-Launcher/)
![GitHub Release](https://img.shields.io/github/v/release/N1NA-MMK/Momoka-Craft-Launcher?label=release&logo=github&style=for-the-badge)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/N1NA-MMK/Momoka-Craft-Launcher/build-test.yml?style=for-the-badge)

[![Issues](https://img.shields.io/github/issues/N1NA-MMK/Momoka-Craft-Launcher?style=for-the-badge&label=issues&labelColor=444444&color=1F883D&logo=github)](https://github.com/N1NA-MMK/Momoka-Craft-Launcher/issues)
[![Pull requests](https://img.shields.io/github/issues-pr/N1NA-MMK/Momoka-Craft-Launcher?style=for-the-badge&label=pull%20requests&labelColor=444444&color=1F883D&logo=github)](https://github.com/N1NA-MMK/Momoka-Craft-Launcher/pulls)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/N1NA-MMK/Momoka-Craft-Launcher/total?style=for-the-badge)
[![哔哩哔哩](https://img.shields.io/badge/动态-bilibili-00A4DB?style=for-the-badge&labelColor=444444&logo=bilibili)](https://space.bilibili.com/1528402807/dynamic) <br />

[下载最新版](https://github.com/N1NA-MMK/Momoka-Craft-Launcher/releases/latest) |
[上游存储库](https://github.com/Meloong-Git/PCL)

[提交问题](https://github.com/N1NA-MMK/Momoka-Craft-Launcher/issues/new/choose) |
[贡献指南](CONTRIBUTING.md)

</div>

PCL-Momoka（简称 MCL）是基于 [PCL 社区版](https://github.com/PCL-Community/PCL-CE)（PCL CE）二次开发的版本，主要在视觉与交互体验上进行调整，未对核心功能做大幅改动。

## 功能特性

- **游戏管理**：多实例隔离、版本隔离、整合包导入导出、存档管理
- **资源下载**：Mod、整合包、资源包、光影、数据包一键下载（CurseForge / Modrinth）
- **账号系统**：微软正版登录、离线登录、第三方皮肤站（外置登录）登录
- **Java 管理**：自动检测、手动添加、按实例配置
- **主题系统**：可自定义主题色、背景图片/视频、毛玻璃效果
- **多语言**：简中、繁中、英语、日语、法语、西班牙语等
- **跨架构**：支持 x64 与 ARM64 原生运行

## 支持平台

| 操作系统 | 支持情况 | 环境要求 |
|---|---|---|
| Windows 10 1809 (17763) 或更高 | ✅ 完整支持 | [.NET 10 Desktop Runtime](https://get.dot.net/10) |
| Windows 8 — Windows 10 1809 | ⚠️ 酌情社区支持 | [.NET 10 Desktop Runtime](https://get.dot.net/10) |
| Windows 7 或更低 | ❌ 不支持 | / |
| macOS / Linux | ⚠️ 仅支持交叉编译开发 | [.NET 10 SDK](https://get.dot.net/10) |

> 始终建议使用最新版本的操作系统以获得最佳体验。

## 构建

### 环境要求

- [.NET 10 SDK](https://get.dot.net/10)
- Windows 10 1809 或更高（或配置 `EnableWindowsTargeting=true` 进行交叉编译）
- Visual Studio 2026 或 JetBrains Rider（可选）

### 命令行构建

```bash
# Debug 构建
dotnet build "Plain Craft Launcher 2\Plain Craft Launcher 2.csproj" -c Debug

# Release 构建
dotnet build "Plain Craft Launcher 2\Plain Craft Launcher 2.csproj" -c Release

# 发布为单文件（x64）
dotnet publish "Plain Craft Launcher 2\Plain Craft Launcher 2.csproj" -c Release -r win-x64

# 发布为单文件（ARM64）
dotnet publish "Plain Craft Launcher 2\Plain Craft Launcher 2.csproj" -c Release -r win-arm64
```

构建产物位于 `Plain Craft Launcher 2\bin\<Configuration>-<Platform>\` 目录下。

### 构建配置

| 配置 | 说明 |
|---|---|
| `Debug` | 本地开发调试，输出到 `bin\Debug\` |
| `CI` | CI 构建使用，输出到 `bin\CI\` |
| `Beta` | Beta 测试版本，启用优化 |
| `Release` | 正式发布版本，启用优化 |

### Secret 注入

部分功能（如微软登录、CurseForge API、遥测）需要配置 Secret：

- **Debug 构建**：直接读取环境变量（如 `PCL_MS_CLIENT_ID`）
- **Release 构建**：同样支持读取环境变量，或通过设置 `PCL_WRITE_SECRET` 环境变量在编译时注入

## 项目结构

```
PCL-CE-2.15.0/
├── Plain Craft Launcher 2/       # WPF 主程序（自定义许可证）
│   ├── Controls/                 # 自定义控件
│   ├── Modules/                  # 功能模块
│   │   ├── Base/                 # 基础模块（动画、日志、网络等）
│   │   ├── Minecraft/            # MC 相关（启动、下载、Java 等）
│   │   └── Resource/             # 资源下载
│   └── Pages/                    # 页面
│       ├── PageLaunch/           # 启动页（含登录）
│       ├── PageDownload/         # 下载页
│       └── PageSetup/            # 设置页
├── PCL.Core/                     # 核心库（Apache 2.0）
│   ├── App/                      # 应用核心（配置、IoC、本地化、生命周期）
│   ├── IO/                       # I/O 与网络
│   ├── Minecraft/                # MC 核心逻辑
│   ├── UI/                       # UI 相关（动画、控件、主题）
│   └── Utils/                    # 工具类
├── PCL.Core.SourceGenerators/    # 源代码生成器
└── PCL.Core.Test/                # 单元测试
```

## 参与贡献

欢迎为项目贡献代码！请阅读 [贡献指南](CONTRIBUTING.md) 了解提交流程、代码风格与提交规范。

- 提交信息遵循 [Angular 提交规范](https://github.com/angular/angular/blob/main/CONTRIBUTING.md#-commit-message-guidelines)
- PR 请指向 `main` 分支
- 提交前请确保本地编译通过

## 许可证

- [`Plain Craft Launcher 2/`](Plain%20Craft%20Launcher%202/LICENCE) 使用自定义许可证
- [其余所有目录](LICENSE) 使用 Apache License 2.0

## 统计

![Alt](https://repobeats.axiom.co/api/embed/e2ed0c3a10e2786a8285c60c67388e0c132c6fd4.svg "Repobeats analytics image")

[![Star History Chart](https://api.star-history.com/svg?repos=N1NA-MMK/Momoka-Craft-Launcher&type=Date)](https://www.star-history.com/#N1NA-MMK/Momoka-Craft-Launcher&Date)

## 贡献者

[![](https://contrib.rocks/image?repo=N1NA-MMK/Momoka-Craft-Launcher)](https://github.com/N1NA-MMK/Momoka-Craft-Launcher/graphs/contributors)

---

PCL-Momoka 是一个基于 PCL 社区版的二次开发版本，与原版 PCL 作者龙腾猫跃无直接隶属关系。

特别感谢：
- [龙腾猫跃](https://github.com/Meloong-Git) — 原版 PCL 的开发与开源
- [PCL 社区版](https://github.com/PCL-Community/PCL-CE) — 社区版的持续维护与改进
