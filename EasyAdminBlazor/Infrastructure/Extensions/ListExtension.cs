using BootstrapBlazor.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public static class ListExtension
{
    /// <summary>
    /// 转换select tree数据格式
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="items"></param>
    /// <param name="parentId"></param>
    /// <param name="getText"></param>
    /// <returns></returns>
    public static List<TreeViewItem<TKey>> BuildTreeViews<TItem, TKey>(
    this IEnumerable<TItem> items,
    TKey parentId,
    Func<TItem, string> getText,
    Func<TItem, CheckboxState>? getCheckedState = null)
    where TItem : IHasParentId<TKey>
    where TKey : struct
    {
        // 先将所有子项按 ParentId 分组，减少重复筛选
        var groupedItems = items.GroupBy(i => i.ParentId).ToArray();
        // 初始层级为 1
        return BuildTreeViewsRecursive(groupedItems, parentId, getText, getCheckedState, 1);
    }

    private static List<TreeViewItem<TKey>> BuildTreeViewsRecursive<TItem, TKey>(
        IGrouping<TKey, TItem>[] groupedItems,
        TKey parentId,
        Func<TItem, string> getText,
        Func<TItem, CheckboxState>? getCheckedState,
        int currentLevel)
        where TItem : IHasParentId<TKey>
        where TKey : struct
    {
        var ret = new List<TreeViewItem<TKey>>();
        var currentChildren = groupedItems.FirstOrDefault(g => EqualityComparer<TKey>.Default.Equals(g.Key, parentId));

        if (currentChildren != null)
        {
            foreach (var item in currentChildren)
            {
                var hasChildren = groupedItems.Any(g => EqualityComparer<TKey>.Default.Equals(g.Key, item.Id));
                var checkedState = getCheckedState != null ? getCheckedState(item) : CheckboxState.UnChecked;
                ret.Add(new TreeViewItem<TKey>(item.Id)
                {
                    Text = getText(item),
                    IsExpand = hasChildren,
                    CheckedState = checkedState,
                    HasChildren = hasChildren,
                    // 设置 CssClass 为当前层级
                    CssClass = $"level-{currentLevel}",
                    Items = BuildTreeViewsRecursive(groupedItems, item.Id, getText, getCheckedState, currentLevel + 1)
                });
            }
        }

        return ret;
    }

    /// <summary>
    /// 转换tree table数据格式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="items"></param>
    /// <param name="parentId"></param>
    /// <returns></returns>
    public static IEnumerable<TableTreeNode<T>> BuildTreeNodes<T, TKey>(this IEnumerable<T> items, TKey parentId, Func<T, TKey>? getParentId)
    where T : IEntity<TKey>
    where TKey : struct
    {
        // 先将所有子项按 ParentId 分组，减少重复筛选
        var groupedItems = items.GroupBy(i => getParentId == null ? (TKey)typeof(T).GetProperty("ParentId")?.GetValue(i)! : getParentId(i)).ToArray();
        return BuildTreeNodesRecursive(groupedItems, parentId);
    }

    private static IEnumerable<TableTreeNode<T>> BuildTreeNodesRecursive<T, TKey>(IGrouping<TKey, T>[] groupedItems, TKey parentId)
        where T : IEntity<TKey>
        where TKey : struct
    {
        var ret = new List<TableTreeNode<T>>();
        // 查找当前父节点的子节点分组
        var currentChildrenGroup = groupedItems.FirstOrDefault(g => EqualityComparer<TKey>.Default.Equals(g.Key, parentId));

        if (currentChildrenGroup != null)
        {
            foreach (var item in currentChildrenGroup)
            {
                // 检查当前节点是否有子节点
                var hasChildren = groupedItems.Any(g => EqualityComparer<TKey>.Default.Equals(g.Key, item.Id));
                ret.Add(new TableTreeNode<T>(item)
                {
                    IsExpand = hasChildren,
                    HasChildren = hasChildren,
                    Items = BuildTreeNodesRecursive(groupedItems, item.Id)
                });
            }
        }

        return ret;
    }
}
