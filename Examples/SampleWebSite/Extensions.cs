using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo;

namespace Example.DefaultUser
{
    public static class Extensions2
    {
        public static async Task<MongoIdentityUser> ToUser(this ClaimsPrincipal claim)
        {
            if (claim?.Identity?.Name == null) return null;

            return await MongoIdentityUser.FirstOrDefaultAsync(x => x.UserName == claim.Identity.Name);
        }
    }
}