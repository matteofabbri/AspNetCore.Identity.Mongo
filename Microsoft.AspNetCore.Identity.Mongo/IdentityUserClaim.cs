using System.Security.Claims;
using Mongolino;

namespace Maddalena.Identity
{
    public class IdentityUserClaim : DBObject<IdentityUserClaim>
    {
        public IdentityUserClaim()
        {
        }

        public IdentityUserClaim(Claim claim)
        {
            Type = claim.Type;
            Value = claim.Value;
        }

        public IdentityUserClaim(string id, Claim claim):this(claim)
        {
            Id = id;
        }


        /// <summary>
        /// Claim type
        /// </summary>
        public string UserId { get; set; }

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
