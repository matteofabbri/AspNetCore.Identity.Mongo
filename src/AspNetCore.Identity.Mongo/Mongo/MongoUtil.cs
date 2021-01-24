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

        public static async Task<TItem> FirstOrDefaultAsync<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, bool>> p, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return await (await collection.FindAsync(p, LimitOneOption<TItem>(), cancellationToken).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public static async Task<IEnumerable<TItem>> WhereAsync<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, bool>> p, CancellationToken cancellationToken = default)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return (await collection.FindAsync(p, cancellationToken: cancellationToken).ConfigureAwait(false)).ToEnumerable();
        }
    }
}
