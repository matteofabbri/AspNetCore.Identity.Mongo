namespace IntegrationTests
{
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Identity.MongoDB;
	using NUnit.Framework;

	// todo low - validate all tests work
	[TestFixture]
	public class UserTwoFactorStoreTests : UserIntegrationTestsBase
	{
		[Test]
		public async Task SetTwoFactorEnabled()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);

			await manager.SetTwoFactorEnabledAsync(user, true);

			Expect(await manager.GetTwoFactorEnabledAsync(user));
		}

		[Test]
		public async Task ClearTwoFactorEnabled_PreviouslyEnabled_NotEnabled()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);
			await manager.SetTwoFactorEnabledAsync(user, true);

			await manager.SetTwoFactorEnabledAsync(user, false);

			Expect(await manager.GetTwoFactorEnabledAsync(user), Is.False);
		}
	}
}