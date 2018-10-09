using System;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Mongo;
using AspNetCore.Identity.Mongo.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Identity.Mongo
{
	public static class MongoIdentityExtensions
	{
	    public static IServiceCollection AddIdentityMongoDbProvider<TUser>(this IServiceCollection services) where TUser : MongoUser
	    {
	        return AddIdentityMongoDbProvider<TUser, MongoRole>(services, x => { });
	    }

        public static IServiceCollection AddIdentityMongoDbProvider<TUser>(this IServiceCollection services,
	        Action<MongoIdentityOptions> setupDatabaseAction) where TUser : MongoUser
	    {
	        return AddIdentityMongoDbProvider<TUser, MongoRole>(services, setupDatabaseAction);
	    }

        public static IServiceCollection AddIdentityMongoDbProvider<TUser, TRole>(this IServiceCollection services,
			Action<MongoIdentityOptions> setupDatabaseAction) where TUser : MongoUser
			where TRole : MongoRole
        {
            return AddIdentityMongoDbProvider<TUser, TRole>(services, x => { }, setupDatabaseAction);
        }

		public static IServiceCollection AddIdentityMongoDbProvider<TUser, TRole>(this IServiceCollection services,
			Action<IdentityOptions> setupIdentityAction, Action<MongoIdentityOptions> setupDatabaseAction) where TUser : MongoUser
			where TRole : MongoRole
		{
			services.AddIdentity<TUser, TRole>(setupIdentityAction ?? (x => { }))
				.AddRoleStore<RoleStore<TRole>>()
				.AddUserStore<UserStore<TUser, TRole>>()
				.AddDefaultTokenProviders();

			var dbOptions = new MongoIdentityOptions();
			setupDatabaseAction(dbOptions);

			var userCollection = new IdentityUserCollection<TUser>(dbOptions.ConnectionString, dbOptions.UsersCollection);
			var roleCollection = new IdentityRoleCollection<TRole>(dbOptions.ConnectionString, dbOptions.RolesCollection);

            services.AddTransient<IIdentityUserCollection<TUser>>(x => userCollection);
            services.AddTransient<IIdentityRoleCollection<TRole>>(x => roleCollection);


            // Identity Services
            services.AddTransient<IUserStore<TUser>>(x => new UserStore<TUser, TRole>(userCollection, roleCollection));
			services.AddTransient<IRoleStore<TRole>>(x => new RoleStore<TRole>(roleCollection));
			return services;
		}
	}
}