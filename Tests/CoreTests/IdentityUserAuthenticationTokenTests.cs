namespace CoreTests
{
	using Microsoft.AspNetCore.Identity.MongoDB;
	using NUnit.Framework;

	public class IdentityUserAuthenticationTokenTests : AssertionHelper
	{
		[Test]
		public void GetToken_NoTokens_ReturnsNull()
		{
			var user = new IdentityUser();

			var value = user.GetTokenValue("loginProvider", "tokenName");

			Expect(value, Is.Null);
		}

		[Test]
		public void GetToken_WithToken_ReturnsValueIfProviderAndNameMatch()
		{
			var user = new IdentityUser();
			user.SetToken("loginProvider", "tokenName", "tokenValue");

			Expect(user.GetTokenValue("loginProvider", "tokenName"),
				Is.EqualTo("tokenValue"), "GetToken should match on both provider and name, but isn't");

			Expect(user.GetTokenValue("wrongProvider", "tokenName"),
				Is.Null, "GetToken should match on loginProvider, but isn't");

			Expect(user.GetTokenValue("loginProvider", "wrongName"),
				Is.Null, "GetToken should match on tokenName, but isn't");
		}

		[Test]
		public void RemoveToken_OnlyRemovesIfNameAndProviderMatch()
		{
			var user = new IdentityUser();
			user.SetToken("loginProvider", "tokenName", "tokenValue");

			user.RemoveToken("wrongProvider", "tokenName");
			Expect(user.GetTokenValue("loginProvider", "tokenName"),
				Is.EqualTo("tokenValue"), "RemoveToken should match on loginProvider, but isn't");

			user.RemoveToken("loginProvider", "wrongName");
			Expect(user.GetTokenValue("loginProvider", "tokenName"),
				Is.EqualTo("tokenValue"), "RemoveToken should match on tokenName, but isn't");

			user.RemoveToken("loginProvider", "tokenName");
			Expect(user.GetTokenValue("loginProvider", "tokenName"),
				Is.Null, "RemoveToken should match on both loginProvider and tokenName, but isn't");
		}

		[Test]
		public void SetToken_ReplacesValue()
		{
			var user = new IdentityUser();
			user.SetToken("loginProvider", "tokenName", "tokenValue");

			user.SetToken("loginProvider", "tokenName", "updatedValue");

			Expect(user.Tokens.Count, Is.EqualTo(1));
			Expect(user.GetTokenValue("loginProvider", "tokenName"),
				Is.EqualTo("updatedValue"));
		}
	}
}