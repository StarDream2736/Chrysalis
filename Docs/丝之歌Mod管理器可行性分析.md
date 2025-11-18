# 空洞骑士：丝之歌 Mod管理器 - 可行性分析文档

## 📋 项目概述

基于现有的 **Scarab**（空洞骑士Mod管理器）代码库，开发一个全新的 **空洞骑士：丝之歌** Mod管理器。

### 基本信息
- **源项目**: Scarab v2.5.0.0
- **目标游戏**: Hollow Knight: Silksong（空洞骑士：丝之歌）
- **开发方式**: Fork + 重构
- **技术栈**: 继承（C# + .NET 8.0 + Avalonia）

---

## ✅ 可行性评估

### 1. **技术可行性：⭐⭐⭐⭐⭐（非常高）**

#### 优势：
✅ **完善的架构基础**
- MVVM架构清晰，易于修改
- 接口驱动设计，扩展性强
- 依赖注入容器，模块解耦

✅ **核心功能可复用**
- 文件下载与验证（SHA256）
- 压缩文件处理（.zip/.dll）
- 依赖关系管理
- 版本比较逻辑
- UI组件库（Avalonia）

✅ **跨平台特性**
- 已支持Windows/Linux/macOS
- 平台检测机制成熟
- 路径处理健壮

#### 挑战：
⚠️ **游戏差异**
- 丝之歌的游戏引擎版本可能不同
- Assembly名称可能改变
- 目录结构可能调整

⚠️ **Modding API**
- 丝之歌需要全新的Modding API
- API可能尚未发布
- Mod格式规范未确定

---

### 2. **生态可行性：⭐⭐⭐（中等）**

#### 当前状态：
🔴 **游戏未发布**
- 丝之歌仍在开发中（2024年预计发布）
- Modding社区尚未形成
- 官方Mod支持政策未明确

🟡 **需等待的资源**
- 游戏发布后的文件结构
- 社区开发的Modding API
- ModLinks.xml等mod索引服务

🟢 **可预先准备**
- 管理器框架开发
- UI/UX设计
- 基础架构搭建

---

### 3. **开发工作量评估：⭐⭐⭐⭐（较大）**

#### 预计工作量分布：

| 任务类型 | 工作量 | 优先级 | 说明 |
|---------|-------|--------|------|
| 代码重命名/品牌化 | 🔹 小 | 高 | 更改名称、图标、文案 |
| 路径检测逻辑 | 🔹🔹 中 | 高 | 适配丝之歌目录结构 |
| API安装逻辑 | 🔹🔹🔹 大 | 高 | 适配新的Modding API |
| ModLinks数据源 | 🔹🔹 中 | 高 | 建立新的mod索引 |
| UI调整 | 🔹 小 | 中 | 主题色、logo等 |
| 测试与调试 | 🔹🔹🔹 大 | 高 | 等游戏发布后进行 |

---

## 🔧 主要修改点分析

### 第一阶段：品牌化修改（立即可做）

#### 1.1 项目重命名
```
Scarab → Silksong Mod Manager (暂定名：Chrysalis)
```

**需要修改的文件：**
```
- 项目名称: Scarab.csproj → Chrysalis.csproj
- 命名空间: namespace Scarab → namespace Chrysalis
- 程序集名称: AssemblyInfo.cs
- 解决方案: Scarab.sln → Chrysalis.sln
```

**修改范围：**
- ✏️ ~100个文件的命名空间
- ✏️ 所有XAML文件的引用
- ✏️ 项目配置文件

#### 1.2 视觉元素更新

**图标资源：**
```diff
- Assets/omegamaggotprime.ico (甲虫主题)
+ Assets/chrysalis.ico (茧/蝶主题)
```

**配色方案：**
```diff
- 橙色/红色 (空洞骑士)
+ 粉色/紫色 (丝之歌，基于Hornet配色)
```

**文案更新：**
```
- "Hollow Knight" → "Hollow Knight: Silksong"
- "Modding API" → "Silksong Modding API"
- README.md 完全重写
```

#### 1.3 配置文件路径
```diff
Settings.cs:
- %AppData%/HKModInstaller/
+ %AppData%/SilksongModManager/

- HKInstallerSettings.json
+ SilksongManagerSettings.json
```

---

### 第二阶段：核心逻辑适配（需游戏发布后）

#### 2.1 游戏路径检测 (PathUtil.cs)

**当前检测路径：**
```csharp
"Steam/steamapps/common/Hollow Knight"
"GOG Galaxy/Games/Hollow Knight"
```

**需修改为：**
```csharp
"Steam/steamapps/common/Hollow Knight Silksong"
"GOG Galaxy/Games/Hollow Knight Silksong"
// 可能的Epic Games Store路径
"Epic Games/Hollow Knight Silksong"
```

**目录后缀：**
```diff
当前：
- "Hollow Knight_Data/Managed"
- "hollow_knight_Data/Managed"

预计：
- "Hollow Knight Silksong_Data/Managed"  (GOG)
- "silksong_Data/Managed"                 (Steam，待确认）
```

**验证文件：**
```diff
- Assembly-CSharp.dll (空洞骑士)
+ Assembly-CSharp.dll (丝之歌，名称可能相同但内容不同)
```

#### 2.2 注册表检测 (Settings.cs)

**Windows注册表路径：**
```diff
GOG:
- HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\GOG.com\Games\1308320804
+ HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\GOG.com\Games\[新的游戏ID]
```

**需要的信息：**
- ❓ 丝之歌的GOG游戏ID
- ❓ Steam AppID（用于其他场景）

#### 2.3 API安装逻辑 (Installer.cs)

**关键差异点：**

| 项目 | 空洞骑士 | 丝之歌（预测） |
|------|---------|---------------|
| Assembly名称 | Assembly-CSharp.dll | 可能相同 |
| 备份文件命名 | .dll.v / .dll.m | 可继续使用 |
| API包格式 | .zip | 可能相同 |
| 游戏引擎 | Unity 2020.x | Unity 20xx.x（未知） |

**需要适配：**
```csharp
// Installer.cs - 可能需要修改的常量
internal const string Modded = "Assembly-CSharp.dll.m";
internal const string Vanilla = "Assembly-CSharp.dll.v";
internal const string Current = "Assembly-CSharp.dll";

// 如果丝之歌使用不同的Assembly名称
// 需要修改为新的名称
```

#### 2.4 ModLinks数据源 (ModDatabase.cs)

**当前数据源：**
```
https://raw.githubusercontent.com/hk-modding/modlinks/main/ModLinks.xml
https://raw.githubusercontent.com/hk-modding/modlinks/main/ApiLinks.xml
```

**需要建立新的数据源：**
```
选项1: 自建仓库
https://raw.githubusercontent.com/silksong-modding/modlinks/main/ModLinks.xml

选项2: Fork并修改
Fork hk-modding/modlinks → silksong-modlinks

选项3: 使用相同仓库不同分支
https://raw.githubusercontent.com/hk-modding/modlinks/silksong/ModLinks.xml
```

**ModLinks.xml 结构（无需改动）：**
```xml
<ModLinks>
  <Manifest>
    <Name>SomeMod</Name>
    <Version>1.0.0</Version>
    <Links>
      <Windows SHA256="...">https://...</Windows>
      <Mac SHA256="...">https://...</Mac>
      <Linux SHA256="...">https://...</Linux>
    </Links>
    <Dependencies>
      <Dependency>OtherMod</Dependency>
    </Dependencies>
    <Description>...</Description>
  </Manifest>
</ModLinks>
```

---

### 第三阶段：新功能开发（可选）

#### 3.1 UI/UX改进

**建议的新功能：**
1. ✨ **Mod预览图片**
   - 在Mod列表显示缩略图
   - ModLinks.xml添加`<ThumbnailUrl>`字段

2. ✨ **Mod分类浏览**
   - 增强Tag系统
   - 添加分类页面

3. ✨ **Mod评分系统**
   - 集成社区评分
   - 显示下载量

4. ✨ **游戏启动器集成**
   - 直接从管理器启动游戏
   - 检测游戏运行状态

#### 3.2 技术改进

**性能优化：**
```
- 增量更新ModLinks（仅下载差异）
- 本地缓存机制优化
- 并行下载多个Mod
```

**安全增强：**
```
- 代码签名验证
- Mod沙箱隔离（如果API支持）
- 用户权限管理
```

---

## 📊 开发路线图

### Phase 0: 准备阶段（当前）
- [x] 分析Scarab源码
- [ ] 设计新的品牌视觉
- [ ] 准备开发环境
- [ ] Fork代码仓库

### Phase 1: 基础改造（游戏发布前）
**时间：2-3周**
```
Week 1:
- [x] 项目重命名
- [ ] 命名空间批量替换
- [ ] 资源文件更新（图标、配色）
- [ ] 文案本地化

Week 2-3:
- [ ] 预设游戏路径（基于推测）
- [ ] ModLinks数据源准备
- [ ] UI主题调整
- [ ] 建立CI/CD流程
```

### Phase 2: 核心适配（游戏发布后）
**时间：1-2周**
```
Day 1-3: 紧急适配
- [ ] 确认游戏目录结构
- [ ] 更新路径检测逻辑
- [ ] 测试基础功能

Day 4-7: API集成
- [ ] 等待社区Modding API发布
- [ ] 适配API安装逻辑
- [ ] 测试API切换

Day 8-14: 完善与测试
- [ ] 修复发现的Bug
- [ ] 性能优化
- [ ] 准备发布
```

### Phase 3: 功能扩展（长期）
**时间：持续迭代**
```
- [ ] 根据社区反馈添加功能
- [ ] 支持更多Mod格式
- [ ] 与丝之歌Modding社区合作
- [ ] 持续维护和更新
```

---

## ⚠️ 风险与挑战

### 高风险项：

1. **🔴 游戏Modding API延迟**
   - **风险**: 游戏发布后API可能需要数月才能稳定
   - **缓解**: 提前与Modding社区沟通，参与API开发

2. **🔴 游戏架构大幅改变**
   - **风险**: Unity版本升级导致Mod机制完全不同
   - **缓解**: 设计灵活的抽象层，易于适配

3. **🟡 社区接受度**
   - **风险**: 社区可能选择其他Mod管理器
   - **缓解**: 尽早发布，建立品牌，提供优质体验

4. **🟡 维护成本**
   - **风险**: 需要长期维护两个项目（空洞骑士+丝之歌）
   - **缓解**: 尽可能共享代码库，使用Git子模块

---

## 🛠️ 需要的关键信息

### 在开始修改前，我需要以下信息：

#### 1. **项目决策信息**
```
❓ 新项目的正式名称是什么？
   建议: Chrysalis (蛹/茧，与Silksong主题契合)

❓ 是Fork独立项目还是在原项目基础上切换？
   建议: 本地开发，暂不上云

❓ 是否需要同时支持空洞骑士和丝之歌？
   建议: 分别维护，避免复杂性
```

#### 2. **品牌与视觉信息**
```
❓ 应用图标设计方向？
   需要: 高分辨率图标文件 (.ico, .png)

❓ 主题配色方案？
   建议: 暗红色色系 (#E91E63,rgb(110, 21, 3))

❓ 应用名称本地化？
   需要: 你先翻译简中和英文
```

#### 3. **游戏相关信息（游戏发布后）**
```
❓ 丝之歌的默认安装路径？
   F:\SteamLibrary\steamapps\common\Hollow Knight Silksong

❓ 游戏的可执行文件名称？
   例: silksong.exe, Silksong.app

❓ Unity Data目录名称？
   例: silksong_Data, Hollow Knight Silksong_Data

❓ 游戏的Steam AppID？
   https://store.steampowered.com/app/1030300/Hollow_Knight_Silksong/

❓ 游戏的GOG ID？
   不需要
```

#### 4. **Modding生态信息（API发布后）**
```
❓ Modding API的官方仓库？
   例: https://github.com/BepInEx/BepInEx

❓ ModLinks.xml维护者？
   自建

❓ Mod开发规范文档？
   了解Mod格式和加载机制

❓ API版本号机制？
   向后兼容性策略
```

#### 5. **技术规格信息**
```
❓ 丝之歌使用的Unity版本？
   影响Assembly处理方式

❓ Assembly-CSharp.dll的实际名称？
   可能与空洞骑士相同或不同

❓ Mod加载机制差异？
   BepInEx

❓ 是否支持热重载？
   否
```
模组的安装规范
在你的Steam界面中，右键点击《空洞骑士：丝绸之歌》游戏链接=>管理=>浏览本地文件。
从本模组页面的“文件”部分下载.zip文件并打开。
把打开的档案里的所有文件拖到你在步骤#1打开的“空洞骑士丝绸之歌”文件夹里。
启动游戏后退出。
在你的“空洞骑士 Silksong”浏览器窗口中，打开“BepInEx”文件夹。如果里面有两个新文件夹（插件和配置），那么 BepInEx 已经正确加载，可以安装插件。如果没有，我建议你查看我上面链接的GitHub页面。
现在你可以下载并使用插件，方法是将它们解压到BepInEx/plugins文件夹中。
---

## 💡 立即可执行的准备工作

即使没有上述所有信息，我们现在也可以开始以下工作：

### 1. **代码重构准备**
```bash
# 创建Fork
git clone https://github.com/fifty-six/Scarab.git Chrysalis
cd Chrysalis
git remote rename origin upstream
git remote add origin <your-new-repo>

# 创建开发分支
git checkout -b feature/silksong-adaptation
```

### 2. **项目重命名脚本**
```powershell
# 批量重命名命名空间
Get-ChildItem -Recurse -Include *.cs | 
ForEach-Object {
    (Get-Content $_.FullName) -replace 'namespace Scarab', 'namespace Chrysalis' |
    Set-Content $_.FullName
}
```

### 3. **配置文件模板准备**
创建适配层，方便后续修改：
```csharp
// GameConfig.cs (新文件)
public static class GameConfig 
{
    public const string GameName = "Hollow Knight: Silksong";
    public const string ExecutableName = "silksong.exe"; // 待确认
    public const string DataFolderName = "silksong_Data"; // 待确认
    public const string ManagedFolder = "Managed";
    
    public static readonly string[] SearchPaths = {
        "Steam/steamapps/common/Hollow Knight Silksong",
        "GOG Galaxy/Games/Hollow Knight Silksong",
        // 更多路径待补充
    };
}
```

### 4. **UI资源占位符**
```
Assets/
  ├── chrysalis-icon-temp.ico (临时图标)
  ├── placeholder-banner.png (占位横幅)
  └── color-scheme.md (配色方案文档)
```

### 5. **文档准备**
```
docs/
  ├── CONTRIBUTING.md (贡献指南)
  ├── API_ADAPTATION.md (API适配文档)
  ├── PATH_DETECTION.md (路径检测说明)
  └── ROADMAP.md (开发路线图)
```

---

## 📈 成功指标

### Alpha版本（游戏发布时）
- ✅ 能正确检测丝之歌安装路径
- ✅ 能成功安装/卸载Modding API
- ✅ 基础UI可用

### Beta版本（API稳定后）
- ✅ 支持10+个测试Mod
- ✅ 依赖管理正常工作
- ✅ 无严重Bug

### 1.0版本（社区推广）
- ✅ 支持50+ Mod
- ✅ 社区ModLinks.xml稳定
- ✅ 多语言支持
- ✅ 跨平台测试通过
- ✅ 完善的文档

---

## 🎯 总结与建议

### 可行性结论：**高度可行✅**

**理由：**
1. ✅ 技术栈成熟，代码质量高
2. ✅ 架构设计优秀，易于改造
3. ✅ 核心逻辑可复用90%+
4. ✅ 社区需求明确（空洞骑士Mod非常火）

### 最大挑战：**等待游戏发布⏳**

**建议策略：**
1. **现在**: 完成品牌化和基础重构
2. **游戏发布日**: 紧急适配路径检测
3. **API发布**: 快速集成并发布Alpha版

### 推荐的下一步行动：

#### 立即执行（1-2天）：
```
1. 确定项目正式名称
2. 设计logo和配色
3. Fork代码仓库
4. 重命名项目和命名空间
```

#### 短期准备（1-2周）：
```
5. 重构UI资源
6. 建立开发文档
7. 准备ModLinks仓库
8. 联系丝之歌Modding社区
```

#### 等待游戏发布：
```
9. 监控游戏发布信息
10. 第一时间获取游戏文件
11. 快速适配并发布
```

---

## 📞 需要明确的问题清单

请提供以下信息以便我开始修改：

### 必需信息（现在）：
- [ ] **项目名称**: Chrysalis 还是其他？
- [ ] **仓库位置**: GitHub用户名/组织名？
- [ ] **开发模式**: Fork独立 or 原地改造？
- [ ] **图标设计**: 有现成的设计稿吗？

### 重要信息（游戏发布前准备）：
- [ ] **配色方案**: 使用推荐的粉紫色系吗？
- [ ] **支持语言**: 仅中英文还是更多？
- [ ] **发布平台**: GitHub Releases? Steam Workshop?
- [ ] **社区联系**: 是否已联系丝之歌Modding团队？

### 关键信息（游戏发布后）：
- [ ] **游戏路径**: 实际的安装目录结构
- [ ] **API信息**: Modding API的仓库和文档
- [ ] **测试环境**: 是否有游戏副本用于测试？

---

## 🚀 准备就绪后的启动流程

一旦你提供了上述必需信息，我将：

1. **第1小时**: 
   - 批量重命名所有文件和命名空间
   - 更新项目配置文件

2. **第1天**:
   - 替换UI资源
   - 修改所有文案
   - 更新README

3. **第1周**:
   - 适配路径检测逻辑（预测版）
   - 准备ModLinks基础设施
   - 建立CI/CD

4. **游戏发布后24小时内**:
   - 紧急适配实际路径
   - 发布Alpha测试版

---

*文档创建日期: 2025年11月18日*  
*基于Scarab v2.5.0.0架构分析*  
*等待《空洞骑士：丝之歌》正式发布*

---

## 📚 参考资料

- [Scarab源码](https://github.com/fifty-six/Scarab)
- [空洞骑士Modding社区](https://github.com/hk-modding)
- [Hollow Knight: Silksong官网](https://www.hollowknightsilksong.com/)
- [Avalonia文档](https://docs.avaloniaui.net/)
- [Unity Modding最佳实践](https://docs.bepinex.dev/)
