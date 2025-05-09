using Microsoft.Extensions.Localization;

namespace EasyAdminBlazor
{
    public class CommonLocalizer
    {
        private readonly IStringLocalizer _localizer;

        public CommonLocalizer(IStringLocalizerFactory factory)
        {
            // 加载公共语言包
            _localizer = factory.Create("Common", "EasyAdminBlazor");
        }

        public string this[string name] => _localizer[name];
    }
}
