using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Maddalena.Identity.Policy
{
    public class DynamicPolicyHandler<T> : AuthorizationHandler<DynamicPolicy> where T: IDynamicAccessStore, new()
    {
        readonly IDynamicAccessStore store = new T();

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DynamicPolicy requirement)
        {
            if (await store.IsAuthorizedAsync(context.User, requirement.Area))
            {
                context.Succeed(requirement);
                return;
            }
            context.Fail();
        }
    }
}
