using EasyAdminBlazor.Components;
using EasyAdminBlazor.Test.Components;
using Microsoft.AspNetCore.SignalR;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddEasyAdminBlazor(new EasyAdminBlazorOptions
{
    Assemblies = [typeof(Program).Assembly]
});

// Add services to the container.
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// ���� Pdf ��������
builder.Services.AddBootstrapBlazorTableExportService();

// ���� SignalR �������ݴ����С��������
builder.Services.Configure<HubOptions>(option => option.MaximumReceiveMessageSize = null);

var app = builder.Build();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(EasyAdminBlazorOptions).Assembly)
    .AddInteractiveServerRenderMode();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
