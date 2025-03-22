namespace CoreArchV2.Services.Interfaces
{
    public interface IPagedList<T> : IList<T>
    {
        int CurrentPage { get; }
        int PageIndex { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }
}