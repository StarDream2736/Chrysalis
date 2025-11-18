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
    
    // Assembly名称
    public const string MainAssembly = "Assembly-CSharp.dll";
    public const string UnityEngineAssembly = "UnityEngine.dll";
    
    // ModLinks数据源（待建立）
    public const string ModLinksUri = "https://raw.githubusercontent.com/YOUR-ORG/silksong-modlinks/main/ModLinks.xml";
    public const string ApiLinksUri = "https://raw.githubusercontent.com/YOUR-ORG/silksong-modlinks/main/ApiLinks.xml";
    
    // 备用CDN
    public const string FallbackModLinksUri = "https://cdn.jsdelivr.net/gh/YOUR-ORG/silksong-modlinks@latest/ModLinks.xml";
    public const string FallbackApiLinksUri = "https://cdn.jsdelivr.net/gh/YOUR-ORG/silksong-modlinks@latest/ApiLinks.xml";
    
    // BepInEx下载地址（5.x最新版）
    public const string BepInExDownloadUrl = "https://github.com/BepInEx/BepInEx/releases/latest";
    
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
}
