using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo.Mongo
{
    public static class MongoUtil
    {
        private static FindOptions<TItem> LimitOneOption<TItem>() => new FindOptions<TItem>
        {
            Limit = 1
        };


        public static IMongoCollection<TItem> FromConnectionString<TItem>(string connectionString)
        {
            var name = typeof(TItem).Name.ToCharArray();
            name[0] = char.ToLowerInvariant(name[0]);

            var newName = new String(name);

            return FromConnectionString<TItem>(connectionString, newName);
        }

        public static IMongoCollection<TItem> FromConnectionString<TItem>(string connectionString, string collectionName)
        {
            IMongoCollection<TItem> collection;
                
            var type = typeof(TItem);


            if (connectionString != null)
            {
                var url = new MongoUrl(connectionString);
                var client = new MongoClient(connectionString);
                collection = client.GetDatabase(url.DatabaseName ?? "default")
                    .GetCollection<TItem>(collectionName ?? type.Name.ToLowerInvariant());
            }
            else
            {
                collection = new MongoClient().GetDatabase("default")
                    .GetCollection<TItem>(collectionName ?? type.Name.ToLowerInvariant());
            }

            return collection;
        }

        public static void AscendingIndex<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, object>> field)
        {
            var def = Builders<TItem>.IndexKeys.Ascending(field);
            collection.Indexes.CreateOne(new CreateIndexModel<TItem>(def));
        }

        public static void DescendingIndex<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, object>> field)
        {
            var def = Builders<TItem>.IndexKeys.Descending(field);
            collection.Indexes.CreateOne(new CreateIndexModel<TItem>(def));
        }

        public static void FullTextIndex<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, object>> field)
        {
            var def = Builders<TItem>.IndexKeys.Text(field);
            collection.Indexes.CreateOne(new CreateIndexModel<TItem>(def));
        }

        public static async Task<IEnumerable<TItem>> TakeAsync<TItem>(this IMongoCollection<TItem> collection, int count, int skip = 0)
        {
            return await (await collection.FindAsync(x => true, new FindOptions<TItem, TItem>()
            {
                Skip = skip,
                Limit = count
            })).ToListAsync();
        }

        public static async Task<List<TItem>> All<TItem>(this IMongoCollection<TItem> mongoCollection)
        {
            return (await (await mongoCollection.FindAsync(Builders<TItem>.Filter.Empty)).ToListAsync());
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

        public static TItem FirstOrDefault<TItem>(this IMongoCollection<TItem> mongoCollection, Expression<Func<TItem, bool>> p)
        {
            return (mongoCollection.Find(p)).FirstOrDefault();
        }

        public static async Task<TItem> FirstOrDefaultAsync<TItem>(this IMongoCollection<TItem> mongoCollection, FilterDefinition<TItem> p)
        {
            return await (await mongoCollection.FindAsync(p,LimitOneOption<TItem>())).FirstOrDefaultAsync();
        }

        public static async Task<TItem> FirstOrDefaultAsync<TItem>(this IMongoCollection<TItem> mongoCollection, Expression<Func<TItem, bool>> p)
        {
            return await (await mongoCollection.FindAsync(p,LimitOneOption<TItem>())).FirstOrDefaultAsync();
        }

        public static async Task ForEachAsync<TItem>(this IMongoCollection<TItem> mongoCollection, Expression<Func<TItem, bool>> p,  Action<TItem> action)
        {
            await (await mongoCollection.FindAsync(p)).ForEachAsync(action);
        }

        public static async Task ForEachAsync<TItem>(this IMongoCollection<TItem> mongoCollection, Action<TItem> action)
        {
            await (await mongoCollection.FindAsync(Builders<TItem>.Filter.Empty)).ForEachAsync(action);
        }

        public static async Task<IEnumerable<TItem>> WhereAsync<TItem>(this IMongoCollection<TItem> mongoCollection, FilterDefinition<TItem> p)
        {
            return (await mongoCollection.FindAsync(p)).ToEnumerable();
        }

        public static async Task<IEnumerable<TItem>> WhereAsync<TItem>(this IMongoCollection<TItem> mongoCollection, Expression<Func<TItem, bool>> p)
        {
            return (await mongoCollection.FindAsync(p)).ToEnumerable();
        }

        public static async Task<IEnumerable<TItem>> TextSearch<TItem>(this IMongoCollection<TItem> mongoCollection, string str)
        {
            return (await mongoCollection.FindAsync(Builders<TItem>.Filter.Text(str))).ToEnumerable();
        }

        public static async Task<IEnumerable<TItem>> TextSearch<TItem>(this IMongoCollection<TItem> mongoCollection, string str, Expression<Func<TItem, bool>> filter)
        {
            var f = Builders<TItem>.Filter.Text(str) & filter;
            return (await mongoCollection.FindAsync(f)).ToEnumerable();
        }
    }
}
