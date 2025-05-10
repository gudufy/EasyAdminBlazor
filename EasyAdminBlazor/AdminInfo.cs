using BootstrapBlazor.Components;
using FreeSql;

namespace EasyAdminBlazor
{
    public class AdminRemoveEventArgs<TItem> where TItem : class
    {
        public IAggregateRootRepository<TItem> Repo { get; set; }
        public List<TItem> Items { get; set; }
        public bool Cancel { get; set; }
    }

    public class AdminSaveEventArgs<TItem> where TItem : class
    {
        public IAggregateRootRepository<TItem> Repo { get; set; }
        public ItemChangedType ChangedType { get; set; }
        public TItem Item { get; set; }
        public bool Cancel { get; set; }
    }
}
