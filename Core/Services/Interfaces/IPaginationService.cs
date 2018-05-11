using System.Linq;
using System.Reflection;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IPaginationService<T> where T : class
    {
        IQueryable<T> PaginateSortAndFilter(IQueryable<T> query, int first, int amount, string sortBy, string dir,
            string filterBy);

        IQueryable<T> PaginateAndSort(IQueryable<T> query, int first, int amount, string sortBy, string dir);
        IQueryable<T> SortAndFilter(IQueryable<T> query, string sortBy, string dir, string filterBy);
        IQueryable<T> Sort(IQueryable<T> query, PropertyInfo sortProp, string dir);
        IQueryable<T> Filter(IQueryable<T> query, PropertyInfo filterProp);
        IOrderedQueryable<T> ApplyOrder(IQueryable<T> source, string property, string methodName);
        IQueryable<T> Paginate(IQueryable<T> query, int first, int amount);
    }
}
