using System.Linq.Expressions;
using MongoDB.Driver;

namespace pefi.dynamicdns.Persistance
{
    public class MongoDatastore(IMongoClient client) : IDataStore
    {
        private IMongoCollection<T> GetCollection<T>(string database, string collection) 
            => client.GetDatabase(database).GetCollection<T>(collection);

        public async Task<IEnumerable<T>> GetAll<T>(string database, string collection, Expression<Func<T, bool>> predicate)
        {
            var a = await GetCollection<T>(database, collection)
                .FindAsync(predicate);

            return a.ToEnumerable();
        }

        public async Task<IEnumerable<T>> GetAll<T>(string database, string collection) => await GetAll<T>(database, collection, _ => true);

        public async Task<T> Add<T>(string database, string collection, T item)
        {
            await GetCollection<T>(database, collection)
                .InsertOneAsync(item);

            return item;
        }

        async Task<T> IDataStore.Get<T>(string database, string collection, Expression<Func<T, bool>> predicate)
               => (await GetAll<T>(database, collection, predicate)).Single();
    }
}
