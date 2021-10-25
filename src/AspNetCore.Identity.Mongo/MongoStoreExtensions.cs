using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Migrations;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Mongo;
using AspNetCore.Identity.Mongo.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;

namespace AspNetCore.Identity.Mongo
{
    public static class MongoStoreExtensions
    {
        public static IdentityBuilder AddMongoDbStores<TUser>(this IdentityBuilder builder, Action<MongoIdentityOptions> setupDatabaseAction,
            IdentityErrorDescriber identityErrorDescriber = null)
            where TUser : MongoUser
        {
            return AddMongoDbStores<TUser, MongoRole, ObjectId>(builder, setupDatabaseAction, identityErrorDescriber);
        }

        public static IdentityBuilder AddMongoDbStores<TUser, TRole, TKey>(this IdentityBuilder builder, Action<MongoIdentityOptions> setupDatabaseAction,
            IdentityErrorDescriber identityErrorDescriber = null)
            where TKey : IEquatable<TKey>
            where TUser : MongoUser<TKey>
            where TRole : MongoRole<TKey>
        {
            var dbOptions = new MongoIdentityOptions();
            setupDatabaseAction(dbOptions);

            var migrationCollection = MongoUtil.FromConnectionString<MigrationHistory>(dbOptions, dbOptions.MigrationCollection);
            var migrationUserCollection = MongoUtil.FromConnectionString<MigrationMongoUser<TKey>>(dbOptions, dbOptions.UsersCollection);
            var userCollection = MongoUtil.FromConnectionString<TUser>(dbOptions, dbOptions.UsersCollection);
            var roleCollection = MongoUtil.FromConnectionString<TRole>(dbOptions, dbOptions.RolesCollection);

            Migrator.Apply<MigrationMongoUser<TKey>, TRole, TKey>(migrationCollection, migrationUserCollection, roleCollection);

            builder.Services.AddSingleton(x => userCollection);
            builder.Services.AddSingleton(x => roleCollection);

            // register custom ObjectId TypeConverter
            if (typeof(TKey) == typeof(ObjectId))
            {
                TypeConverterResolver.RegisterTypeConverter<ObjectId, ObjectIdConverter>();
            }

            // Identity Services
            builder.Services.AddTransient<IRoleStore<TRole>>(x => new RoleStore<TRole, TKey>(roleCollection, identityErrorDescriber));
            builder.Services.AddTransient<IUserStore<TUser>>(x => new UserStore<TUser, TRole, TKey>(userCollection, roleCollection, identityErrorDescriber));

            return builder;
        }
    }
}
