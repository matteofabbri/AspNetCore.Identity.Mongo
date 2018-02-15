using Microsoft.AspNetCore.Authorization;

namespace Microsoft.AspNetCore.Identity.Mongo.DynamicPolicy
{
    public class DynamicPolicy : IAuthorizationRequirement
    {
        public string Area { get; set; }
    }
}
