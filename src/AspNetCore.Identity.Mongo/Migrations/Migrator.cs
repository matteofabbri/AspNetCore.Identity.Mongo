using AspNetCore.Identity.Mongo.Model;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]

namespace AspNetCore.Identity.Mongo.Migrations;

internal static class Migrator
{
    //Starting from 4 in case we want to implement migrations for previous versions
    public static int CurrentVersion = 6;

    public static void Apply<TUser, TRole, TKeyUser, TKeyRole>(IMongoCollection<MigrationHistory> migrationCollection,
        IMongoCollection<TUser> usersCollection, IMongoCollection<TRole> rolesCollection)
        where TKeyUser : IEquatable<TKeyUser>
        where TKeyRole : IEquatable<TKeyRole>
        where TUser : MigrationMongoUser<TKeyUser>
        where TRole : MongoRole<TKeyRole>
    {
        var version = migrationCollection
            .Find(h => true)
            .SortByDescending(h => h.DatabaseVersion)
            .Project(h => h.DatabaseVersion)
            .FirstOrDefault();

        var appliedMigrations = BaseMigration.Migrations
            .Where(m => m.Version >= version)
            .Select(migration => migration.Apply<TUser, TRole, TKeyUser, TKeyRole>(usersCollection, rolesCollection))
            .ToList();

        if (appliedMigrations.Count > 0)
        {
            migrationCollection.InsertMany(appliedMigrations);
        }
    }
}