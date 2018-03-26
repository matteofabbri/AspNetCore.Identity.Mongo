namespace IntegrationTests
{
	using System.Linq;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Identity.MongoDB;
	using NUnit.Framework;
	using Tests;

	[TestFixture]
	public class UserClaimStoreTests : UserIntegrationTestsBase
	{
		[Test]
		public async Task Create_NewUser_HasNoClaims()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);

			var claims = await manager.GetClaimsAsync(user);

			Expect(claims, Is.Empty);
		}

		[Test]
		public async Task AddClaim_ReturnsClaim()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);

			await manager.AddClaimAsync(user, new Claim("type", "value"));

			var claim = (await manager.GetClaimsAsync(user)).Single();
			Expect(claim.Type, Is.EqualTo("type"));
			Expect(claim.Value, Is.EqualTo("value"));
		}

		[Test]
		public async Task RemoveClaim_RemovesExistingClaim()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);
			await manager.AddClaimAsync(user, new Claim("type", "value"));

			await manager.RemoveClaimAsync(user, new Claim("type", "value"));

			Expect(await manager.GetClaimsAsync(user), Is.Empty);
		}

		[Test]
		public async Task RemoveClaim_DifferentType_DoesNotRemoveClaim()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);
			await manager.AddClaimAsync(user, new Claim("type", "value"));

			await manager.RemoveClaimAsync(user, new Claim("otherType", "value"));

			Expect(await manager.GetClaimsAsync(user), Is.Not.Empty);
		}

		[Test]
		public async Task RemoveClaim_DifferentValue_DoesNotRemoveClaim()
		{
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);
			await manager.AddClaimAsync(user, new Claim("type", "value"));

			await manager.RemoveClaimAsync(user, new Claim("type", "otherValue"));

			Expect(await manager.GetClaimsAsync(user), Is.Not.Empty);
		}

		[Test]
		public async Task ReplaceClaim_Replaces()
		{
			// note: unit tests cover behavior of ReplaceClaim method on IdentityUser
			var user = new IdentityUser {UserName = "bob"};
			var manager = GetUserManager();
			await manager.CreateAsync(user);
			var existingClaim = new Claim("type", "value");
			await manager.AddClaimAsync(user, existingClaim);
			var newClaim = new Claim("newType", "newValue");

			await manager.ReplaceClaimAsync(user, existingClaim, newClaim);

			user.ExpectOnlyHasThisClaim(newClaim);
		}

		[Test]
		public async Task GetUsersForClaim()
		{
			var userWithClaim = new IdentityUser
			{
				UserName = "with"
			};
			var userWithout = new IdentityUser();
			var manager = GetUserManager();
			await manager.CreateAsync(userWithClaim);
			await manager.CreateAsync(userWithout);
			var claim = new Claim("sameType", "sameValue");
			await manager.AddClaimAsync(userWithClaim, claim);

			var matchedUsers = await manager.GetUsersForClaimAsync(claim);

			Expect(matchedUsers.Count, Is.EqualTo(1));
			Expect(matchedUsers.Single().UserName, Is.EqualTo("with"));

			var matchesForWrongType = await manager.GetUsersForClaimAsync(new Claim("wrongType", "sameValue"));
			Expect(matchesForWrongType, Is.Empty, "Users with claim with wrongType should not be returned but were.");

			var matchesForWrongValue = await manager.GetUsersForClaimAsync(new Claim("sameType", "wrongValue"));
			Expect(matchesForWrongValue, Is.Empty, "Users with claim with wrongValue should not be returned but were.");
		}
	}
}