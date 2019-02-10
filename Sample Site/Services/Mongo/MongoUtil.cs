using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace SampleSite.Mongo
{
    public static class MongoUtil
    {
        private static FindOptions<TItem> LimitOneOption<TItem>() => new FindOptions<TItem>
        {
            Limit = 1
        };

        public static IMongoCollection<TItem> FromConnectionString<TItem>(string connectionString, string collectionName)
        {
            var type = typeof(TItem);

            if (connectionString != null)
            {
                var url = new MongoUrl(connectionString);
                var client = new MongoClient(connectionString);
                return client.GetDatabase(url.DatabaseName ?? "default")
                    .GetCollection<TItem>(collectionName ?? type.Name.ToLowerInvariant());
            }

            return new MongoClient().GetDatabase("default")
                .GetCollection<TItem>(collectionName ?? type.Name.ToLowerInvariant());
        }

        public static async Task<IEnumerable<TItem>> TakeAsync<TItem>(this IMongoCollection<TItem> collection, int count, int skip = 0)
        {
            return await (await collection.FindAsync(x => true, new FindOptions<TItem, TItem>()
            {
                Skip = skip,
                Limit = count
            })).ToListAsync();
        }

        public static async Task<bool> AnyAsync<TItem>(this IMongoCollection<TItem> mongoCollection)
        {
            return await (await mongoCollection.FindAsync(x => true,LimitOneOption<TItem>())).AnyAsync();
        }


        public static async Task<bool> AnyAsync<TItem>(this IMongoCollection<TItem> mongoCollection, Expression<Func<TItem, bool>> p)
        {
            return await (await mongoCollection.FindAsync(p, LimitOneOption<TItem>())).AnyAsync();
        }

        public static async Task<TItem> FirstOrDefault<TItem>(this IMongoCollection<TItem> mongoCollection)
        {
            return await (await mongoCollection.FindAsync(Builders<TItem>.Filter.Empty,LimitOneOption<TItem>())).FirstOrDefaultAsync();
        }

        public static async Task<TItem> FirstOrDefaultAsync<TItem>(this IMongoCollection<TItem> mongoCollection, Expression<Func<TItem, bool>> p)
        {
            return await (await mongoCollection.FindAsync(p,LimitOneOption<TItem>())).FirstOrDefaultAsync();
        }

        public static async Task ForEachAsync<TItem>(this IMongoCollection<TItem> mongoCollection, Action<TItem> action)
        {
            await (await mongoCollection.FindAsync(Builders<TItem>.Filter.Empty)).ForEachAsync(action);
        }

        public static async Task<IEnumerable<TItem>> WhereAsync<TItem>(this IMongoCollection<TItem> mongoCollection, Expression<Func<TItem, bool>> p)
        {
            return (await mongoCollection.FindAsync(p)).ToEnumerable();
        }
    }
}
