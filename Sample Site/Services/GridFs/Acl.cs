using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SampleSite.GridFs
{
    public class Acl
    {
        public bool Public { get; set; }

        public string Owner { get; set; } = "<SYSTEM>";

        public List<string> AllowUsers { get; set; } = new List<string>();

        public List<string> AllowRoles { get; set; } = new List<string>();

        public List<string> DenyUsers { get; set; } = new List<string>();

        public List<string> DenyRoles { get; set; } = new List<string>();

        public bool IsAllowed(ClaimsPrincipal claim)
        {
            if (Public || claim.IsInRole("admin") || Owner.Equals(claim.Identity.Name)) return true;

            if (DenyUsers.Contains(claim.Identity.Name)) return false;

            if (AllowUsers.Contains(claim.Identity.Name)) return true;

            return !DenyRoles.Any(claim.IsInRole) && AllowRoles.Any(claim.IsInRole);
        }
    }
}