using IdentityServer4.Services;
using System.Threading.Tasks;

namespace IdentityServer4Demo
{
    // allows arbitrary CORS origins - only for demo purposes. NEVER USE IN PRODUCTION
    public class DemoCorsPolicy : ICorsPolicyService
    {
        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            return Task.FromResult(true);
        }
    }
}
