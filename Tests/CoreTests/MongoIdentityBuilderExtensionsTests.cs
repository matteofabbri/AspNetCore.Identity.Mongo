namespace CoreTests
{
	using Microsoft.AspNetCore.Identity;
	using Microsoft.AspNetCore.Identity.MongoDB;
	using Microsoft.Extensions.DependencyInjection;
	using NUnit.Framework;

	[TestFixture]
	public class MongoIdentityBuilderExtensionsTests : AssertionHelper
	{
		private const string FakeConnectionStringWithDatabase = "mongodb://fakehost:27017/database";

		[Test]
		public void AddMongoStores_WithDefaultTypes_ResolvesStoresAndManagers()
		{
			var services = new ServiceCollection();
			services.AddIdentityWithMongoStores(FakeConnectionStringWithDatabase);
			// note: UserManager and RoleManager use logging
			services.AddLogging();

			var provider = services.BuildServiceProvider();
			var resolvedUserStore = provider.GetService<IUserStore<IdentityUser>>();
			Expect(resolvedUserStore, Is.Not.Null, "User store did not resolve");

			var resolvedRoleStore = provider.GetService<IRoleStore<IdentityRole>>();
			Expect(resolvedRoleStore, Is.Not.Null, "Role store did not resolve");

			var resolvedUserManager = provider.GetService<UserManager<IdentityUser>>();
			Expect(resolvedUserManager, Is.Not.Null, "User manager did not resolve");

			var resolvedRoleManager = provider.GetService<RoleManager<IdentityRole>>();
			Expect(resolvedRoleManager, Is.Not.Null, "Role manager did not resolve");
		}

		protected class CustomUser : IdentityUser
		{
		}

		protected class CustomRole : IdentityRole
		{
		}

		[Test]
		public void AddMongoStores_WithCustomTypes_ThisShouldLookReasonableForUsers()
		{
			// this test is just to make sure I consider the interface for using custom types
			// so that it's not a horrible experience even though it should be rarely used
			var services = new ServiceCollection();
			services.AddIdentityWithMongoStoresUsingCustomTypes<CustomUser, CustomRole>(FakeConnectionStringWithDatabase);
			services.AddLogging();

			var provider = services.BuildServiceProvider();
			var resolvedUserStore = provider.GetService<IUserStore<CustomUser>>();
			Expect(resolvedUserStore, Is.Not.Null, "User store did not resolve");

			var resolvedRoleStore = provider.GetService<IRoleStore<CustomRole>>();
			Expect(resolvedRoleStore, Is.Not.Null, "Role store did not resolve");

			var resolvedUserManager = provider.GetService<UserManager<CustomUser>>();
			Expect(resolvedUserManager, Is.Not.Null, "User manager did not resolve");

			var resolvedRoleManager = provider.GetService<RoleManager<CustomRole>>();
			Expect(resolvedRoleManager, Is.Not.Null, "Role manager did not resolve");
		}

		[Test]
		public void AddMongoStores_ConnectionStringWithoutDatabase_Throws()
		{
			var connectionStringWithoutDatabase = "mongodb://fakehost";

			TestDelegate addMongoStores = () => new ServiceCollection()
				.AddIdentity<IdentityUser, IdentityRole>()
				.RegisterMongoStores<IdentityUser, IdentityRole>(connectionStringWithoutDatabase);

			Expect(addMongoStores, Throws.Exception
				.With.Message.Contains("Your connection string must contain a database name"));
		}

		protected class WrongUser : IdentityUser
		{
		}

		protected class WrongRole : IdentityRole
		{
		}

		[Test]
		public void AddMongoStores_MismatchedTypes_ThrowsWarningToHelpUsers()
		{
			Expect(() => new ServiceCollection()
					.AddIdentity<IdentityUser, IdentityRole>()
					.RegisterMongoStores<WrongUser, IdentityRole>(FakeConnectionStringWithDatabase),
				Throws.Exception.With.Message
					.EqualTo("User type passed to RegisterMongoStores must match user type passed to AddIdentity. You passed Microsoft.AspNetCore.Identity.MongoDB.IdentityUser to AddIdentity and CoreTests.MongoIdentityBuilderExtensionsTests+WrongUser to RegisterMongoStores, these do not match.")
			);

			Expect(() => new ServiceCollection()
					.AddIdentity<IdentityUser, IdentityRole>()
					.RegisterMongoStores<IdentityUser, WrongRole>(FakeConnectionStringWithDatabase),
				Throws.Exception.With.Message
					.EqualTo("Role type passed to RegisterMongoStores must match role type passed to AddIdentity. You passed Microsoft.AspNetCore.Identity.MongoDB.IdentityRole to AddIdentity and CoreTests.MongoIdentityBuilderExtensionsTests+WrongRole to RegisterMongoStores, these do not match.")
			);
		}
	}
}