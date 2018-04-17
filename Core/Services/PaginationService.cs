using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Services.Interfaces;
using System.Linq.Dynamic.Core;

namespace Refundeo.Core.Services
{
    public class PaginationService<T> : IPaginationService<T> where T : class
    {
        private static readonly string ORDER_BY = nameof(Queryable.OrderBy);
        private static readonly string ORDER_BY_DESCENDING = nameof(Queryable.OrderByDescending);
        private static readonly string THEN_BY = nameof(Queryable.ThenBy);
        private static readonly string THEN_BY_DESCENDING = nameof(Queryable.ThenByDescending);

        public IQueryable<T> PaginateSortAndFilter(IQueryable<T> query, int first, int amount, string sortBy, string dir, string filterBy)
        {
            var sortProp = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var filterProp = typeof(T).GetProperty(filterBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (filterProp != null)
            {
                query = Filter(query, filterProp);
            }

            if (sortProp != null)
            {
                query = Sort(query, sortProp, dir);
            }
            return Paginate(query, first, amount);
        }

        public IQueryable<T> PaginateAndSort(IQueryable<T> query, int first, int amount, string sortBy, string dir)
        {
            var sortProp = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (sortProp != null)
            {
                query = Sort(query, sortProp, dir);
            }

            return Paginate(query, first, amount);
        }

        public IQueryable<T> SortAndFilter(IQueryable<T> query, string sortBy, string dir, string filterBy)
        {
            var sortProp = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var filterProp = typeof(T).GetProperty(filterBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (filterProp != null)
            {
                query = Filter(query, filterProp);
            }

            if (sortProp != null)
            {
                query = Sort(query, sortProp, dir);
            }

            return query;
        }

        public IQueryable<T> Paginate(IQueryable<T> query, int first, int amount)
        {
            return query.Skip(first).Take(amount);
        }

        public IQueryable<T> Filter(IQueryable<T> query, PropertyInfo filterProp)
        {
            if (filterProp.PropertyType == typeof(Boolean))
            {
                query = query.Where($"{filterProp.Name}");
            }
            if (filterProp.PropertyType == typeof(String))
            {
                query = query.Where($"!String.IsNullOrEmpty({filterProp.Name})");
            }
            else
            {
                query = query.Where($"{filterProp.Name} != null");
            }
            return query;
        }

        public IQueryable<T> Sort(IQueryable<T> query, PropertyInfo sortProp, string dir)
        {
            var orderBy = dir == "asc" ? ORDER_BY : ORDER_BY_DESCENDING;
            var thenBy = dir == "asc" ? THEN_BY : THEN_BY_DESCENDING;

            String orderByProp = sortProp.Name;
            String thenByProp = null;

            if (sortProp.PropertyType == typeof(CustomerInformation))
            {
                orderByProp += $".{nameof(CustomerInformation.FirstName)}"; ;
                thenByProp = $"{sortProp.Name}.{nameof(CustomerInformation.LastName)}";
            }

            query = ApplyOrder(query, orderByProp, orderBy);

            if (thenByProp != null)
            {
                query = ApplyOrder(query, thenByProp, thenBy);
            }

            return query;
        }

        public IOrderedQueryable<T> ApplyOrder(IQueryable<T> source, string property, string methodName)
        {
            var props = property.Split('.');
            var type = typeof(T);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (var prop in props)
            {
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            var result = typeof(Queryable).GetMethods().Single(
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
