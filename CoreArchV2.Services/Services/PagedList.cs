using CoreArchV2.Services.Interfaces;

namespace CoreArchV2.Services.Services
{
    [Serializable]
    public class PagedList<T> : List<T>, IPagedList<T>
    {
        public PagedList(IQueryable<T> source, int? pageIndex, int pageSize)
        {
            if (!pageIndex.HasValue || pageIndex < 1)
                pageIndex = 1;

            CurrentPage = pageIndex.Value;
            pageIndex = pageIndex - 1;
            var total = source.Count();
            TotalCount = total;
            TotalPages = total / pageSize;

            if (total % pageSize > 0)
                TotalPages++;

            PageSize = pageSize;
            PageIndex = pageIndex.Value;
            if (TotalPages == 0)
                AddRange(source.ToList());
            else
                AddRange(source.Skip(pageIndex.Value * pageSize).Take(pageSize).ToList());
        }

        public PagedList(IQueryable<T> source, int? pageIndex, int pageSize, bool? excel)
        {
            var flag = false;

            if (excel.HasValue)
                if (excel.Value)
                    flag = true;

            if (!flag)
            {
                if (!pageIndex.HasValue || pageIndex < 1)
                    pageIndex = 1;

                CurrentPage = pageIndex.Value;
                pageIndex = pageIndex - 1;
                var total = source.Count();
                TotalCount = total;
                TotalPages = total / pageSize;

                if (total % pageSize > 0)
                    TotalPages++;

                PageSize = pageSize;
                PageIndex = pageIndex.Value;
                if (TotalPages == 0)
                    AddRange(source.ToList());
                else
                    AddRange(source.Skip(pageIndex.Value * pageSize).Take(pageSize).ToList());
            }
            else
            {
                AddRange(source.ToList());
            }
        }

        public PagedList(List<T> source, int? pageIndex, int pageSize, int tCount)
        {
            if (!pageIndex.HasValue || pageIndex < 1)
                pageIndex = 1;

            CurrentPage = pageIndex.Value;
            pageIndex = pageIndex - 1;
            var total = tCount < 1 ? source.Count() : tCount;
            TotalCount = total;
            TotalPages = total / pageSize;

            if (total % pageSize > 0)
                TotalPages++;

            PageSize = pageSize;
            PageIndex = pageIndex.Value;
            AddRange(source);
        }

        public PagedList(IList<T> source, int? pageIndex, int pageSize)
        {
            if (!pageIndex.HasValue || pageIndex < 1)
                pageIndex = 1;

            CurrentPage = pageIndex.Value;
            pageIndex = pageIndex - 1;
            TotalCount = source.Count();
            TotalPages = TotalCount / pageSize;

            if (TotalCount % pageSize > 0)
                TotalPages++;

            PageSize = pageSize;
            PageIndex = pageIndex.Value;
            AddRange(source.Skip(pageIndex.Value * pageSize).Take(pageSize).ToList());
        }

        public PagedList(IEnumerable<T> source, int? pageIndex, int pageSize, string totalCount)
        {
            if (!pageIndex.HasValue || pageIndex < 1)
                pageIndex = 1;

            CurrentPage = pageIndex.Value;
            pageIndex = pageIndex - 1;
            TotalCount = Convert.ToInt32(totalCount);
            TotalPages = TotalCount / pageSize;

            if (TotalCount % pageSize > 0)
                TotalPages++;

            PageSize = pageSize;
            PageIndex = pageIndex.Value;
            AddRange(source);
        }

        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int CurrentPage { get; private set; }
        public int PageIndex { get; private set; }
        public int TotalCount { get; private set; }

        public bool HasNextPage => PageIndex + 1 < TotalPages;

        public bool HasPreviousPage => PageIndex > 0;
    }
}