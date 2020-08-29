using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Mongo;
using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo.Migrations
{
    internal static class Migrator
    {
        //Starting from 4 in case we want to implement migrations for previous versions
        public static int CurrentVersion = 4;

        public static async Task Apply(IMongoCollection<MigrationHistory> migrationCollection)
        {
            var history = (await migrationCollection.WhereAsync(x => true)).ToList();

            if (history.Count == 0)
            {
                await migrationCollection.InsertOneAsync(new MigrationHistory
                {
                    InstalledOn = DateTime.Now,
                    DatabaseVersion = CurrentVersion
                });

                //We have nothing to migrate yet but now we can introduce the first flag to recognize which version is installed
            }
        }
    }
}
