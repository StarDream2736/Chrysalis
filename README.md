# Chrysalis - 空洞骑士：丝之歌 Mod管理器

![Version](https://img.shields.io/badge/version-1.0.0-red)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)
![Framework](https://img.shields.io/badge/.NET-8.0-purple)

> **注意**: 本项目基于 [Scarab](https://github.com/fifty-six/Scarab) 改造而成，专为《空洞骑士：丝之歌》设计。

## 📖 简介

**Chrysalis**（蛹）是一个现代化的Mod管理器，专为《空洞骑士：丝之歌》(Hollow Knight: Silksong) 设计。它简化了Mod的安装、更新和管理过程，让玩家可以轻松地扩展游戏体验。

### 主要特性

- ✅ **一键安装** - 自动处理BepInEx和Mod依赖
- ✅ **自动更新** - 检测并更新过期的Mod
- ✅ **依赖管理** - 智能处理Mod间的依赖关系
- ✅ **跨平台** - 支持Windows、Linux和macOS
- ✅ **多语言** - 支持中文、英文等多种语言
- ✅ **美观界面** - 基于Avalonia的现代UI设计

## 🚀 快速开始

### 系统要求

- .NET 8.0 Runtime
- 《空洞骑士：丝之歌》游戏本体
- 操作系统：Windows 10+, Linux (推荐Ubuntu 20.04+), macOS 11+

### 安装步骤

1. **下载Chrysalis**
   - 从 [Releases](../../releases/latest) 页面下载最新版本
   - 解压到任意目录

2. **首次运行**
   - 运行 `Chrysalis.exe` (Windows) 或 `Chrysalis` (Linux/Mac)
   - 应用会自动检测游戏安装路径
   - 如果检测失败，手动选择游戏目录

3. **安装BepInEx**
   - 首次使用时，Chrysalis会提示安装BepInEx
   - 点击"安装"自动完成配置
   - 启动游戏一次以生成BepInEx目录

4. **开始安装Mod**
   - 浏览Mod列表
   - 点击"安装"按钮
   - 自动下载并配置Mod及其依赖

## 📚 使用指南

### 游戏路径检测

Chrysalis会尝试以下路径自动检测游戏：

**Windows:**
```
C:\Program Files\Steam\steamapps\common\Hollow Knight Silksong
C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight Silksong
[其他盘符]\SteamLibrary\steamapps\common\Hollow Knight Silksong
```

**Linux:**
```
~/.local/share/Steam/steamapps/common/Hollow Knight Silksong
~/.steam/steam/steamapps/common/Hollow Knight Silksong
```

**macOS:**
```
~/Library/Application Support/Steam/steamapps/common/Hollow Knight Silksong
```

### BepInEx安装

丝之歌使用**BepInEx**作为Mod加载框架：

1. Mod存放位置：`[游戏目录]/BepInEx/plugins/`
2. 配置文件位置：`[游戏目录]/BepInEx/config/`
3. 日志文件：`[游戏目录]/BepInEx/LogOutput.log`

### Mod管理

#### 安装Mod
- 在列表中选择Mod
- 点击"安装"按钮
- 等待下载和配置完成

#### 更新Mod
- 有更新的Mod会显示红色标记
- 点击"全部更新"或单独更新

#### 启用/禁用Mod
- 使用复选框切换Mod启用状态
- 禁用的Mod不会被游戏加载

#### 卸载Mod
- 点击"卸载"按钮
- 确认删除Mod文件

## 🔧 高级功能

### 依赖管理

Chrysalis会自动：
- 安装Mod所需的依赖项
- 启用Mod时启用其依赖
- 卸载前警告反向依赖

### 配置文件

应用配置保存在：
```
Windows: %AppData%\SilksongModManager\ChrysalisSettings.json
Linux:   ~/.config/SilksongModManager/ChrysalisSettings.json
macOS:   ~/Library/Application Support/SilksongModManager/ChrysalisSettings.json
```

### 手动安装Mod

如果需要手动安装：
1. 下载Mod的.dll文件
2. 放入 `[游戏目录]/BepInEx/plugins/`
3. 重启Chrysalis刷新列表

## 🐛 故障排除

### BepInEx未正确加载

**症状**: 游戏启动后Mod不生效

**解决方案**:
1. 确认BepInEx目录存在
2. 运行游戏一次后退出
3. 检查是否生成了`BepInEx/config`和`BepInEx/plugins`文件夹

### Mod下载失败

**可能原因**:
- 网络连接问题
- ModLinks服务器暂时不可用

**解决方案**:
- 检查网络连接
- 稍后重试
- 查看应用日志：`%AppData%\SilksongModManager\Chrysalis-[日期].log`

### 游戏路径检测失败

**解决方案**:
1. 点击设置
2. 选择"更改游戏路径"
3. 手动选择游戏的可执行文件

## 💡 常见问题

**Q: Chrysalis和Scarab有什么区别？**
A: Chrysalis是专为丝之歌设计的，使用BepInEx而非传统的ModdingAPI，适配丝之歌的目录结构。

**Q: 可以同时使用Scarab和Chrysalis吗？**
A: 可以，它们管理不同的游戏，配置文件和Mod目录互不干扰。

**Q: Mod安全吗？**
A: 所有Mod都经过SHA256哈希验证，确保文件完整性。但请只从官方ModLinks安装Mod。

**Q: 如何提交我的Mod到ModLinks？**
A: 请参阅 [ModLinks提交指南](ModLinks仓库建立和提交规范.md)

## 🤝 贡献

欢迎贡献代码、报告Bug或提出建议！

### 开发环境

- Visual Studio 2022 / JetBrains Rider
- .NET 8.0 SDK
- Git

### 构建项目

```bash
git clone [your-repo-url]
cd Chrysalis
dotnet restore
dotnet build
```

### 运行测试

```bash
dotnet test
```

## 📄 许可证

本项目基于 [GPL-3.0 License](LICENSE) 开源。

原项目Scarab由 [fifty-six](https://github.com/fifty-six) 开发。

## 🙏 致谢

- **Scarab** - 原项目，提供了优秀的架构基础
- **BepInEx团队** - 提供强大的Mod加载框架
- **空洞骑士Modding社区** - 提供宝贵经验
- **Team Cherry** - 创作了精彩的游戏

## 📞 联系方式

- 问题反馈：[GitHub Issues](../../issues)
- 讨论交流：[GitHub Discussions](../../discussions)

---

**⚠️ 免责声明**

本软件仅供学习交流使用。使用Mod可能影响游戏体验，请自行承担风险。
我们不对因使用本软件或Mod造成的任何问题负责。

---

*Made with ❤️ for the Hollow Knight: Silksong community*
