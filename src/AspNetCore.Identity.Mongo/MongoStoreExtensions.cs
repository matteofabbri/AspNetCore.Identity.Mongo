using AspNetCore.Identity.Mongo.Migrations;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Mongo;
using AspNetCore.Identity.Mongo.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using System;

namespace AspNetCore.Identity.Mongo;

public static class MongoStoreExtensions
{
    public static IdentityBuilder AddMongoDbStores<TUser>(this IdentityBuilder builder,
        Action<MongoIdentityOptions> setupDatabaseAction,
        IdentityErrorDescriber identityErrorDescriber = null)
        where TUser : MongoUser
    {
        return AddMongoDbStores<TUser, MongoRole, ObjectId, ObjectId>(builder, setupDatabaseAction, identityErrorDescriber);
    }

    public static IdentityBuilder AddMongoDbStores<TUser, TRole, TKeyUser, TKeyRole>(this IdentityBuilder builder,
        Action<MongoIdentityOptions> setupDatabaseAction,
        IdentityErrorDescriber identityErrorDescriber = null)
        where TKeyUser : IEquatable<TKeyUser>
        where TKeyRole : IEquatable<TKeyRole>
        where TUser : MongoUser<TKeyUser>
        where TRole : MongoRole<TKeyRole>
    {
        var dbOptions = new MongoIdentityOptions();
        setupDatabaseAction(dbOptions);

        var migrationCollection =
            MongoUtil.FromConnectionString<MigrationHistory>(dbOptions, dbOptions.MigrationCollection);
        var migrationUserCollection =
            MongoUtil.FromConnectionString<MigrationMongoUser<TKeyUser>>(dbOptions, dbOptions.UsersCollection);
        var userCollection = MongoUtil.FromConnectionString<TUser>(dbOptions, dbOptions.UsersCollection);
        var roleCollection = MongoUtil.FromConnectionString<TRole>(dbOptions, dbOptions.RolesCollection);

        if (!dbOptions.DisableAutoMigrations)
        {
            Migrator.Apply<MigrationMongoUser<TKeyUser>, TRole, TKeyUser, TKeyRole>(
                migrationCollection, migrationUserCollection, roleCollection);
        }

        builder.Services.AddSingleton(x => userCollection);
        builder.Services.AddSingleton(x => roleCollection);

        // register custom ObjectId TypeConverter
        if (typeof(TKeyUser) == typeof(ObjectId))
        {
            TypeConverterResolver.RegisterTypeConverter<ObjectId, ObjectIdConverter>();
        }

        // Identity Services
        builder.Services.AddTransient<IRoleStore<TRole>>(x =>
            new RoleStore<TRole, TKeyRole>(roleCollection, identityErrorDescriber));
        builder.Services.AddTransient<IUserStore<TUser>>(x =>
            new UserStore<TUser, TRole, TKeyUser, TKeyRole>(userCollection, roleCollection, identityErrorDescriber));

        return builder;
    }
}