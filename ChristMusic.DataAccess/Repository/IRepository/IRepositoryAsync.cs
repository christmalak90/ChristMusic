using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ChristMusic.DataAccess.Repository.IRepository
{
    public interface IRepositoryAsync<T> where T : class
    {
        Task<T> GetAsync(int id);

        Task<T> GetWithStringIdAsync(string id);

        //Retrieve list of T based on number of conditions
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null
            );

        //Retrieve T based on number of conditions
        Task<T> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> filter = null,
            string includeProperties = null
            );

        Task AddAsync(T entity);
        Task RemoveAsync(int id);
        Task RemoveAsync(T entity);

        //Remove range of T
        Task RemoveRangeAsync(IEnumerable<T> entity);
    }
}
