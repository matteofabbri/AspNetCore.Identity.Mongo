using AspNetCore.Identity.Mongo.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Mongo;

namespace AspNetCore.Identity.Mongo
{
    class Migrator
    {
        internal static async Task Apply(IMongoCollection<MigrationHistory> migrationCollection)
        {
            var first = await migrationCollection.FirstOrDefaultAsync(x => true);
        }
    }
}
