using System.Security.Claims;

namespace AspNetCore.Identity.Mongo
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

        //
        // Summary:
        //     Gets the subject of the claim.
        //
        // Returns:
        //     The subject of the claim.
        public ClaimsIdentity Subject { get; set; }

        //
        // Summary:
        //     Gets the original issuer of the claim.
        //
        // Returns:
        //     A name that refers to the original issuer of the claim.
        public string OriginalIssuer { get; set; }

        //
        // Summary:
        //     Gets the issuer of the claim.
        //
        // Returns:
        //     A name that refers to the issuer of the claim.
        public string Issuer { get; set; }

        //
        // Summary:
        //     Gets the value type of the claim.
        //
        // Returns:
        //     The claim value type.
        public string ValueType { get; }

        /// <summary>
        /// Claim type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Claim value
        /// </summary>
        public string Value { get; set; }

        public Claim ToSecurityClaim()
        {
            return new Claim(Type, Value);
        }
    }
}
