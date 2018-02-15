using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity.Mongo.Stores
{
    public class RoleStore : UserStore, IQueryableRoleStore<ApplicationRole>
    {
        IQueryable<ApplicationRole> IQueryableRoleStore<ApplicationRole>.Roles => ApplicationRole.Queryable();

        async Task<IdentityResult> IRoleStore<ApplicationRole>.CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            if (!ApplicationRole.Any(x => x.NormalizedName == role.NormalizedName)) await ApplicationRole.CreateAsync(role);

            return IdentityResult.Success;
        }

        async Task<IdentityResult> IRoleStore<ApplicationRole>.UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            await ApplicationRole.UpdateAsync(role);
            return IdentityResult.Success;
        }

        async Task<IdentityResult> IRoleStore<ApplicationRole>.DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            await ApplicationRole.DeleteAsync(role);
            await RoleMembership.DeleteAsync(x => x.RoleId == role.Id);

            return IdentityResult.Success;
        }

        Task<string> IRoleStore<ApplicationRole>.GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        Task<string> IRoleStore<ApplicationRole>.GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        async Task IRoleStore<ApplicationRole>.SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            await ApplicationRole.UpdateAsync(role, x => x.Name, roleName);
        }

        Task<string> IRoleStore<ApplicationRole>.GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        async Task IRoleStore<ApplicationRole>.SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            await ApplicationRole.UpdateAsync(role, x => x.NormalizedName, normalizedName);
        }

        Task<ApplicationRole> IRoleStore<ApplicationRole>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return ApplicationRole.FirstOrDefaultAsync(x => x.Id == roleId);
        }

        Task<ApplicationRole> IRoleStore<ApplicationRole>.FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return ApplicationRole.FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName);
        }

        void IDisposable.Dispose()
        {

        }
    }
}
