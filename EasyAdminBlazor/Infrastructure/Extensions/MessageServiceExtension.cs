using BootstrapBlazor.Components;

public static class MessageServiceExtension
{
    /// <summary>
    /// Message 调用成功快捷方法
    /// </summary>
    /// <param name="service"></param>
    /// <param name="content">Content 属性</param>
    /// <param name="autoHide">自动隐藏属性默认为 true</param>
    public static Task Success(this MessageService service, string? content = null, bool autoHide = true) => Success(service, content, autoHide, true);

    /// <summary>
    /// Message 调用成功快捷方法
    /// </summary>
    /// <param name="service"></param>
    /// <param name="content">Content 属性</param>
    /// <param name="autoHide">自动隐藏属性默认为 true</param>
    /// <param name="showClose">是否显示关闭按钮 默认 true</param>
    public static Task Success(this MessageService service, string? content, bool autoHide, bool showClose) => service.Show(new MessageOption()
    {
        Color = Color.Success,
        IsAutoHide = autoHide,
        Content = content,
        ShowDismiss = showClose
    });

    /// <summary>
    /// Message 调用错误快捷方法
    /// </summary>
    /// <param name="service"></param>
    /// <param name="content">Content 属性</param>
    /// <param name="autoHide">自动隐藏属性默认为 true</param>
    public static Task Error(this MessageService service, string? content = null, bool autoHide = true) => Error(service, content, autoHide, true);

    /// <summary>
    /// Message 调用错误快捷方法
    /// </summary>
    /// <param name="service"></param>
    /// <param name="content">Content 属性</param>
    /// <param name="autoHide">自动隐藏属性默认为 true</param>
    /// <param name="showClose">是否显示关闭按钮 默认 true</param>
    public static Task Error(this MessageService service, string? content, bool autoHide, bool showClose) => service.Show(new MessageOption()
    {
        Color = Color.Danger,
        IsAutoHide = autoHide,
        Content = content,
        ShowDismiss = showClose
    });

    /// <summary>
    /// Message 调用提示信息快捷方法
    /// </summary>
    /// <param name="service"></param>
    /// <param name="title">Title 属性</param>
    /// <param name="content">Content 属性</param>
    /// <param name="autoHide">自动隐藏属性默认为 true</param>
    public static Task Information(this MessageService service, string? content = null, bool autoHide = true) => Information(service, content, autoHide, true);

    /// <summary>
    /// Message 调用提示信息快捷方法
    /// </summary>
    /// <param name="service"></param>
    /// <param name="content">Content 属性</param>
    /// <param name="autoHide">自动隐藏属性默认为 true</param>
    /// <param name="showClose">是否显示关闭按钮 默认 true</param>
    public static Task Information(this MessageService service, string? content, bool autoHide, bool showClose) => service.Show(new MessageOption()
    {
        Color = Color.Info,
        IsAutoHide = autoHide,
        Content = content,
        ShowDismiss = showClose
    });

    /// <summary>
    /// Message 调用警告信息快捷方法
    /// </summary>
    /// <param name="service"></param>
    /// <param name="content">Content 属性</param>
    /// <param name="autoHide">自动隐藏属性默认为 true</param>
    public static Task Warning(this MessageService service, string? content = null, bool autoHide = true) => Warning(service, content, autoHide, true);

    /// <summary>
    /// Message 调用警告信息快捷方法
    /// </summary>
    /// <param name="service"></param>
    /// <param name="content">Content 属性</param>
    /// <param name="autoHide">自动隐藏属性默认为 true</param>
    /// <param name="showClose">是否显示关闭按钮 默认 true</param>
    public static Task Warning(this MessageService service, string? content, bool autoHide, bool showClose) => service.Show(new MessageOption()
    {
        Color = Color.Warning,
        IsAutoHide = autoHide,
        Content = content,
        ShowDismiss = showClose
    });
}