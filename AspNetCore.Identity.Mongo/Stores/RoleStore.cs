using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Mongolino;

namespace AspNetCore.Identity.Mongo.Stores
{
    public class RoleStore<TRole> : IQueryableRoleStore<TRole> where TRole : MongoIdentityRole
    {
        private readonly Collection<TRole> _collection;

        public RoleStore(Collection<TRole> collection)
        {
            _collection = collection;
        }

        IQueryable<TRole> IQueryableRoleStore<TRole>.Roles => _collection.Queryable();

        async Task<IdentityResult> IRoleStore<TRole>.CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            if (!_collection.Any(x => x.NormalizedName == role.NormalizedName)) await _collection.CreateAsync(role);

            return IdentityResult.Success;
        }

        async Task<IdentityResult> IRoleStore<TRole>.UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            await _collection.UpdateAsync(role);
            return IdentityResult.Success;
        }

        async Task<IdentityResult> IRoleStore<TRole>.DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            await _collection.DeleteAsync(role);
            return IdentityResult.Success;
        }

        Task<string> IRoleStore<TRole>.GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        Task<string> IRoleStore<TRole>.GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        async Task IRoleStore<TRole>.SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            await _collection.UpdateAsync(role, x => x.Name, roleName);
        }

        Task<string> IRoleStore<TRole>.GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        async Task IRoleStore<TRole>.SetNormalizedRoleNameAsync(TRole role, string normalizedName,
            CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            await _collection.UpdateAsync(role, x => x.NormalizedName, normalizedName);
        }

        Task<TRole> IRoleStore<TRole>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return _collection.FirstOrDefaultAsync(x => x.Id == roleId);
        }

        Task<TRole> IRoleStore<TRole>.FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return _collection.FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName);
        }

        void IDisposable.Dispose()
        {
        }
    }
}