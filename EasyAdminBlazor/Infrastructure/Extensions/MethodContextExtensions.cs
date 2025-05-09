using Rougamo.Context;

public static class MethodContextExtensions
{
    public static IServiceProvider GetServiceProvider(this MethodContext context)
    {
        var targetType = context.Target.GetType();
        var service = targetType.GetPropertyOrFieldValue(context.Target, "ServiceProvider") as IServiceProvider;
        if (service == null)
        {
            throw new Exception($"_Imports.razor 未使用 @inject IServiceProvider ServiceProvider");
        }
        return service;
    }
}
