using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.Mongo.Stores;
using Microsoft.Extensions.DependencyInjection;
using Mongolino;

namespace Microsoft.AspNetCore.Identity.Mongo
{
    public static class Extensions
    {
        public static void AddMongoIdentityProvider<TUser>(this IServiceCollection services, Action<IdentityOptions> setupAction) where TUser : DBObject<TUser>, IMongoIdentityUser
        {
            services.AddIdentity<TUser, MongoIdentityRole>(setupAction)
                .AddRoleStore<RoleStore<TUser>>()
                .AddUserStore<UserStore<TUser>>()
                .AddDefaultTokenProviders();

            // Identity Services
            services.AddTransient<IUserStore<TUser>, UserStore<TUser>>();
            services.AddTransient<IRoleStore<MongoIdentityRole>, RoleStore<TUser>>();
        }

        public static IServiceCollection AddMongoIdentityProvider(this IServiceCollection services, Action<IdentityOptions> setupAction)
        {
            services.AddIdentity<MongoIdentityUser, MongoIdentityRole>(setupAction)
                .AddRoleStore<RoleStore<MongoIdentityUser>>()
                .AddUserStore<UserStore<MongoIdentityUser>>()
                .AddDefaultTokenProviders();

            // Identity Services
            services.AddTransient<IUserStore<MongoIdentityUser>, UserStore<MongoIdentityUser>>();
            services.AddTransient<IRoleStore<MongoIdentityRole>, RoleStore<MongoIdentityUser>>();
            return services;
        }
    }
}
