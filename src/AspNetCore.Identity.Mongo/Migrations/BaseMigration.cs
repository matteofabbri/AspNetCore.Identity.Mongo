using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspNetCore.Identity.Mongo.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo.Migrations
{
    internal abstract class BaseMigration
    {
        private static List<BaseMigration> _migrations;
        public static List<BaseMigration> Migrations  {
            get
            {
                if (_migrations == null)
                {
                    _migrations = typeof(BaseMigration)
                        .Assembly
                        .GetTypes()
                        .Where(t => typeof(BaseMigration).IsAssignableFrom(t))
                        .Select(t => t.GetConstructor(Type.EmptyTypes)?.Invoke(Array.Empty<object>()))
                        .Where(o => o != null)
                        .Cast<BaseMigration>()
                        .OrderBy(m => m.Version)
                        .ToList();
                    if (_migrations.Count != _migrations.Select(m => m.Version).Distinct().Count())
                    {
                        throw new InvalidOperationException("Migration versions must be unique, please check versions");
                    }
                }

                return _migrations;
            }
        }


        public abstract int Version { get; }

        public MigrationHistory Apply<TUser, TRole, TKey>(IMongoCollection<TUser> usersCollection,
            IMongoCollection<TRole> rolesCollection)
            where TKey : IEquatable<TKey>
            where TUser : MigrationMongoUser<TKey>
            where TRole : MongoRole<TKey>
        {
            DoApply<TUser, TRole, TKey>(usersCollection, rolesCollection);
            return new MigrationHistory
            {
                Id = ObjectId.GenerateNewId(),
                InstalledOn = DateTime.UtcNow,
                DatabaseVersion = Version + 1
            };
        }

        protected abstract void DoApply<TUser, TRole, TKey>(
            IMongoCollection<TUser> usersCollection, IMongoCollection<TRole> rolesCollection)
            where TKey : IEquatable<TKey>
            where TUser : MigrationMongoUser<TKey>
            where TRole : MongoRole<TKey>;
    }
}