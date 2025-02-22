using AspNetCore.Identity.Mongo.Migrations;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Mongo;
using AspNetCore.Identity.Mongo.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using System;

namespace AspNetCore.Identity.Mongo;

public static class MongoIdentityExtensions
{
    public static IdentityBuilder AddIdentityMongoDbProvider<TUser>(this IServiceCollection services)
        where TUser : MongoUser
    {
        return AddIdentityMongoDbProvider<TUser, MongoRole<ObjectId>, ObjectId>(services, x => { });
    }

    public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TKey>(this IServiceCollection services)
        where TKey : IEquatable<TKey>
        where TUser : MongoUser<TKey>
    {
        return AddIdentityMongoDbProvider<TUser, MongoRole<TKey>, TKey>(services, _ => { });
    }

    public static IdentityBuilder AddIdentityMongoDbProvider<TUser>(this IServiceCollection services,
        Action<MongoIdentityOptions> setupDatabaseAction)
        where TUser : MongoUser
    {
        return AddIdentityMongoDbProvider<TUser, MongoRole, ObjectId>(services, setupDatabaseAction);
    }

    public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TKey>(this IServiceCollection services,
        Action<MongoIdentityOptions> setupDatabaseAction)
        where TKey : IEquatable<TKey>
        where TUser : MongoUser<TKey>
    {
        return AddIdentityMongoDbProvider<TUser, MongoRole<TKey>, TKey>(services, setupDatabaseAction);
    }

    public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TRole>(this IServiceCollection services,
        Action<IdentityOptions> setupIdentityAction, Action<MongoIdentityOptions> setupDatabaseAction)
        where TUser : MongoUser
        where TRole : MongoRole
    {
        return AddIdentityMongoDbProvider<TUser, TRole, ObjectId, ObjectId>(services, setupIdentityAction, setupDatabaseAction);
    }

    public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TRole, TKey>(this IServiceCollection services,
        Action<MongoIdentityOptions> setupDatabaseAction)
        where TKey : IEquatable<TKey>
        where TUser : MongoUser<TKey>
        where TRole : MongoRole<TKey>
    {
        return AddIdentityMongoDbProvider<TUser, TRole, TKey, TKey>(services, _ => { }, setupDatabaseAction);
    }

    public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TRole, TKeyUser, TKeyRole>(this IServiceCollection services,
        Action<MongoIdentityOptions> setupDatabaseAction)
        where TKeyUser : IEquatable<TKeyUser>
        where TKeyRole : IEquatable<TKeyRole>
        where TUser : MongoUser<TKeyUser>
        where TRole : MongoRole<TKeyRole>
    {
        return AddIdentityMongoDbProvider<TUser, TRole, TKeyUser, TKeyRole>(services, _ => { }, setupDatabaseAction);
    }

    public static IdentityBuilder AddIdentityMongoDbProvider(this IServiceCollection services,
        Action<IdentityOptions> setupIdentityAction, Action<MongoIdentityOptions> setupDatabaseAction)
    {
        return AddIdentityMongoDbProvider<MongoUser, MongoRole, ObjectId, ObjectId>(services, setupIdentityAction, setupDatabaseAction);
    }

    public static IdentityBuilder AddIdentityMongoDbProvider<TUser>(this IServiceCollection services,
        Action<IdentityOptions> setupIdentityAction, Action<MongoIdentityOptions> setupDatabaseAction) where TUser : MongoUser
    {
        return AddIdentityMongoDbProvider<TUser, MongoRole, ObjectId, ObjectId>(services, setupIdentityAction, setupDatabaseAction);
    }

    public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TRole, TKeyUser, TKeyRole>(this IServiceCollection services,
        Action<IdentityOptions> setupIdentityAction, Action<MongoIdentityOptions> setupDatabaseAction, IdentityErrorDescriber identityErrorDescriber = null)
        where TKeyUser : IEquatable<TKeyUser>
        where TKeyRole : IEquatable<TKeyRole>
        where TUser : MongoUser<TKeyUser>
        where TRole : MongoRole<TKeyRole>
    {
        var dbOptions = new MongoIdentityOptions();
        setupDatabaseAction(dbOptions);

        var migrationCollection = MongoUtil.FromConnectionString<MigrationHistory>(dbOptions, dbOptions.MigrationCollection);
        var migrationUserCollection = MongoUtil.FromConnectionString<MigrationMongoUser<TKeyUser>>(dbOptions, dbOptions.UsersCollection);
        var userCollection = MongoUtil.FromConnectionString<TUser>(dbOptions, dbOptions.UsersCollection);
        var roleCollection = MongoUtil.FromConnectionString<TRole>(dbOptions, dbOptions.RolesCollection);

        // apply migrations before identity services resolved
        if (!dbOptions.DisableAutoMigrations)
        {
            Migrator.Apply<MigrationMongoUser<TKeyUser>, TRole, TKeyUser, TKeyRole>(
                migrationCollection, migrationUserCollection, roleCollection);
        }

        var builder = services.AddIdentity<TUser, TRole>(setupIdentityAction ?? (x => { }));

        builder.AddRoleStore<RoleStore<TRole, TKeyRole>>()
            .AddUserStore<UserStore<TUser, TRole, TKeyUser, TKeyRole>>()
            .AddUserManager<UserManager<TUser>>()
            .AddRoleManager<RoleManager<TRole>>()
            .AddDefaultTokenProviders();

        services.AddSingleton(x => userCollection);
        services.AddSingleton(x => roleCollection);

        // register custom ObjectId TypeConverter
        if (typeof(TKeyUser) == typeof(ObjectId))
        {
            TypeConverterResolver.RegisterTypeConverter<ObjectId, ObjectIdConverter>();
        }

        // Identity Services
        services.AddTransient<IRoleStore<TRole>>(x => new RoleStore<TRole, TKeyRole>(roleCollection, identityErrorDescriber));
        services.AddTransient<IUserStore<TUser>>(x => new UserStore<TUser, TRole, TKeyUser, TKeyRole>(userCollection, roleCollection, identityErrorDescriber));

        return builder;
    }
}