namespace Tests
{
    using System.Security.Claims;
    using NUnit.Framework;

	public static class TestExtensions
	{
		public static void ExpectOnlyHasThisClaim(this IdentityUser user, Claim expectedClaim)
		{
			AssertionHelper.Expect(user.Claims.Count, Is.EqualTo(1));
			var actualClaim = user.Claims.Single();
			AssertionHelper.Expect(actualClaim.Type, Is.EqualTo(expectedClaim.Type));
			AssertionHelper.Expect(actualClaim.Value, Is.EqualTo(expectedClaim.Value));
		}
	}
}