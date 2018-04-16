using System.Collections.Generic;
using System.Linq;

namespace Refundeo.Core.Services.Interfaces
{
    public interface ISort<T> where T : class
    {
        IQueryable<T> PaginateAndSort(IQueryable<T> query, int first, int amount, string sortBy, int dir);
    }
}