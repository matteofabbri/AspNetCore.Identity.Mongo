namespace IntegrationTests
{
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Identity.MongoDB;
	using NUnit.Framework;

	// todo low - validate all tests work
	[TestFixture]
	public class UserPhoneNumberStoreTests : UserIntegrationTestsBase
	{
		private const string PhoneNumber = "1234567890";

		[Test]
		public async Task SetPhoneNumber_StoresPhoneNumber()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);

			await manager.SetPhoneNumberAsync(user, PhoneNumber);

			Expect(await manager.GetPhoneNumberAsync(user), Is.EqualTo(PhoneNumber));
		}

		[Test]
		public async Task ConfirmPhoneNumber_StoresPhoneNumberConfirmed()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);
			var token = await manager.GenerateChangePhoneNumberTokenAsync(user, PhoneNumber);

			await manager.ChangePhoneNumberAsync(user, PhoneNumber, token);

			Expect(await manager.IsPhoneNumberConfirmedAsync(user));
		}

		[Test]
		public async Task ChangePhoneNumber_OriginalPhoneNumberWasConfirmed_NotPhoneNumberConfirmed()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);
			var token = await manager.GenerateChangePhoneNumberTokenAsync(user, PhoneNumber);
			await manager.ChangePhoneNumberAsync(user, PhoneNumber, token);

			await manager.SetPhoneNumberAsync(user, PhoneNumber);

			Expect(await manager.IsPhoneNumberConfirmedAsync(user), Is.False);
		}
	}
}