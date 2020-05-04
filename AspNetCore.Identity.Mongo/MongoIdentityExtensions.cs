using System;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo
{
	public static class MongoIdentityExtensions
	{
	    public static IdentityBuilder AddIdentityMongoDbProvider(this IServiceCollection services, Action<IdentityOptions> setupIdentityAction = null, Action<MongoIdentityOptions> setupDatabaseAction = null)
			=>
			AddIdentityMongoDbProvider<MongoUserInfo<MongoUser>, MongoUser, MongoRole>(services, setupIdentityAction, setupDatabaseAction);

	    public static IdentityBuilder AddIdentityMongoDbProvider<TUser>(this IServiceCollection services, Action<IdentityOptions> setupIdentityAction = null, Action<MongoIdentityOptions> setupDatabaseAction = null)
			where TUser : MongoUser
			=>
			AddIdentityMongoDbProvider<MongoUserInfo<TUser>, TUser, MongoRole>(services, setupIdentityAction, setupDatabaseAction);

		public static IdentityBuilder AddIdentityMongoDbProvider<TUser, TRole>(this IServiceCollection services, Action<IdentityOptions> setupIdentityAction = null, Action<MongoIdentityOptions> setupDatabaseAction = null)
			where TUser : MongoUser
	        where TRole : MongoRole
			=>
			AddIdentityMongoDbProvider<MongoUserInfo<TUser>, TUser, TRole>(services, setupIdentityAction, setupDatabaseAction);

		public static IdentityBuilder AddIdentityMongoDbProvider<TUserInfo, TUser, TRole>(this IServiceCollection services, Action<IdentityOptions> setupIdentityAction = null, Action<MongoIdentityOptions> setupDatabaseAction = null)
			where TUserInfo : MongoUserInfo<TUser>, new()
			where TUser : MongoUser
	        where TRole : MongoRole
	    {
			if (services == null) throw new ArgumentNullException(nameof(services));

			var dbOptions = new MongoIdentityOptions();

            setupDatabaseAction?.Invoke(dbOptions);

            var builder = setupIdentityAction != null ? services.AddIdentity<TUser, TRole>(setupIdentityAction) : services.AddIdentity<TUser, TRole>();
	        
	        builder
				.AddRoleStore<RoleStore<TRole>>()
				.AddUserStore<UserStore<TUserInfo, TUser, TRole>>()
				.AddUserManager<UserManager<TUser>>()
				.AddRoleManager<RoleManager<TRole>>()
				.AddDefaultTokenProviders();


	        var userCollection =  MongoUtil.FromConnectionString<TUserInfo>(dbOptions.ConnectionString, dbOptions.UsersCollection);
	        var roleCollection = MongoUtil.FromConnectionString<TRole>(dbOptions.ConnectionString, dbOptions.RolesCollection);

	        services.AddSingleton<IMongoCollection<TUserInfo>>(x => userCollection);
	        services.AddSingleton<IMongoCollection<TRole>>(x => roleCollection);

            services.AddTransient<IRoleStore<TRole>>(x => new RoleStore<TRole>(roleCollection));
            services.AddTransient<IUserStore<TUser>>(x => new UserStore<TUserInfo, TUser, TRole>(userCollection, roleCollection, x.GetService<ILookupNormalizer>()));

	        return builder;
	    }
	}
}