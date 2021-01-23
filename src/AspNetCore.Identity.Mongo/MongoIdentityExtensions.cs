using System;
using System.ComponentModel;
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
            return AddIdentityMongoDbProvider<TUser, MongoRole<TKey>, TKey>(services, x => { });
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

        public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TRole, TKey>(this IServiceCollection services,
            Action<MongoIdentityOptions> setupDatabaseAction)
            where TKey : IEquatable<TKey>
            where TUser : MongoUser<TKey>
            where TRole : MongoRole<TKey>
        {
            return AddIdentityMongoDbProvider<TUser, TRole, TKey>(services, x => { }, setupDatabaseAction);
        }

        public static IdentityBuilder AddIdentityMongoDbProvider(this IServiceCollection services,
            Action<IdentityOptions> setupIdentityAction, Action<MongoIdentityOptions> setupDatabaseAction)
        {
            return AddIdentityMongoDbProvider<MongoUser, MongoRole, ObjectId>(services, setupIdentityAction, setupDatabaseAction);
        }

        public static IdentityBuilder AddIdentityMongoDbProvider<TUser>(this IServiceCollection services,
            Action<IdentityOptions> setupIdentityAction, Action<MongoIdentityOptions> setupDatabaseAction) where TUser : MongoUser
        {
            return AddIdentityMongoDbProvider<TUser, MongoRole, ObjectId>(services, setupIdentityAction, setupDatabaseAction);
        }

        public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TRole, TKey>(this IServiceCollection services,
            Action<IdentityOptions> setupIdentityAction, Action<MongoIdentityOptions> setupDatabaseAction)
            where TKey : IEquatable<TKey>
            where TUser : MongoUser<TKey>
            where TRole : MongoRole<TKey>
        {
            var dbOptions = new MongoIdentityOptions();
            setupDatabaseAction(dbOptions);

            var builder = services.AddIdentity<TUser, TRole>(setupIdentityAction ?? (x => { }));

            builder.AddRoleStore<RoleStore<TRole, TKey>>()
            .AddUserStore<UserStore<TUser, TRole, TKey>>()
            .AddUserManager<UserManager<TUser>>()
            .AddRoleManager<RoleManager<TRole>>()
            .AddDefaultTokenProviders();

            var migrationCollection = MongoUtil.FromConnectionString<MigrationHistory>(dbOptions.ConnectionString, dbOptions.MigrationCollection);

            Task.WaitAny(Migrator.Apply(migrationCollection));

            var userCollection = MongoUtil.FromConnectionString<TUser>(dbOptions.ConnectionString, dbOptions.UsersCollection);
            var roleCollection = MongoUtil.FromConnectionString<TRole>(dbOptions.ConnectionString, dbOptions.RolesCollection);

            services.AddSingleton(x => userCollection);
            services.AddSingleton(x => roleCollection);

            // register custom ObjectId TypeConverter
            if (typeof(TKey) == typeof(ObjectId))
            {
                RegisterTypeConverter<ObjectId, ObjectIdConverter>();
            }

            // Identity Services
            services.AddTransient<IRoleStore<TRole>>(x => new RoleStore<TRole, TKey>(roleCollection));
            services.AddTransient<IUserStore<TUser>>(x => new UserStore<TUser, TRole, TKey>(userCollection, new RoleStore<TRole, TKey>(roleCollection), x.GetService<ILookupNormalizer>()));

            return builder;
        }

        private static void RegisterTypeConverter<T, TC>() where TC : TypeConverter
        {
            Attribute[] attr = new Attribute[1];
            TypeConverterAttribute vConv = new TypeConverterAttribute(typeof(TC));
            attr[0] = vConv;
            TypeDescriptor.AddAttributes(typeof(T), attr);
        }
    }
}