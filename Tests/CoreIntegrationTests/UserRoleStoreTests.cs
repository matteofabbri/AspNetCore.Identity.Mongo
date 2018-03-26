namespace IntegrationTests
{
	using System.Linq;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Identity.MongoDB;
	using NUnit.Framework;

	// todo low - validate all tests work
	[TestFixture]
	public class UserRoleStoreTests : UserIntegrationTestsBase
	{
		[Test]
		public async Task GetRoles_UserHasNoRoles_ReturnsNoRoles()
		{
			var manager = GetUserManager();
			var user = new IdentityUser {UserName = "bob"};
			await manager.CreateAsync(user);

			var roles = await manager.GetRolesAsync(user);

			Expect(roles, Is.Empty);
		}

		[Test]
		public async Task AddRole_Adds()
		{
			var manager = GetUserManager();
			var user = new IdentityUser {UserName = "bob"};
			await manager.CreateAsync(user);

			await manager.AddToRoleAsync(user, "role");

			var savedUser = Users.FindAll().Single();
			// note: addToRole now passes a normalized role name
			Expect(savedUser.Roles, Is.EquivalentTo(new[] {"ROLE"}));
			Expect(await manager.IsInRoleAsync(user, "role"), Is.True);
		}

		[Test]
		public async Task RemoveRole_Removes()
		{
			var manager = GetUserManager();
			var user = new IdentityUser {UserName = "bob"};
			await manager.CreateAsync(user);
			await manager.AddToRoleAsync(user, "role");

			await manager.RemoveFromRoleAsync(user, "role");

			var savedUser = Users.FindAll().Single();
			Expect(savedUser.Roles, Is.Empty);
			Expect(await manager.IsInRoleAsync(user, "role"), Is.False);
		}

		[Test]
		public async Task GetUsersInRole_FiltersOnRole()
		{
			var roleA = "roleA";
			var roleB = "roleB";
			var userInA = new IdentityUser {UserName = "nameA"};
			var userInB = new IdentityUser {UserName = "nameB"};
			var manager = GetUserManager();
			await manager.CreateAsync(userInA);
			await manager.CreateAsync(userInB);
			await manager.AddToRoleAsync(userInA, roleA);
			await manager.AddToRoleAsync(userInB, roleB);

			var matchedUsers = await manager.GetUsersInRoleAsync("roleA");

			Expect(matchedUsers.Count, Is.EqualTo(1));
			Expect(matchedUsers.First().UserName, Is.EqualTo("nameA"));
		}
	}
}