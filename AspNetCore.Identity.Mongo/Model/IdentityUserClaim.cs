using System.Security.Claims;

namespace AspNetCore.Identity.Mongo.Model
{
	public class IdentityUserClaim
	{
		public IdentityUserClaim()
		{
		}

		public IdentityUserClaim(Claim claim)
		{
			Type = claim.Type;
			Value = claim.Value;
		}

		/// <summary>
		///     Gets the subject of the claim.
		///     <returns>
		///         The subject of the claim.
		///     </returns>
		/// </summary>
		public ClaimsIdentity Subject { get; set; }

		/// <summary>
		///     Gets the original issuer of the claim.
		///     <returns>
		///         A name that refers to the original issuer of the claim.
		///     </returns>
		/// </summary>
		public string OriginalIssuer { get; set; }

		/// <summary>
		///     Gets the issuer of the claim.
		///     <returns>A name that refers to the issuer of the claim.</returns>
		/// </summary>
		public string Issuer { get; set; }

		/// <summary>
		///     Gets the value type of the claim.
		///     <returns>The claim value type.</returns>
		/// </summary>
		public string ValueType { get; }

		/// <summary>
		///     Claim type
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		///     Claim value
		/// </summary>
		public string Value { get; set; }

		public Claim ToSecurityClaim()
		{
			return new Claim(Type, Value);
		}
	}
}