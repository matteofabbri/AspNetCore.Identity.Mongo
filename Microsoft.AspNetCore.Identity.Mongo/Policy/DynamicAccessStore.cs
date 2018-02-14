using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mongolino;

namespace Maddalena.Identity.Policy
{
    public class DynamicAccessStore : IDynamicAccessStore
    {
        public class AreaAccess : DBObject<AreaAccess>
        {
            public string Area { get; set; }

            public List<string> Roles { get; set; }
        }

        public async Task<bool> IsAuthorizedAsync(ClaimsPrincipal user, string area)
        {
            var ac = await AreaAccess.FirstOrDefaultAsync(x => x.Area == area);

            return ac != null && ac.Roles.Any(user.IsInRole);
        }
    }
}
