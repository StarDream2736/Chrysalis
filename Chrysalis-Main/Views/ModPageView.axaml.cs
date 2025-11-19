using System.Diagnostics;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media;
// Resources is a field in Avalonia UserControls, so alias it for brevity
using Localization = Chrysalis.Resources;

namespace Chrysalis.Views;

[UsedImplicitly]
public partial class ModPageView : ReactiveUserControl<ModPageViewModel>
{
    private WindowNotificationManager? _notify;

    public ModPageView()
    {
        InitializeComponent();

        UserControl.KeyDown += OnKeyDown;

        this.WhenActivatedVM((vm, d) =>
        {
            vm.CompletedAction += OnComplete;
            vm.ExceptionRaised += OnError;

            this.WhenAnyValue(x => x.TagBox.SelectionBoxItem)
                .Subscribe(x => vm.SelectedTag = (Tag) (x ?? Models.Tag.All))
                .DisposeWith(d);
        });
    }

    private void OnError(ModPageViewModel.ModAction act, Exception e, ModItem? m)
    {
        Trace.TraceError($"Failed action {act} for {m?.Name ?? "null item"}, ex: {e}");

        string title = $"Failed to {act}";
        string message;

        switch (e)
        {
            case HttpRequestException:
            {
                message = string.Format(
                    Localization.MLVM_DisplayNetworkError_Msgbox_Text, 
                    m?.Name ?? "the API"
                );
                break;
            }

            case HashMismatchException hashEx:
            {
                message = string.Format(
                    Localization.MLVM_DisplayHashMismatch_Msgbox_Text,
                    hashEx.Name,
                    hashEx.Actual,
                    hashEx.Expected
                );
                break;
            }

            default:
            {
                message = e.Message;
                break;
            }
        }

        _notify?.Show(new Notification(
            title,
            message,
            NotificationType.Error
        ));
    }

    private void OnComplete(ModPageViewModel.ModAction act, ModItem mod)
    {
        // BepInEx 特殊通知（标题就是传入的名称）
        bool isBepInExNotification = mod.Name != "BepInEx" && 
            (mod.Name.Contains("发现新版本") || 
            mod.Name.Contains("已是最新") || 
            mod.Name.Contains("提示") || 
            mod.Name.Contains("错误"));
        
        string title;
        string message;
        NotificationType notifType;
        
        if (isBepInExNotification)
        {
            // BepInEx 更新检查通知
            title = mod.Name;
            message = mod.Description;
            
            notifType = title.Contains("错误") ? NotificationType.Error
                : title.Contains("发现新版本") ? NotificationType.Warning
                : title.Contains("已是最新") ? NotificationType.Success
                : NotificationType.Information;
        }
        else
        {
            // 普通 Mod 操作通知
            title = "Success!";
            
            string act_s = act switch
            {
                ModPageViewModel.ModAction.Install => Localization.NOTIFY_Installed,
                ModPageViewModel.ModAction.Update => Localization.NOTIFY_Updated,
                ModPageViewModel.ModAction.Uninstall => Localization.NOTIFY_Uninstalled,
                ModPageViewModel.ModAction.Toggle => "",
                _ => throw new ArgumentOutOfRangeException(nameof(act), act, null)
            };
            
            message = string.IsNullOrEmpty(act_s) ? mod.Description : $"{act_s} {mod.Name}!";
            notifType = NotificationType.Success;
        }

        _notify?.Show(new Notification(title, message, notifType));
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var tl = TopLevel.GetTopLevel(this);

        _notify = new WindowNotificationManager(tl) 
        { 
            MaxItems = 3,
            Position = NotificationPosition.BottomRight
        };
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!Search.IsFocused)
            Search.Focus();
    }
}