using System;
using AspNetCore.Identity.Mongo.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Mongolino;

namespace AspNetCore.Identity.Mongo
{
    public static class Extensions
    {
        public static IServiceCollection AddMongoIdentityProvider(this IServiceCollection services)
        {
            return AddMongoIdentityProvider<MongoIdentityUser, MongoIdentityRole>(services, null, null);
        }

        public static IServiceCollection AddMongoIdentityProvider<TUser>(this IServiceCollection services) where TUser : MongoIdentityUser
        {
            return AddMongoIdentityProvider<TUser, MongoIdentityRole>(services, null, null);
        }

        public static IServiceCollection AddMongoIdentityProvider<TUser, TRole>(this IServiceCollection services) where TUser : MongoIdentityUser
            where TRole : MongoIdentityRole
        {
            return AddMongoIdentityProvider<TUser, TRole>(services, null, null);
        }

        public static IServiceCollection AddMongoIdentityProvider<TUser, TRole>(this IServiceCollection services,
            Action<IdentityOptions> setupAction) where TUser : MongoIdentityUser
            where TRole : MongoIdentityRole
        {
            return AddMongoIdentityProvider<TUser, TRole>(services, null, setupAction);
        }

        public static IServiceCollection AddMongoIdentityProvider<TUser,TRole>(this IServiceCollection services, string connectionString, Action<IdentityOptions> setupAction) where TUser : MongoIdentityUser
                                                                                                                                                                 where TRole : MongoIdentityRole
        {
            services.AddIdentity<TUser, MongoIdentityRole>(setupAction ?? (x=>{}))
                .AddRoleStore<RoleStore<TRole>>()
                .AddUserStore<UserStore<TUser>>()
                .AddDefaultTokenProviders();

            var userCollection = new Collection<TUser>(connectionString, "users");
            var roleCollection = new Collection<TRole>(connectionString, "roles");


            // Identity Services
            services.AddTransient<IUserStore<TUser>>(x => new UserStore<TUser>(userCollection));
            services.AddTransient<IRoleStore<TRole>>(x => new RoleStore<TRole>(roleCollection));

            return services;
        }
    }
}
