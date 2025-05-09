using BootstrapBlazor.Components;
using EasyAdminBlazor;
using EasyAdminBlazor.Components;
using EasyAdminBlazor.Test.Components;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddEasyAdminBlazor(new EasyAdminBlazorOptions
{
    Assemblies = [typeof(Program).Assembly],
    EnableLocalization = false,
});

// Add services to the container.
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// 增加 Pdf 导出服务
builder.Services.AddBootstrapBlazorTableExportService();

// 增加 SignalR 服务数据传输大小限制配置
builder.Services.Configure<HubOptions>(option => option.MaximumReceiveMessageSize = null);

var app = builder.Build();

// 启用本地化
var option = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
if (option != null)
{
    app.UseRequestLocalization(option.Value);
}

app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(EasyAdminBlazorOptions).Assembly)
    .AddInteractiveServerRenderMode();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseBootstrapBlazor();

app.MapRazorPages();

app.Run();
