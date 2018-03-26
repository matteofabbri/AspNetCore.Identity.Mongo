namespace IntegrationTests
{
	using System.Linq;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.AspNetCore.Identity.MongoDB;
	using NUnit.Framework;

	// todo low - validate all tests work
	[TestFixture]
	public class UserLoginStoreTests : UserIntegrationTestsBase
	{
		[Test]
		public async Task AddLogin_NewLogin_Adds()
		{
			var manager = GetUserManager();
			var login = new UserLoginInfo("provider", "key", "name");
			var user = new IdentityUser {UserName = "bob"};
			await manager.CreateAsync(user);

			await manager.AddLoginAsync(user, login);

			var savedLogin = Users.FindAll().Single().Logins.Single();
			Expect(savedLogin.LoginProvider, Is.EqualTo("provider"));
			Expect(savedLogin.ProviderKey, Is.EqualTo("key"));
			Expect(savedLogin.ProviderDisplayName, Is.EqualTo("name"));
		}

		[Test]
		public async Task RemoveLogin_NewLogin_Removes()
		{
			var manager = GetUserManager();
			var login = new UserLoginInfo("provider", "key", "name");
			var user = new IdentityUser {UserName = "bob"};
			await manager.CreateAsync(user);
			await manager.AddLoginAsync(user, login);

			await manager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);

			var savedUser = Users.FindAll().Single();
			Expect(savedUser.Logins, Is.Empty);
		}

		[Test]
		public async Task GetLogins_OneLogin_ReturnsLogin()
		{
			var manager = GetUserManager();
			var login = new UserLoginInfo("provider", "key", "name");
			var user = new IdentityUser {UserName = "bob"};
			await manager.CreateAsync(user);
			await manager.AddLoginAsync(user, login);

			var logins = await manager.GetLoginsAsync(user);

			var savedLogin = logins.Single();
			Expect(savedLogin.LoginProvider, Is.EqualTo("provider"));
			Expect(savedLogin.ProviderKey, Is.EqualTo("key"));
			Expect(savedLogin.ProviderDisplayName, Is.EqualTo("name"));
		}

		[Test]
		public async Task Find_UserWithLogin_FindsUser()
		{
			var manager = GetUserManager();
			var login = new UserLoginInfo("provider", "key", "name");
			var user = new IdentityUser {UserName = "bob"};
			await manager.CreateAsync(user);
			await manager.AddLoginAsync(user, login);

			var findUser = await manager.FindByLoginAsync(login.LoginProvider, login.ProviderKey);

			Expect(findUser, Is.Not.Null);
		}

		[Test]
		public async Task Find_UserWithDifferentKey_DoesNotFindUser()
		{
			var manager = GetUserManager();
			var login = new UserLoginInfo("provider", "key", "name");
			var user = new IdentityUser {UserName = "bob"};
			await manager.CreateAsync(user);
			await manager.AddLoginAsync(user, login);

			var findUser = await manager.FindByLoginAsync("provider", "otherkey");

			Expect(findUser, Is.Null);
		}

		[Test]
		public async Task Find_UserWithDifferentProvider_DoesNotFindUser()
		{
			var manager = GetUserManager();
			var login = new UserLoginInfo("provider", "key", "name");
			var user = new IdentityUser {UserName = "bob"};
			await manager.CreateAsync(user);
			await manager.AddLoginAsync(user, login);

			var findUser = await manager.FindByLoginAsync("otherprovider", "key");

			Expect(findUser, Is.Null);
		}
	}
}