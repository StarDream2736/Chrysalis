using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace Chrysalis.ViewModels;

public partial class ModPageViewModel : ViewModelBase
{
    public enum ModAction
    {
        Install,
        Update,
        Uninstall,
        Toggle
    }

    private static readonly Func<object, bool> ConstTrue = _ => true;
    public static ImmutableArray<Tag> Tags { get; } = Enum.GetValues<Tag>().ToImmutableArray();
    
    private readonly ReadOnlyObservableCollection<ModItem> _filteredItems;
    private readonly SortableObservableCollection<ModItem> _items;

    private readonly Dictionary<ModItem, (string? branch, string content)?> _readmes = new();
    private readonly HttpClient _hc;
    private readonly IModDatabase _db;
    private readonly IInstaller _installer;
    private readonly IModSource _mods;
    private readonly ISettings _settings;
    private readonly ReverseDependencySearch _reverseDependencySearch;
    
    public delegate void ExceptionHandler(ModAction act, Exception e, ModItem? mod);

    public delegate void CompletionHandler(ModAction act, ModItem mod);

    public event CompletionHandler? CompletedAction;
    public event ExceptionHandler? ExceptionRaised;
    
    public ReactiveCommand<Unit, Unit> UpdateAll { get; }
    public ReactiveCommand<Unit, Unit> ToggleApi { get; }
    public ReactiveCommand<Unit, Unit> ReinstallApi { get; }
    public ReactiveCommand<Unit, Unit> UpdateApi { get; }
    public ReactiveCommand<Unit, Unit> CheckBepInExUpdateCommand { get; }
    public ReactiveCommand<Unit, Unit> ReinstallBepInExCommand { get; }

    public ReactiveCommand<ModItem, Unit> OnUpdate    { get; }
    public ReactiveCommand<ModItem, Unit> OnInstall   { get; }
    public ReactiveCommand<ModItem, Unit> OnUninstall { get; }
    public ReactiveCommand<ModItem, Unit> OnEnable    { get; }

    [Notify("ProgressBarIndeterminate")] 
    private bool _pbIndeterminate;
    
    [Notify("Progress")] 
    private double _pbProgress;

    [Notify("ProgressBarVisible")] 
    private bool _pbVisible;

    [Notify] 
    private string? _search;

    [Notify]
    private ModItem? _selectedModItem;

    [Notify] 
    private Tag _selectedTag = Tag.All;
    
    [Notify] 
    private Func<ModItem, bool> _searchFilter = ConstTrue;

    [Notify] 
    private Func<ModItem, bool> _selectionFilter = ConstTrue;

    [Notify] 
    private Func<ModItem, bool> _tagFilter = ConstTrue;

    [Notify]
    private bool _updating;

    private readonly ILogger _logger;

    // BepInEx鐘舵€?
    public bool IsBepInExInstalled => GameConfig.IsBepInExInstalled(_settings.GameRootFolder);
    public string BepInExButtonText => IsBepInExInstalled ? "Uninstall BepInEx" : "Install BepInEx";
    
    [Notify]
    private string? _bepInExInstalledVersion;
    
    [Notify]
    private string? _bepInExLatestVersion;
    
    [Notify]
    private bool _bepInExHasUpdate;
    
    public string BepInExVersionInfo
    {
        get
        {
            if (!IsBepInExInstalled)
                return "Not Installed";
            if (_bepInExInstalledVersion == null)
                return "Installed";
            return _bepInExHasUpdate 
                ? $"v{_bepInExInstalledVersion} → v{_bepInExLatestVersion} Available" 
                : $"v{_bepInExInstalledVersion} (Latest)";
        }
    }
    
    // 鍏煎灞炴€э紙鐢ㄤ簬BepInEx锛?
    public ModState Api => IsBepInExInstalled ? new InstalledState(true, new Version(1, 0), true) : new NotInstalledState();
    public bool ApiOutOfDate => false; // BepInEx涓嶉渶瑕侀€氳繃绋嬪簭鏇存柊
    public bool CanUpdateAll => _items.Any(x => x.State is InstalledState { Updated: false }) && !_updating;
    public ReadOnlyObservableCollection<ModItem> FilteredItems => _filteredItems;

    public ModPageViewModel(ISettings settings, IModDatabase db, IInstaller inst, IModSource mods, ILogger logger, HttpClient hc)
    {
        _settings = settings;
        _installer = inst;
        _mods = mods;
        _db = db;
        _logger = logger;
        _hc = hc;

        _items = new SortableObservableCollection<ModItem>(db.Items.OrderBy(ModToOrderedTuple));

        // Create a source cache for dynamic filtering via IObservable
        var cache = new SourceCache<ModItem, string>(x => x.Name);
        cache.AddOrUpdate(_items);

        _filteredItems = new ReadOnlyObservableCollection<ModItem>(_items);

        cache.Connect()
             .Filter(this.WhenAnyValue(x => x.SelectionFilter))
             .Filter(this.WhenAnyValue(x => x.TagFilter))
             .Filter(this.WhenAnyValue(x => x.SearchFilter))
             .Sort(SortExpressionComparer<ModItem>.Ascending(x => ModToOrderedTuple(x)))
             .Bind(out _filteredItems)
             .Subscribe();

        _reverseDependencySearch = new ReverseDependencySearch(_items);

        ToggleApi = ReactiveCommand.CreateFromTask(ToggleApiCommand);
        UpdateApi = ReactiveCommand.CreateFromTask(UpdateApiAsync);
        ReinstallApi = ReactiveCommand.CreateFromTask(ReinstallApiAsync);
        UpdateAll = ReactiveCommand.CreateFromTask(UpdateAllAsync);
        CheckBepInExUpdateCommand = ReactiveCommand.CreateFromTask(async () => await CheckBepInExUpdateWithFeedbackAsync());
        ReinstallBepInExCommand = ReactiveCommand.CreateFromTask(ReinstallBepInExAsync);

        OnUpdate = ReactiveCommand.CreateFromTask<ModItem>(OnUpdateAsync);
        OnInstall = ReactiveCommand.CreateFromTask<ModItem>(OnInstallAsync);
        OnUninstall = ReactiveCommand.CreateFromTask<ModItem>(OnUninstallAsync);
        OnEnable = ReactiveCommand.CreateFromTask<ModItem>(OnEnableAsync);
        
        this.WhenAnyValue(x => x.Search)
            .Subscribe(
                s => SearchFilter = m =>
                    string.IsNullOrEmpty(s) || m.Name.Contains(s, StringComparison.OrdinalIgnoreCase)
            );

        this.WhenAnyValue(x => x.SelectedTag).Subscribe(t => { TagFilter = m => m.Tags.HasFlag(t); });
        
        // 启动时检测BepInEx安装状态
        CheckBepInExStatus();
        
        // 自动检查 BepInEx 更新
        _ = Task.Run(async () =>
        {
            await Task.Delay(2000); // 延迟2秒后检查，避免阻塞启动
            await CheckBepInExUpdateAsync();
        });
    }

    /// <summary>
    /// 检测并更新BepInEx安装状态
    /// </summary>
    private void CheckBepInExStatus()
    {
        RaisePropertyChanged(nameof(IsBepInExInstalled));
        RaisePropertyChanged(nameof(BepInExButtonText));
        RaisePropertyChanged(nameof(Api));
        RaisePropertyChanged(nameof(BepInExVersionInfo));
    }
    
    /// <summary>
    /// 检查 BepInEx 更新
    /// </summary>
    public async Task CheckBepInExUpdateAsync()
    {
        try
        {
            var (installed, latest, hasUpdate) = await _installer.CheckBepInExUpdate();
            
            BepInExInstalledVersion = installed?.ToString();
            BepInExLatestVersion = latest.ToString();
            BepInExHasUpdate = hasUpdate;
            
            RaisePropertyChanged(nameof(BepInExVersionInfo));
            
            _logger.LogInformation(
                "BepInEx update check completed: Installed={Installed}, Latest={Latest}, HasUpdate={HasUpdate}",
                BepInExInstalledVersion ?? "None",
                BepInExLatestVersion,
                BepInExHasUpdate);
            
            // 添加MessageBox反馈（只在手动触发时显示）
            // 自动检查时不显示弹窗
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check BepInEx update");
        }
    }
    
    /// <summary>
    /// 手动检查 BepInEx 更新（带反馈）
    /// </summary>
    public async Task CheckBepInExUpdateWithFeedbackAsync()
    {
        await CheckBepInExUpdateAsync();
        
        // 显示反馈
        if (!IsBepInExInstalled)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "BepInEx 未安装",
                "请先安装 BepInEx 才能检查更新。",
                ButtonEnum.Ok,
                Icon.Info
            ).ShowAsync();
        }
        else if (BepInExHasUpdate)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "发现新版本",
                $"BepInEx 有新版本可用！\n\n" +
                $"当前版本：v{BepInExInstalledVersion}\n" +
                $"最新版本：v{BepInExLatestVersion}\n\n" +
                $"请使用下拉菜单中的\"重新安装\"功能更新。",
                ButtonEnum.Ok,
                Icon.Warning
            ).ShowAsync();
        }
        else
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "已是最新版本",
                $"BepInEx 已是最新版本 v{BepInExInstalledVersion}！",
                ButtonEnum.Ok,
                Icon.Success
            ).ShowAsync();
        }
    }
    
    /// <summary>
    /// 重新安装 BepInEx
    /// </summary>
    private async Task ReinstallBepInExAsync()
    {
        if (!IsBepInExInstalled)
        {
            _logger.LogInformation("BepInEx 未安装，无法重新安装");
            await MessageBoxManager.GetMessageBoxStandard(
                "提示",
                "BepInEx 尚未安装，请先安装。",
                ButtonEnum.Ok,
                Icon.Info
            ).ShowAsync();
            return;
        }

        _logger.LogInformation("重新安装 BepInEx");
        
        try
        {
            var result = await MessageBoxManager.GetMessageBoxStandard(
                "确认重新安装",
                "确定要重新安装 BepInEx 吗？这将覆盖现有的 BepInEx 文件。",
                ButtonEnum.YesNo,
                Icon.Warning
            ).ShowAsync();

            if (result != ButtonResult.Yes)
                return;

            // 显示进度条
            ProgressBarVisible = true;
            ProgressBarIndeterminate = true;

            // 先卸载
            await _installer.UninstallBepInEx();
            
            // 再安装
            await _installer.InstallBepInEx(progress =>
            {
                ProgressBarIndeterminate = false;
                Progress = progress.Download?.PercentComplete ?? 0;
            });
            
            // 更新UI状态
            CheckBepInExStatus();
            await CheckBepInExUpdateAsync();
            
            ProgressBarVisible = false;
            
            await MessageBoxManager.GetMessageBoxStandard(
                "重新安装成功",
                "BepInEx 已成功重新安装！",
                ButtonEnum.Ok,
                Icon.Success
            ).ShowAsync();
        }
        catch (Exception e)
        {
            ProgressBarVisible = false;
            _logger.LogError(e, "Failed to reinstall BepInEx");
            
            await MessageBoxManager.GetMessageBoxStandard(
                "错误",
                $"重新安装 BepInEx 失败：{e.Message}",
                ButtonEnum.Ok,
                Icon.Error
            ).ShowAsync();
        }
    }

    private async Task ReinstallApiAsync()
    {
        if (!IsBepInExInstalled)
        {
            _logger.LogInformation("BepInEx未安装，无需卸载");
            return;
        }

        _logger.LogInformation("卸载BepInEx");
        
        try
        {
            var result = await MessageBoxManager.GetMessageBoxStandard(
                "确认卸载",
                "确定要卸载 BepInEx 吗？这将删除所有已安装的Mod。",
                ButtonEnum.YesNo,
                Icon.Warning
            ).ShowAsync();

            if (result != ButtonResult.Yes)
                return;

            await _installer.UninstallBepInEx();
            
            // 更新UI状态
            RaisePropertyChanged(nameof(IsBepInExInstalled));
            RaisePropertyChanged(nameof(BepInExButtonText));
            RaisePropertyChanged(nameof(Api));
            
            await MessageBoxManager.GetMessageBoxStandard(
                "卸载成功",
                "BepInEx 已成功卸载！",
                ButtonEnum.Ok,
                Icon.Success
            ).ShowAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to uninstall BepInEx");
            
            await MessageBoxManager.GetMessageBoxStandard(
                "卸载失败",
                $"BepInEx 卸载失败：{e.Message}",
                ButtonEnum.Ok,
                Icon.Error
            ).ShowAsync();
        }
    }

    private async Task ToggleApiCommand()
    {
        if (!IsBepInExInstalled)
        {
            // 安装BepInEx
            _logger.LogInformation("Installing BepInEx");
            try
            {
                await _installer.InstallBepInEx(progress => {
                    Dispatcher.UIThread.Invoke(() => {
                        ProgressBarVisible = !progress.Completed;
                        if (progress.Download?.PercentComplete is { } percent) {
                            ProgressBarIndeterminate = false;
                            Progress = percent;
                        } else {
                            ProgressBarIndeterminate = true;
                        }
                    });
                });
                
                // 更新UI状态
                CheckBepInExStatus();
                
                // 显示成功消息
                await MessageBoxManager.GetMessageBoxStandard(
                    "安装成功",
                    "BepInEx 已成功安装！",
                    ButtonEnum.Ok,
                    Icon.Success
                ).ShowAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to install BepInEx");
                ProgressBarVisible = false;
                
                // 显示错误消息
                await MessageBoxManager.GetMessageBoxStandard(
                    "安装失败",
                    $"BepInEx 安装失败：{e.Message}",
                    ButtonEnum.Ok,
                    Icon.Error
                ).ShowAsync();
            }
        }
        else
        {
            // 卸载BepInEx
            _logger.LogInformation("Uninstalling BepInEx");
            
            try
            {
                var result = await MessageBoxManager.GetMessageBoxStandard(
                    "确认卸载",
                    "确定要卸载 BepInEx 吗？这将删除所有已安装的Mod。",
                    ButtonEnum.YesNo,
                    Icon.Warning
                ).ShowAsync();

                if (result != ButtonResult.Yes)
                    return;

                await _installer.UninstallBepInEx();
                
                // 更新UI状态
                CheckBepInExStatus();
                
                await MessageBoxManager.GetMessageBoxStandard(
                    "卸载成功",
                    "BepInEx 已成功卸载！",
                    ButtonEnum.Ok,
                    Icon.Success
                ).ShowAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to uninstall BepInEx");
                
                await MessageBoxManager.GetMessageBoxStandard(
                    "卸载失败",
                    $"BepInEx 卸载失败：{e.Message}",
                    ButtonEnum.Ok,
                    Icon.Error
                ).ShowAsync();
            }
        }
    }

    public void OpenModsDirectory()
    {
        // 使用BepInEx的plugins文件夹作为Mods目录
        var modsFolder = _settings.ModsFolder;

        // Create the directory if it doesn't exist,
        // so we don't open a non-existent folder.
        Directory.CreateDirectory(modsFolder);

        Process.Start(new ProcessStartInfo(modsFolder)
        {
            UseShellExecute = true
        });
    }

    public void SelectAll()       => SelectionFilter = ConstTrue;
    public void SelectInstalled() => SelectionFilter = x => x.Installed;
    public void SelectUnupdated() => SelectionFilter = x => x.State is InstalledState { Updated: false };
    public void SelectEnabled()   => SelectionFilter = x => x.State is InstalledState { Enabled: true };

    public async Task UpdateAllAsync()
    {
        _logger.LogInformation("Updating all mods!");
        
        Updating = true;

        var outOfDate = _items.Where(x => x.State is InstalledState { Updated: false }).ToList();

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var mod in outOfDate)
        {
            // Mods can get updated as dependencies of others while doing this
            if (mod.State is not InstalledState { Updated: false })
                continue;

            await OnUpdateAsync(mod);
        }

        Updating = false;
    }
    
    [UsedImplicitly]
    private async Task OnEnableAsync(ModItem item)
    {
        try
        {
            if (!item.Enabled || await CheckDependents(item, onlyEnabled: true))
                await _installer.Toggle(item);

            // Reset the visuals of the toggle, as otherwise
            // it remains 'toggled', despite possibly being cancelled by
            // CheckDependents - leading to an incorrectly shown state
            item.CallOnPropertyChanged(nameof(item.Enabled));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when enabling {Mod}!", item.Name);
            ExceptionRaised?.Invoke(ModAction.Toggle, e, item);
        }
    }

    private async Task UpdateApiAsync()
    {
        try
        {
            await Task.Run(() => _installer.InstallApi());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when updating API!");
            ExceptionRaised?.Invoke(ModAction.Toggle, e, null);
        }

        RaisePropertyChanged(nameof(ApiOutOfDate));
    }

    private async Task InternalUpdateInstallAsync(ModAction type, ModItem item, Func<IInstaller, Action<ModProgressArgs>, Task> f)
    {
        _logger.LogInformation("Performing {Type} for {Mod}.", type, item);
        
        static bool IsHollowKnight(Process p)
        {
            return p.ProcessName.StartsWith("hollow_knight")
                   || p.ProcessName.StartsWith("Hollow Knight");
        }

        if (Process.GetProcesses().FirstOrDefault(IsHollowKnight) is { } proc)
        {
            var res = await MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ContentTitle = Resources.MLVM_InternalUpdateInstallAsync_Msgbox_W_Title,
                ContentMessage = Resources.MLVM_InternalUpdateInstallAsync_Msgbox_W_Text,
                ButtonDefinitions = ButtonEnum.YesNo,
                MinHeight = 200,
                SizeToContent = SizeToContent.WidthAndHeight
            }).ShowAsync();

            try
            {
                if (res == ButtonResult.Yes)
                    proc.Kill();
            }
            catch (Win32Exception)
            {
                // tragic, but oh well.
            }
        }

        try
        {
            await Task.Run(async () => await f
            (
                _installer,
                progress =>
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        ProgressBarVisible = !progress.Completed;

                        if (progress.Download?.PercentComplete is not { } percent)
                        {
                            ProgressBarIndeterminate = true;
                            return;
                        }

                        ProgressBarIndeterminate = false;
                        Progress = percent;
                    });
                }
            ));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when performing {Action} for {Mod}!", type, item.Name);

            ExceptionRaised?.Invoke(type, e, item);
            
            // Even if we threw, stop the progress bar.
            ProgressBarVisible = false;

            // Don't need to sort as we didn't successfully install anything
            // and we don't want to send the successfully completed action event.
            return;
        }

        ProgressBarVisible = false;

        static int Comparer(ModItem x, ModItem y)
        {
            return ModToOrderedTuple(x).CompareTo(ModToOrderedTuple(y));
        }

        CompletedAction?.Invoke(type, item);

        _items.SortBy(Comparer);
        
        _logger.LogInformation("Completed {Type} for {Mod}", type, item.Name);
    }

    [UsedImplicitly]
    private Task OnUpdateAsync(ModItem item) => InternalUpdateInstallAsync(ModAction.Update, item, item.OnUpdate);

    [UsedImplicitly]
    private Task OnInstallAsync(ModItem item) => InternalUpdateInstallAsync(ModAction.Install, item, item.OnInstall);

    private async Task<bool> CheckDependents(ModItem item, bool onlyEnabled = false)
    {
        var deps = onlyEnabled
            ? _reverseDependencySearch.GetAllEnabledDependents(item)
            : _reverseDependencySearch.GetDependents(item);

        if (deps.Count == 0)
            return true;

        return await DisplayHasDependentsWarning(item.Name, deps);
    }
    
    [UsedImplicitly]
    private async Task OnUninstallAsync(ModItem item)
    {
        if (!await CheckDependents(item))
            return;

        await InternalUpdateInstallAsync(ModAction.Uninstall, item, item.OnUninstall);
    }

    // asks user for confirmation on whether or not they want to uninstall/disable mod.
    // returns whether or not user presses yes on the message box
    private static async Task<bool> DisplayHasDependentsWarning(string modName, IEnumerable<ModItem> dependents)
    {
        var dependentsString = string.Join(", ", dependents.Select(x => x.Name));
        var result = await MessageBoxManager.GetMessageBoxStandard
        (
            "Warning! This mod is required for other mods to function!",
            $"{modName} is required for {dependentsString} to function properly. Do you still want to continue?",
            icon: Icon.Stop,
            @enum: ButtonEnum.YesNo
        ).ShowAsync();

        // Make sure we also don't return true if X was clicked instead of No
        return result.HasFlag(ButtonResult.Yes) && !result.HasFlag(ButtonResult.None);
    }

    private static (int priority, string name) ModToOrderedTuple(ModItem m)
    {
        return (
            m.State is InstalledState { Updated : false } ? -1 : 1,
            m.Name
        );
    }

    public async Task<(string? repo, string content)?> FetchReadme(ModItem item)
    {
        if (_readmes.TryGetValue(item, out var cached))
            return cached;
        
        return await Task.Run(async () =>
        {
            // If we couldn't get it from the raw, we fall back
            // to the actual gh API, as this handles more cases
            // (e.g. non-MD readmes, non-main/master branches, etc.)
            var res = await FetchReadmeRaw(item) ?? await FetchGithubReadme(item);

            if (res is not null)
                _readmes[item] = res;
            
            return res;
        });
    }

    private async Task<(string repo, string content)?> FetchReadmeRaw(ModItem item)
    {
        var raws = new[]
        {
            FetchReadmeRaw(item, branch: "master"),
            FetchReadmeRaw(item, branch: "main"),
        };

        var res = await Task.WhenAll(raws);

        return res.FirstOrDefault(x => x is { } readme && !readme.content.StartsWith("404"));
    }

    private async Task<(string repo, string content)?> FetchReadmeRaw(ModItem item, string branch)
    {
        var uri = new UriBuilder(item.Repository) {
            Host = "raw.githubusercontent.com"
        };

        uri.Path = $"{uri.Path.TrimEnd('/')}/{branch}/";

        var repo = uri.Uri;

        uri.Path += "README.md";
        
        var req = new HttpRequestMessage
        {
            RequestUri = uri.Uri,
            Method = HttpMethod.Get
        };

        var msg = await _hc.SendAsync(req);

        if (msg.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
            return null;
            
        return (repo.ToString(), await msg.Content.ReadAsStringAsync());
    }

    private async Task<(string repo, string content)?> FetchGithubReadme(ModItem item)
    {
        var repo = new UriBuilder(item.Repository) {
            Host = "api.github.com"
        };
        repo.Path = $"repos/{repo.Path.TrimEnd('/').TrimStart('/')}/readme";

        var req = new HttpRequestMessage()
        {
            RequestUri = repo.Uri,
            Method = HttpMethod.Get
        };

        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.json"));
        req.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

        var msg = await _hc.SendAsync(req);

        // Forbidden means we've hit the rate limit, but in that case the other requests
        // failed, so realistically it's just not there - so we return null.
        if (msg.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound)
            return null;

        if (await msg.Content.ReadFromJsonAsync<JsonElement?>() is not { } elem)
            return null;
        
        var download_url = elem.GetProperty("download_url").GetString() 
                           ?? throw new InvalidDataException("Response is missing download_url!");
        
        var base64 = elem.GetProperty("content").GetString() 
                     ?? throw new InvalidDataException("Response is missing content!");

        return (
            download_url[..(download_url.LastIndexOf('/') + 1)], 
            Encoding.UTF8.GetString(Convert.FromBase64String(base64))
        );
    }
}