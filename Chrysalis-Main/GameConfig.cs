namespace Chrysalis;

/// <summary>
/// 空洞骑士：丝之歌 游戏配置
/// </summary>
public static class GameConfig
{
    // 游戏基本信息
    public const string GameName = "Hollow Knight: Silksong";
    public const string GameNameChinese = "空洞骑士：丝之歌";
    
    // Steam信息
    public const string SteamAppId = "1030300";
    public const string SteamGameFolder = "Hollow Knight Silksong";
    
    // 可执行文件名称（根据实际游戏发布后确认）
    public const string WindowsExe = "Hollow Knight Silksong.exe";
    public const string MacApp = "Hollow Knight Silksong.app";
    public const string LinuxExe = "Hollow Knight Silksong.x86_64";
    
    // Unity Data目录名称（待游戏发布后确认）
    public const string DataFolderName = "Hollow Knight Silksong_Data";
    public const string ManagedFolderName = "Managed";
    
    // BepInEx相关
    public const string BepInExFolder = "BepInEx";
    public const string BepInExPluginsFolder = "plugins";
    public const string BepInExConfigFolder = "config";
    public const string BepInExCoreFolder = "core";
    
    // BepInEx版本配置
    public const string BepInExVersion = "5.4.23.2";
    public const string BepInExBaseUrl = "https://github.com/BepInEx/BepInEx/releases/download";
    
    // Assembly名称
    public const string MainAssembly = "Assembly-CSharp.dll";
    public const string UnityEngineAssembly = "UnityEngine.dll";
    
    // ModLinks数据源
    public const string ModLinksUri = "https://raw.githubusercontent.com/StarDream2736/silksong-modlinks/main/ModLinks.xml";
    public const string ApiLinksUri = "https://raw.githubusercontent.com/StarDream2736/silksong-modlinks/main/ApiLinks.xml";
    
    // 备用CDN
    public const string FallbackModLinksUri = "https://cdn.jsdelivr.net/gh/StarDream2736/silksong-modlinks@latest/ModLinks.xml";
    public const string FallbackApiLinksUri = "https://cdn.jsdelivr.net/gh/StarDream2736/silksong-modlinks@latest/ApiLinks.xml";
    
    /// <summary>
    /// 获取BepInEx下载地址
    /// </summary>
    public static string GetBepInExDownloadUrl(string platform)
    {
        string fileName = platform.ToLowerInvariant() switch
        {
            "windows" => $"BepInEx_win_x64_{BepInExVersion}.zip",
            "linux" => $"BepInEx_linux_x64_{BepInExVersion}.zip",
            "macos" => $"BepInEx_macos_x64_{BepInExVersion}.zip",
            _ => throw new PlatformNotSupportedException($"Platform '{platform}' is not supported")
        };
        return $"{BepInExBaseUrl}/v{BepInExVersion}/{fileName}";
    }
    
    /// <summary>
    /// 获取BepInEx文件名
    /// </summary>
    public static string GetBepInExFileName(string platform)
    {
        return platform.ToLowerInvariant() switch
        {
            "windows" => $"BepInEx_win_x64_{BepInExVersion}.zip",
            "linux" => $"BepInEx_linux_x64_{BepInExVersion}.zip",
            "macos" => $"BepInEx_macos_x64_{BepInExVersion}.zip",
            _ => throw new PlatformNotSupportedException($"Platform '{platform}' is not supported")
        };
    }
    
    /// <summary>
    /// 获取可能的游戏安装路径后缀
    /// </summary>
    public static string[] GetPathSuffixes()
    {
        return new[]
        {
            // Steam/GOG - Unity Data目录
            Path.Combine(DataFolderName, ManagedFolderName),
            // BepInEx路径（如果已安装BepInEx）
            Path.Combine(BepInExFolder, BepInExCoreFolder),
            // macOS
            "Contents/Resources/Data/Managed"
        };
    }
    
    /// <summary>
    /// 获取BepInEx插件目录的完整路径
    /// </summary>
    public static string GetBepInExPluginsPath(string gameRoot)
    {
        return Path.Combine(gameRoot, BepInExFolder, BepInExPluginsFolder);
    }
    
    /// <summary>
    /// 获取BepInEx配置目录的完整路径
    /// </summary>
    public static string GetBepInExConfigPath(string gameRoot)
    {
        return Path.Combine(gameRoot, BepInExFolder, BepInExConfigFolder);
    }
    
    /// <summary>
    /// 检查BepInEx是否已安装
    /// </summary>
    public static bool IsBepInExInstalled(string gameRoot)
    {
        var bepInExPath = Path.Combine(gameRoot, BepInExFolder);
        var corePath = Path.Combine(bepInExPath, BepInExCoreFolder);
        var pluginsPath = Path.Combine(bepInExPath, BepInExPluginsFolder);
        
        // BepInEx 5.x: 只需检查BepInEx文件夹和core文件夹存在即可
        // plugins文件夹会在安装后手动创建
        return Directory.Exists(bepInExPath) 
               && (Directory.Exists(corePath) || File.Exists(Path.Combine(gameRoot, "winhttp.dll")));
    }
    
    /// <summary>
    /// 获取已安装的 BepInEx 版本
    /// </summary>
    public static Version? GetInstalledBepInExVersion(string gameRoot)
    {
        if (!IsBepInExInstalled(gameRoot))
            return null;
            
        // 尝试从 changelog.txt 读取版本信息
        var changelogPath = Path.Combine(gameRoot, BepInExFolder, "changelog.txt");
        if (File.Exists(changelogPath))
        {
            try
            {
                var firstLine = File.ReadLines(changelogPath).FirstOrDefault();
                if (firstLine != null)
                {
                    // changelog 第一行通常是 "## vX.X.X.X"
                    var match = System.Text.RegularExpressions.Regex.Match(firstLine, @"v?(\d+\.\d+\.\d+\.\d+)");
                    if (match.Success && Version.TryParse(match.Groups[1].Value, out var version))
                        return version;
                }
            }
            catch { /* 忽略解析错误 */ }
        }
        
        // 如果无法读取版本，返回当前配置的版本（假设是最新的）
        return Version.TryParse(BepInExVersion, out var configVersion) ? configVersion : null;
    }
}
