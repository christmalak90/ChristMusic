using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ChristMusic.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        T Get(int id);

        T GetWithStringId(string id);

        //Retrieve list of T based on number of conditions
        IEnumerable<T> GetAll(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null
            );

        //Retrieve T based on number of conditions
        T GetFirstOrDefault(
            Expression<Func<T, bool>> filter = null,
            string includeProperties = null
            );

        void Add(T entity);
        void Remove(int id);
        void Remove(T entity);

        //Remove range of T
        void RemoveRange(IEnumerable<T> entity);
    }
}
