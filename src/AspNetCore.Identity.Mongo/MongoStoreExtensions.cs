using AspNetCore.Identity.Mongo.Migrations;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Mongo;
using AspNetCore.Identity.Mongo.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Identity.Mongo
{
    public static class MongoStoreExtensions
    {
        public static IdentityBuilder AddMongoDbStores<TUser>(this IdentityBuilder builder, Action<MongoIdentityOptions> setupDatabaseAction)
            where TUser : MongoUser
        {
            return AddMongoDbStores<TUser, MongoRole, ObjectId>(builder, setupDatabaseAction);
        }

        public static IdentityBuilder AddMongoDbStores<TUser, TRole, TKey>(this IdentityBuilder builder, Action<MongoIdentityOptions> setupDatabaseAction)
            where TKey : IEquatable<TKey>
            where TUser : MongoUser<TKey>
            where TRole : MongoRole<TKey>
        {
            var dbOptions = new MongoIdentityOptions();
            setupDatabaseAction(dbOptions);

            var migrationCollection = MongoUtil.FromConnectionString<MigrationHistory>(dbOptions, dbOptions.MigrationCollection);

            Task.WaitAny(Migrator.Apply(migrationCollection));

            var userCollection = MongoUtil.FromConnectionString<TUser>(dbOptions, dbOptions.UsersCollection);
            var roleCollection = MongoUtil.FromConnectionString<TRole>(dbOptions, dbOptions.RolesCollection);

            builder.Services.AddSingleton(x => userCollection);
            builder.Services.AddSingleton(x => roleCollection);

            // register custom ObjectId TypeConverter
            if (typeof(TKey) == typeof(ObjectId))
            {
                TypeConverterResolver.RegisterTypeConverter<ObjectId, ObjectIdConverter>();
            }

            // Identity Services
            builder.Services.AddTransient<IRoleStore<TRole>>(x => new RoleStore<TRole, TKey>(roleCollection));
            builder.Services.AddTransient<IUserStore<TUser>>(x => new UserStore<TUser, TRole, TKey>(userCollection, new RoleStore<TRole, TKey>(roleCollection), x.GetService<ILookupNormalizer>()));

            return builder;
        }
    }
}
