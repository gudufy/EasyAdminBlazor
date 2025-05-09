namespace EasyAdminBlazor;

public interface IHasParentId<T>
{
    T Id { get; }
    T ParentId { get; }
}
