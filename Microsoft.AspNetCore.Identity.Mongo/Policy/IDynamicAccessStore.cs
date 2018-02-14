using System.Security.Claims;
using System.Threading.Tasks;

namespace Maddalena.Identity.Policy
{
    public interface IDynamicAccessStore
    {
        Task<bool> IsAuthorizedAsync(ClaimsPrincipal user, string area);
    }
}
