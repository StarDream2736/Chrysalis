namespace Chrysalis.Interfaces;

public interface IInstaller
{
    public enum ReinstallPolicy
    {
        ForceReinstall,
        SkipUpToDate
    }

    public Task Toggle(ModItem mod);

    public Task Install(ModItem mod, Action<ModProgressArgs> setProgress, bool enable);

    public Task Uninstall(ModItem mod);

    // BepInEx 安装相关
    public Task InstallBepInEx(Action<ModProgressArgs> setProgress);
    
    public Task UninstallBepInEx();
    
    public Task<(Version? installed, Version latest, bool hasUpdate)> CheckBepInExUpdate();
    
    // 保留旧API方法以兼容现有代码（已废弃，用于平滑过渡）
    [Obsolete("Silksong使用BepInEx，不再需要Modding API")]
    public Task InstallApi(ReinstallPolicy policy = ReinstallPolicy.SkipUpToDate);

    [Obsolete("Silksong使用BepInEx，不再需要Modding API")]
    public Task ToggleApi();
    
    public Task HandlePlatformChange();
}