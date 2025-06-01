using System.Linq.Expressions;

namespace pefi.dynamicdns.Persistance;

public interface IDataStore
{
    Task<T> Add<T>(string database, string collection, T item);

    Task<IEnumerable<T>> GetAll<T>(string database, string collection, Expression< Func<T, bool>> predicate);

    Task<T> Get<T>(string database, string collection, Expression<Func<T, bool>> predicate);

    Task<IEnumerable<T>> GetAll<T>(string database, string collection);

}
