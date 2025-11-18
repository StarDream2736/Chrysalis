# ModLinks仓库建立和提交规范

## 📋 目录
1. [仓库建立指南](#仓库建立指南)
2. [ModLinks.xml格式规范](#modlinksxml格式规范)
3. [ApiLinks.xml格式规范](#apilinksxml格式规范)
4. [Mod提交流程](#mod提交流程)
5. [审核标准](#审核标准)
6. [常见问题](#常见问题)

---

## 🏗️ 仓库建立指南

### 第一步：创建GitHub仓库

1. **创建新仓库**
   ```
   仓库名: silksong-modlinks
   描述: Mod repository for Hollow Knight: Silksong - Chrysalis Mod Manager
   可见性: Public
   初始化: 添加README和LICENSE (GPL-3.0)
   ```

2. **设置分支保护**
   ```
   Settings -> Branches -> Add rule
   Branch name pattern: main
   勾选: Require pull request reviews before merging
   勾选: Require status checks to pass before merging
   ```

3. **创建基础文件结构**
   ```
   silksong-modlinks/
   ├── ModLinks.xml          # Mod索引文件
   ├── ApiLinks.xml          # BepInEx/API索引
   ├── schemas/              # XML Schema定义
   │   ├── ModLinks.xsd
   │   └── ApiLinks.xsd
   ├── icons/                # Mod图标（可选）
   ├── .github/
   │   ├── ISSUE_TEMPLATE/
   │   │   └── mod-submission.md
   │   └── workflows/
   │       └── validate-xml.yml
   ├── README.md
   └── CONTRIBUTING.md
   ```

### 第二步：创建ModLinks.xml模板

```xml
<?xml version="1.0" encoding="utf-8"?>
<ModLinks xmlns="https://github.com/silksong-modding/modlinks">
  <!-- 
    空洞骑士：丝之歌 Mod索引
    维护者: [Your Name]
    最后更新: 2024-XX-XX
  -->
  
  <!-- 示例Mod - 请删除后使用 -->
  <Manifest>
    <Name>ExampleMod</Name>
    <Description>一个示例Mod</Description>
    <Version>1.0.0</Version>
    <Authors>
      <Author>AuthorName</Author>
    </Authors>
    <Links>
      <Windows SHA256="HASH_HERE">https://github.com/user/mod/releases/download/v1.0.0/ExampleMod-Windows.zip</Windows>
      <Mac SHA256="HASH_HERE">https://github.com/user/mod/releases/download/v1.0.0/ExampleMod-Mac.zip</Mac>
      <Linux SHA256="HASH_HERE">https://github.com/user/mod/releases/download/v1.0.0/ExampleMod-Linux.zip</Linux>
    </Links>
    <Dependencies>
      <!-- 如果需要其他Mod，在这里列出 -->
    </Dependencies>
    <Repository>https://github.com/user/mod</Repository>
    <Tags>
      <Tag>Gameplay</Tag>
    </Tags>
    <Integrations>
      <!-- 与哪些Mod有集成支持 -->
    </Integrations>
  </Manifest>
  
</ModLinks>
```

### 第三步：创建ApiLinks.xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<ApiLinks xmlns="https://github.com/silksong-modding/modlinks">
  <!-- BepInEx配置 -->
  <Manifest>
    <Version>5</Version>
    <Files>
      <File>BepInEx/core/BepInEx.dll</File>
      <File>BepInEx/core/0Harmony.dll</File>
      <File>BepInEx/core/BepInEx.Preloader.dll</File>
      <File>BepInEx/core/HarmonyXInterop.dll</File>
      <File>BepInEx/core/Mono.Cecil.dll</File>
      <File>BepInEx/core/MonoMod.RuntimeDetour.dll</File>
      <File>BepInEx/core/MonoMod.Utils.dll</File>
      <File>winhttp.dll</File>
      <File>doorstop_config.ini</File>
    </Files>
    <Links>
      <Windows SHA256="待填写">https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.2/BepInEx_win_x64_5.4.23.2.zip</Windows>
      <Mac SHA256="待填写">https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.2/BepInEx_macos_x64_5.4.23.2.zip</Mac>
      <Linux SHA256="待填写">https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.2/BepInEx_linux_x64_5.4.23.2.zip</Linux>
    </Links>
  </Manifest>
</ApiLinks>
```

### 第四步：设置自动化验证

创建 `.github/workflows/validate-xml.yml`:

```yaml
name: Validate XML

on:
  pull_request:
    paths:
      - '*.xml'
  push:
    branches:
      - main

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Validate ModLinks.xml
        run: |
          xmllint --noout --schema schemas/ModLinks.xsd ModLinks.xml
          
      - name: Validate ApiLinks.xml
        run: |
          xmllint --noout --schema schemas/ApiLinks.xsd ApiLinks.xml
          
      - name: Check SHA256 hashes
        run: |
          python scripts/validate_hashes.py
```

---

## 📝 ModLinks.xml格式规范

### 完整示例

```xml
<Manifest>
  <!-- 必填字段 -->
  <Name>YourModName</Name>
  <Description>Mod的简短描述（推荐100字符以内）</Description>
  <Version>1.2.3</Version>
  
  <!-- 下载链接 - 至少提供一个平台 -->
  <Links>
    <Windows SHA256="abc123...">https://download-url-windows.zip</Windows>
    <Mac SHA256="def456...">https://download-url-mac.zip</Mac>
    <Linux SHA256="ghi789...">https://download-url-linux.zip</Linux>
  </Links>
  
  <!-- 作者信息 - 至少一个 -->
  <Authors>
    <Author>PrimaryAuthor</Author>
    <Author>ContributorName</Author>
  </Authors>
  
  <!-- 仓库链接 - 必填 -->
  <Repository>https://github.com/yourname/yourmod</Repository>
  
  <!-- 依赖项 - 可选 -->
  <Dependencies>
    <Dependency>RequiredModName</Dependency>
    <Dependency>AnotherRequiredMod</Dependency>
  </Dependencies>
  
  <!-- 标签 - 至少一个 -->
  <Tags>
    <Tag>Gameplay</Tag>
    <Tag>Visual</Tag>
  </Tags>
  
  <!-- 集成信息 - 可选 -->
  <Integrations>
    <Integration>CompatibleModName</Integration>
  </Integrations>
</Manifest>
```

### 字段说明

#### Name（必填）
- **格式**: 字符串，无空格
- **规则**: 
  - 使用PascalCase命名
  - 仅包含字母、数字、下划线
  - 长度：3-50字符
- **示例**: `BetterUI`, `DebugTools`, `CustomSkins`

#### Version（必填）
- **格式**: 语义化版本 (SemVer)
- **规则**: `Major.Minor.Patch` 或 `Major.Minor.Patch.Build`
- **示例**: `1.0.0`, `2.5.3`, `1.0.0.42`

#### Description（必填）
- **格式**: 字符串
- **规则**: 
  - 简洁明了
  - 推荐100字符以内
  - 支持英文和中文
- **示例**: `"Adds debugging tools for mod developers"`

#### Links（必填）
- **格式**: XML元素，包含SHA256属性
- **规则**:
  - 至少提供一个平台的链接
  - SHA256必须是64字符的十六进制字符串
  - URL必须是直接下载链接（不能是GitHub release页面）
  - 如果所有平台使用相同文件，可以使用简化格式：
  
```xml
<!-- 简化格式（所有平台相同） -->
<Link SHA256="hash">https://universal-download-url.zip</Link>

<!-- 完整格式（平台特定） -->
<Links>
  <Windows SHA256="hash1">https://windows.zip</Windows>
  <Mac SHA256="hash2">https://mac.zip</Mac>
  <Linux SHA256="hash3">https://linux.zip</Linux>
</Links>
```

#### SHA256计算方法

**Windows PowerShell:**
```powershell
Get-FileHash .\YourMod.zip -Algorithm SHA256 | Select-Object -ExpandProperty Hash
```

**Linux/Mac:**
```bash
shasum -a 256 YourMod.zip
```

**在线工具:**
- https://emn178.github.io/online-tools/sha256_checksum.html

#### Authors（必填）
- **格式**: 作者名称列表
- **规则**: 至少一个作者
- **示例**:
```xml
<Authors>
  <Author>MainDeveloper</Author>
  <Author>Contributor1</Author>
</Authors>
```

#### Repository（必填）
- **格式**: Git仓库URL
- **规则**: 
  - 必须是公开的GitHub/GitLab/Gitee仓库
  - 包含Mod源码或发布说明
- **示例**: `https://github.com/username/modname`

#### Dependencies（可选）
- **格式**: 依赖的Mod名称列表
- **规则**: 
  - 依赖的Mod必须已存在于ModLinks.xml中
  - 名称必须完全匹配
- **示例**:
```xml
<Dependencies>
  <Dependency>ModLibrary</Dependency>
  <Dependency>CoreAPI</Dependency>
</Dependencies>
```

#### Tags（必填）
- **格式**: 标签列表
- **可用标签**:
  - `Gameplay` - 游戏玩法修改
  - `Visual` - 视觉/美化
  - `Audio` - 音频/音乐
  - `UI` - 用户界面
  - `Library` - 库/框架
  - `Utility` - 工具类
  - `Cosmetic` - 装饰性
  - `Boss` - Boss相关
  - `Expansion` - 内容扩展
  - `Difficulty` - 难度调整
  - `QoL` - 生活质量改进
  - `Randomizer` - 随机化器

- **规则**: 至少选择一个，最多5个
- **示例**:
```xml
<Tags>
  <Tag>Gameplay</Tag>
  <Tag>Boss</Tag>
</Tags>
```

#### Integrations（可选）
- **格式**: 集成Mod列表
- **说明**: 列出与此Mod有特殊集成的其他Mod
- **示例**:
```xml
<Integrations>
  <Integration>CustomKnight</Integration>
</Integrations>
```

---

## 🔧 ApiLinks.xml格式规范

```xml
<Manifest>
  <!-- BepInEx版本号 -->
  <Version>5</Version>
  
  <!-- 需要的文件列表 -->
  <Files>
    <File>BepInEx/core/BepInEx.dll</File>
    <File>winhttp.dll</File>
    <!-- 更多文件... -->
  </Files>
  
  <!-- 下载链接 -->
  <Links>
    <Windows SHA256="...">https://bepinex-windows.zip</Windows>
    <Mac SHA256="...">https://bepinex-mac.zip</Mac>
    <Linux SHA256="...">https://bepinex-linux.zip</Linux>
  </Links>
</Manifest>
```

---

## 📤 Mod提交流程

### 作为Mod开发者

#### 1. 准备工作

**a. 发布Mod**
- 在GitHub上创建Release
- 上传编译好的.zip或.dll文件
- 编写清晰的Release Notes

**b. 计算SHA256**
```bash
# 对每个平台的文件计算哈希
shasum -a 256 MyMod-Windows.zip
shasum -a 256 MyMod-Mac.zip
shasum -a 256 MyMod-Linux.zip
```

**c. 测试Mod**
- 确保Mod在所有声明的平台上可用
- 验证依赖关系正确
- 测试安装和卸载流程

#### 2. 创建Pull Request

**a. Fork仓库**
```bash
# Fork silksong-modlinks仓库到你的账户
# 然后克隆
git clone https://github.com/YOUR_USERNAME/silksong-modlinks.git
cd silksong-modlinks
```

**b. 创建分支**
```bash
git checkout -b add-your-mod-name
```

**c. 编辑ModLinks.xml**

在`<ModLinks>`标签内添加你的Mod条目：

```xml
<Manifest>
  <Name>YourModName</Name>
  <Description>你的Mod描述</Description>
  <Version>1.0.0</Version>
  <Authors>
    <Author>YourName</Author>
  </Authors>
  <Links>
    <Windows SHA256="计算出的哈希">https://github.com/you/mod/releases/download/v1.0.0/YourMod.zip</Windows>
  </Links>
  <Dependencies>
    <!-- 如果有依赖 -->
  </Dependencies>
  <Repository>https://github.com/you/mod</Repository>
  <Tags>
    <Tag>Gameplay</Tag>
  </Tags>
</Manifest>
```

**d. 验证XML格式**
```bash
# 使用xmllint验证（如果已安装）
xmllint --noout ModLinks.xml

# 或在线验证工具
# https://www.xmlvalidation.com/
```

**e. 提交更改**
```bash
git add ModLinks.xml
git commit -m "Add YourModName v1.0.0"
git push origin add-your-mod-name
```

**f. 创建Pull Request**

1. 访问你的Fork仓库
2. 点击"Pull Request"
3. 填写PR模板:

```markdown
## Mod信息

- **Mod名称**: YourModName
- **版本**: 1.0.0
- **仓库**: https://github.com/you/mod

## 变更说明

- [ ] 新增Mod
- [ ] 更新现有Mod
- [ ] 修复Bug

## 检查清单

- [ ] 已测试Mod在游戏中正常工作
- [ ] SHA256哈希已验证
- [ ] 下载链接可用
- [ ] XML格式正确
- [ ] 依赖关系准确
- [ ] 遵守提交规范

## 截图（可选）

[如果有UI变化，提供截图]

## 额外说明

[任何需要说明的内容]
```

#### 3. 等待审核

维护者会：
1. 验证XML格式
2. 检查SHA256哈希
3. 测试下载链接
4. 验证Mod在游戏中运行
5. 检查是否有恶意代码

通过后会合并你的PR。

### 更新现有Mod

```bash
git checkout -b update-your-mod-v1.1.0

# 修改ModLinks.xml中你的Mod条目
# 更新Version、Links、SHA256等

git add ModLinks.xml
git commit -m "Update YourModName to v1.1.0"
git push origin update-your-mod-v1.1.0

# 创建PR
```

---

## ✅ 审核标准

### 自动检查（CI）

1. **XML格式验证**
   - XML语法正确
   - 符合Schema定义

2. **SHA256验证**
   - 下载文件并计算哈希
   - 与声明的哈希对比

3. **链接可用性**
   - 所有下载链接返回200
   - 文件大小合理（<100MB）

### 人工审核

1. **代码安全**
   - 无恶意代码
   - 无未声明的网络请求
   - 无数据窃取行为

2. **功能性**
   - Mod描述准确
   - 依赖关系正确
   - 在游戏中可用

3. **规范性**
   - 遵守命名规范
   - 正确的版本号
   - 合适的标签

### 拒绝理由

❌ **会被拒绝的提交**:
- 包含恶意代码
- 破坏游戏本体
- 侵犯版权
- SHA256不匹配
- 链接失效
- XML格式错误
- 依赖不存在的Mod

---

## 🔍 常见问题

### Q: Mod必须开源吗？

A: 不强制，但强烈推荐。开源Mod更容易获得信任和通过审核。

### Q: 可以提交付费Mod吗？

A: 不可以。ModLinks仅接受免费Mod。

### Q: SHA256哈希不匹配怎么办？

A: 
1. 重新计算哈希
2. 确认下载的文件与上传的文件一致
3. 检查是否有缓存问题

### Q: 如何删除我的Mod？

A: 创建PR，从ModLinks.xml中移除对应条目。

### Q: 多久会审核我的PR？

A: 通常1-3个工作日。如果超过一周无响应，请在PR中评论提醒。

### Q: Mod名称可以包含空格吗？

A: 不可以。请使用PascalCase或使用下划线连接，如`CustomBoss`或`Custom_Boss`。

### Q: 如何处理Mod的本地化？

A: 在Description中可以包含多语言描述，用`|`分隔：
```xml
<Description>English description | 中文描述</Description>
```

### Q: 依赖项的顺序重要吗？

A: 是的。Chrysalis会按照列出的顺序安装依赖。核心依赖应放在前面。

### Q: 可以在PR中添加多个Mod吗？

A: 可以，但推荐每个PR只包含一个Mod，便于审核和回滚。

---

## 📞 联系方式

- **问题反馈**: 在仓库中创建Issue
- **讨论交流**: GitHub Discussions
- **紧急联系**: [维护者邮箱]

---

## 📜 更新历史

- **2024-XX-XX**: 初始版本创建

---

*本文档会持续更新，请关注最新版本。*
