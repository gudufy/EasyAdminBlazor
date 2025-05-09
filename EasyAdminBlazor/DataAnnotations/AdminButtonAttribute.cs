using BootstrapBlazor.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Rougamo.Context;

[AttributeUsage(AttributeTargets.Method)]
public class AdminButtonAttribute
    : Rougamo.MoAttribute
{
    public string Name { get; set; }

    public AdminButtonAttribute(string name)
    {
        Name = name;
    }

    public override void OnEntry(MethodContext context)
    {
        var service = context.GetServiceProvider();
        var admin = service.GetService<AdminContext>();
        var swalService = service.GetService<SwalService>();

        Auth().ConfigureAwait(false).GetAwaiter().GetResult();

        async Task Auth()
        {
            if (!await admin.AuthButton(Name))
            {
                context.ReplaceReturnValue(this, context.ReturnType.CreateInstanceGetDefaultValue());
                await swalService.Warning("你没有权限执行该操作！");
            }
        }
    }
}
