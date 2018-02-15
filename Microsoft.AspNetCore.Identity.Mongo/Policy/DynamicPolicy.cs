using Microsoft.AspNetCore.Authorization;

namespace Microsoft.AspNetCore.Identity.Mongo.Policy
{
    public class DynamicPolicy : IAuthorizationRequirement
    {
        public string Area { get; set; }
    }
}
