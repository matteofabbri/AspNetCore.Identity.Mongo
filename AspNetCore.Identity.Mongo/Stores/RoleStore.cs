using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Mongolino;

namespace AspNetCore.Identity.Mongo.Stores
{
    public class RoleStore<T> : UserStore<T>, IQueryableRoleStore<MongoIdentityRole> where T : DBObject<T>, IMongoIdentityUser
    {
        IQueryable<MongoIdentityRole> IQueryableRoleStore<MongoIdentityRole>.Roles => MongoIdentityRole.Queryable();

        async Task<IdentityResult> IRoleStore<MongoIdentityRole>.CreateAsync(MongoIdentityRole role, CancellationToken cancellationToken)
        {
            if (!MongoIdentityRole.Any(x => x.NormalizedName == role.NormalizedName)) await MongoIdentityRole.CreateAsync(role);

            return IdentityResult.Success;
        }

        async Task<IdentityResult> IRoleStore<MongoIdentityRole>.UpdateAsync(MongoIdentityRole role, CancellationToken cancellationToken)
        {
            await MongoIdentityRole.UpdateAsync(role);
            return IdentityResult.Success;
        }

        async Task<IdentityResult> IRoleStore<MongoIdentityRole>.DeleteAsync(MongoIdentityRole role, CancellationToken cancellationToken)
        {
            await MongoIdentityRole.DeleteAsync(role);
            await RoleMembership.DeleteAsync(x => x.RoleId == role.Id);

            return IdentityResult.Success;
        }

        Task<string> IRoleStore<MongoIdentityRole>.GetRoleIdAsync(MongoIdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        Task<string> IRoleStore<MongoIdentityRole>.GetRoleNameAsync(MongoIdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        async Task IRoleStore<MongoIdentityRole>.SetRoleNameAsync(MongoIdentityRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            await MongoIdentityRole.UpdateAsync(role, x => x.Name, roleName);
        }

        Task<string> IRoleStore<MongoIdentityRole>.GetNormalizedRoleNameAsync(MongoIdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        async Task IRoleStore<MongoIdentityRole>.SetNormalizedRoleNameAsync(MongoIdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            await MongoIdentityRole.UpdateAsync(role, x => x.NormalizedName, normalizedName);
        }

        Task<MongoIdentityRole> IRoleStore<MongoIdentityRole>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return MongoIdentityRole.FirstOrDefaultAsync(x => x.Id == roleId);
        }

        Task<MongoIdentityRole> IRoleStore<MongoIdentityRole>.FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return MongoIdentityRole.FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName);
        }

        void IDisposable.Dispose()
        {

        }
    }
}
