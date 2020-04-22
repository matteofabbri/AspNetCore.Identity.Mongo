using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
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
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            var def = Builders<TItem>.IndexKeys.Ascending(field);

            collection.Indexes.CreateOne(new CreateIndexModel<TItem>(def));
        }

        public static void DescendingIndex<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, object>> field)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            var def = Builders<TItem>.IndexKeys.Descending(field);

            collection.Indexes.CreateOne(new CreateIndexModel<TItem>(def));
        }

        public static void FullTextIndex<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, object>> field)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            var def = Builders<TItem>.IndexKeys.Text(field);

            collection.Indexes.CreateOne(new CreateIndexModel<TItem>(def));
        }

        public static async Task<IEnumerable<TItem>> TakeAsync<TItem>(this IMongoCollection<TItem> collection, int count, int skip = 0, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return await (await collection.FindAsync(x => true, new FindOptions<TItem, TItem>(){Skip = skip,Limit = count}, cancellationToken).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
        }

        public static async Task<List<TItem>> AllAsync<TItem>(this IMongoCollection<TItem> collection, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return await (await collection.FindAsync(Builders<TItem>.Filter.Empty, cancellationToken: cancellationToken).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
        }

        public static async Task<bool> AnyAsync<TItem>(this IMongoCollection<TItem> collection, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return await (await collection.FindAsync(x => true, LimitOneOption<TItem>(), cancellationToken).ConfigureAwait(false)).AnyAsync().ConfigureAwait(false);
        }

        public static async Task<bool> AnyAsync<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, bool>> p, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return await (await collection.FindAsync(p, LimitOneOption<TItem>(), cancellationToken).ConfigureAwait(false)).AnyAsync().ConfigureAwait(false);
        }

        public static async Task<TItem> FirstOrDefault<TItem>(this IMongoCollection<TItem> collection, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return await (await collection.FindAsync(Builders<TItem>.Filter.Empty, LimitOneOption<TItem>(), cancellationToken).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public static TItem FirstOrDefault<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, bool>> p)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return collection.Find(p).FirstOrDefault();
        }

        public static async Task<TItem> FirstOrDefaultAsync<TItem>(this IMongoCollection<TItem> collection, FilterDefinition<TItem> p, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return await (await collection.FindAsync(p, LimitOneOption<TItem>(), cancellationToken).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public static async Task<TItem> FirstOrDefaultAsync<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, bool>> p, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return await (await collection.FindAsync(p, LimitOneOption<TItem>(), cancellationToken).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public static async Task ForEachAsync<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, bool>> p, Action<TItem> action, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            await (await collection.FindAsync(p, cancellationToken: cancellationToken).ConfigureAwait(false)).ForEachAsync(action).ConfigureAwait(false);
        }

        public static async Task ForEachAsync<TItem>(this IMongoCollection<TItem> collection, Action<TItem> action, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            await (await collection.FindAsync(Builders<TItem>.Filter.Empty, cancellationToken: cancellationToken).ConfigureAwait(false)).ForEachAsync(action).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<TItem>> WhereAsync<TItem>(this IMongoCollection<TItem> collection, FilterDefinition<TItem> p, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return (await collection.FindAsync(p).ConfigureAwait(false)).ToEnumerable();
        }

        public static async Task<IEnumerable<TItem>> WhereAsync<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, bool>> p, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return (await collection.FindAsync(p, cancellationToken: cancellationToken).ConfigureAwait(false)).ToEnumerable();
        }

        public static async Task<IEnumerable<TItem>> TextSearch<TItem>(this IMongoCollection<TItem> collection, string str, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return (await collection.FindAsync(Builders<TItem>.Filter.Text(str), cancellationToken: cancellationToken).ConfigureAwait(false)).ToEnumerable();
        }

        public static async Task<IEnumerable<TItem>> TextSearch<TItem>(this IMongoCollection<TItem> collection, string str, Expression<Func<TItem, bool>> filter, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            var f = Builders<TItem>.Filter.Text(str) & filter;

            return (await collection.FindAsync(f, cancellationToken: cancellationToken).ConfigureAwait(false)).ToEnumerable();
        }
    }
}
