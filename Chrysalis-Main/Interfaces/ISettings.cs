namespace Chrysalis.Interfaces;

public interface ISettings
{
    bool PlatformChanged { get; }
    
    bool AutoRemoveDeps { get; }
        
    string ManagedFolder { get; set; }
        
    string PreferredCulture { get; set; }
    
    Theme PreferredTheme { get; set; }
        
    /// <summary>
    /// 获取游戏根目录（从Managed文件夹向上两级）
    /// 例如: .../Hollow Knight Silksong_Data/Managed -> .../
    /// </summary>
    string GameRootFolder => Path.GetFullPath(Path.Combine(ManagedFolder, "..", ".."));
    
    /// <summary>
    /// BepInEx插件目录：游戏根目录/BepInEx/plugins
    /// </summary>
    string ModsFolder     => GameConfig.GetBepInExPluginsPath(GameRootFolder);
    
    /// <summary>
    /// 禁用的Mod存放在plugins/Disabled
    /// </summary>
    string DisabledFolder => Path.Combine(ModsFolder, "Disabled");

    void Save();

    void Apply();

    Link PlatformLink(Links link);
}