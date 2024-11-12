namespace ScannerKeyHunt.Domain.Helpers
{
    public class PaginationHelper
    {
        public PaginatedResult<T> GetPaged<T>(IQueryable<T> query, int pageNumber, int pageSize)
        {
            int totalItems = query.Count();

            List<T>? items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResult<T>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
