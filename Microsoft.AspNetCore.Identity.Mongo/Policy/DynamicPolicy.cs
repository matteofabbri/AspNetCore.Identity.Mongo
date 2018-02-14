using Microsoft.AspNetCore.Authorization;

namespace Maddalena.Identity.Policy
{
    public class DynamicPolicy : IAuthorizationRequirement
    {
        public string Area { get; set; }
    }
}
