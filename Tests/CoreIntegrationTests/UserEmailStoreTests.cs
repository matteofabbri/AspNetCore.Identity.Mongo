namespace IntegrationTests
{
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Identity.MongoDB;
	using NUnit.Framework;

	[TestFixture]
	public class UserEmailStoreTests : UserIntegrationTestsBase
	{
		[Test]
		public async Task Create_NewUser_HasNoEmail()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);

			var email = await manager.GetEmailAsync(user);

			Expect(email, Is.Null);
		}

		[Test]
		public async Task SetEmail_SetsEmail()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);

			await manager.SetEmailAsync(user, "email");

			Expect(await manager.GetEmailAsync(user), Is.EqualTo("email"));
		}

		[Test]
		public async Task FindUserByEmail_ReturnsUser()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);
			Expect(await manager.FindByEmailAsync("email"), Is.Null);

			await manager.SetEmailAsync(user, "email");

			Expect(await manager.FindByEmailAsync("email"), Is.Not.Null);
		}

		[Test]
		public async Task Create_NewUser_IsNotEmailConfirmed()
		{
			var manager = GetUserManager();
			var user = new IdentityUser {UserName = "bob"};
			await manager.CreateAsync(user);

			var isConfirmed = await manager.IsEmailConfirmedAsync(user);

			Expect(isConfirmed, Is.False);
		}

		[Test]
		public async Task SetEmailConfirmed_IsConfirmed()
		{
			var manager = GetUserManager();
			var user = new IdentityUser {UserName = "bob"};
			await manager.CreateAsync(user);
			var token = await manager.GenerateEmailConfirmationTokenAsync(user);

			await manager.ConfirmEmailAsync(user, token);

			var isConfirmed = await manager.IsEmailConfirmedAsync(user);
			Expect(isConfirmed);
		}

		[Test]
		public async Task ChangeEmail_AfterConfirmedOriginalEmail_NotEmailConfirmed()
		{
			var manager = GetUserManager();
			var user = new IdentityUser {UserName = "bob"};
			await manager.CreateAsync(user);
			var token = await manager.GenerateEmailConfirmationTokenAsync(user);
			await manager.ConfirmEmailAsync(user, token);

			await manager.SetEmailAsync(user, "new@email.com");

			var isConfirmed = await manager.IsEmailConfirmedAsync(user);
			Expect(isConfirmed, Is.False);
		}
	}
}