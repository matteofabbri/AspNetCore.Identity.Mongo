using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo
{
    internal static class MongoUtil
    {
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

        public static async Task<IEnumerable<TItem>> TakeAsync<TItem>(this IMongoCollection<TItem> collection, int count, int skip = 0, CancellationToken cancellationToken = default)
        {
            var cursor = await collection.FindAsync(x => true, new FindOptions<TItem, TItem> { Skip = skip, Limit = count }, cancellationToken).ConfigureAwait(false);

            return await cursor.ToListAsync().ConfigureAwait(false);
        }

        public static async Task<TItem> FirstOrDefaultAsync<TItem>(this IMongoCollection<TItem> mongoCollection, Expression<Func<TItem, bool>> filter, CancellationToken cancellationToken)
        {
            var cursor = await mongoCollection.FindAsync(filter, new FindOptions<TItem, TItem> { Limit = 1 }, cancellationToken).ConfigureAwait(false);

            return await cursor.FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public static async Task<TProjection> FirstOrDefaultAsync<TItem, TProjection>(this IMongoCollection<TItem> mongoCollection, Expression<Func<TItem, bool>> filter, Expression<Func<TItem, TProjection>> projection, CancellationToken cancellationToken)
        {
            var cursor = await mongoCollection.FindAsync(filter, new FindOptions<TItem, TProjection> { Limit = 1, Projection = Builders<TItem>.Projection.Expression(projection) }, cancellationToken).ConfigureAwait(false);

            return await cursor.FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public static async Task<IEnumerable<TItem>> WhereAsync<TItem>(this IMongoCollection<TItem> mongoCollection, Expression<Func<TItem, bool>> filter, CancellationToken cancellationToken)
        {
            return (await mongoCollection.FindAsync(filter, cancellationToken: cancellationToken).ConfigureAwait(false)).ToEnumerable();
        }
    }
}
