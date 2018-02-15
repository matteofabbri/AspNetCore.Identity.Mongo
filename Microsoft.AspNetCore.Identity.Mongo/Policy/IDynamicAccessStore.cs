using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity.Mongo.Policy
{
    public interface IDynamicAccessStore
    {
        Task<bool> IsAuthorizedAsync(ClaimsPrincipal user, string area);
    }
}
