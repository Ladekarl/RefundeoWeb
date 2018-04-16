using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Core.Services
{
    public class RefundeoSort<T> : ISort<T> where T : class
    {
        public IQueryable<T> PaginateAndSort(IQueryable<T> query, int first, int amount, string sortBy, int dir)
        {
            var sortProp = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (sortProp != null)
            {
                query = sort(query, sortProp, dir);
            }
            return query.Skip(first).Take(amount);
        }

        private IQueryable<T> sort(IQueryable<T> query, PropertyInfo sortProp, int dir)
        {
            var orderBy = dir == 1 ? "OrderBy" : "OrderByDescending";
            var thenBy = dir == 1 ? "ThenBy" : "ThenByDescending";

            String orderByProp = sortProp.Name;
            String thenByProp = null;

            if (sortProp.PropertyType == typeof(DateTime))
            {
                orderByProp += ".Date";
                thenByProp = sortProp.Name + ".TimeOfDay";
            }
            else if (sortProp.PropertyType == typeof(RefundeoUser))
            {
                orderByProp += ".FirstName";
                thenByProp = sortProp.Name + ".LastName";
            }

            query = ApplyOrder(query, orderByProp, orderBy);
            if (thenByProp != null)
            {
                query = ApplyOrder(query, thenByProp, thenBy);
            }
            return query;
        }

        private static IOrderedQueryable<T> ApplyOrder(IQueryable<T> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), type)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }
    }
}