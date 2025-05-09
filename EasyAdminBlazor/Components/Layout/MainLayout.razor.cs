using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EasyAdminBlazor.Components.Layout
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class MainLayout
    {
        private bool UseTabSet { get; set; } = true;

        private bool IsFixedHeader { get; set; } = true;

        private bool IsFixedTabHeader { get; set; } = true;

        private bool IsFixedFooter { get; set; } = true;

        private bool IsFullSide { get; set; } = true;

        private bool ShowFooter { get; set; } = false;

        private List<MenuItem>? Menus { get; set; }

        /// <summary>
        /// OnInitialized 方法
        /// </summary>
        async protected override Task OnInitializedAsync()
        {
            await admin.Init();

            if (!await admin.AuthPath(new Uri(nav.Uri).AbsolutePath) && admin.User == null)
            {
                admin.RedirectLogin();
                return;
            }

            var data = admin.RoleMenus.Where(a => new[] { SysMenuType.菜单, SysMenuType.增删改查 }.Contains(a.Type)).OrderBy(a => a.Sort).ToList();
            Menus = new List<MenuItem>();
            foreach (var item1 in data.Where(x => x.ParentId == 0))
            {
                List<MenuItem> menusubs = new List<MenuItem>();
                foreach (var item2 in data.Where(x => x.ParentId == item1.Id))
                {
                    menusubs.Add(new MenuItem { Text = item2.Label, Url = item2.Path,Icon= item2.Icon });
                }

                Menus.Add(new MenuItem { Text = item1.Label, Url = item1.Path, Items = menusubs, Icon = "fas " + item1.Icon.IsNull("fa-laptop") });
            }

            await base.OnInitializedAsync();
        }

        async Task<bool> OnAuthorizing(string path)
        {
            await Task.Delay(100);
            return await admin.AuthPath(new Uri(path).AbsolutePath);
        }

        async Task LogoutClick()
        {
            await admin.SignOut();
            admin.RedirectLogin();
        }
        private Task OnErrorHandleAsync(ILogger logger, Exception ex)
        {
            System.Console.Error.WriteLine(ex);
            return Task.CompletedTask;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            locationChangingHandler = nav.RegisterLocationChangingHandler(async e =>
            {
                var uri = nav.ToAbsoluteUri(e.TargetLocation);
                await admin.AuthPath(uri.AbsolutePath);
            });
        }

        IDisposable locationChangingHandler;
        public void Dispose() => locationChangingHandler?.Dispose();
    }
}
