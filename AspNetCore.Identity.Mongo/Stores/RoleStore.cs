using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Collections;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.Mongo.Stores
{
	public class RoleStore<TRole> : IQueryableRoleStore<TRole> where TRole : MongoRole
	{
		private readonly IIdentityRoleCollection<TRole> _collection;

		public RoleStore(IIdentityRoleCollection<TRole> collection)
		{
			_collection = collection;
		}

		IQueryable<TRole> IQueryableRoleStore<TRole>.Roles => _collection.GetAllAsync().Result.AsQueryable();

		async Task<IdentityResult> IRoleStore<TRole>.CreateAsync(TRole role, CancellationToken cancellationToken)
		{
			var found = await _collection.FindByNameAsync(role.NormalizedName);
			if (found == null) await _collection.CreateAsync(role);
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

		async Task<string> IRoleStore<TRole>.GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
		{
			return await Task.FromResult(role.Id);
		}
        
		async Task<string> IRoleStore<TRole>.GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
		{
			return await Task.FromResult(role.Name);
		}

		async Task IRoleStore<TRole>.SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
		{
			role.Name = roleName;
			await _collection.UpdateAsync(role);
		}

		async Task<string> IRoleStore<TRole>.GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
		{
			return await Task.FromResult(role.NormalizedName);
		}

		async Task IRoleStore<TRole>.SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
		{
			role.NormalizedName = normalizedName;
			await _collection.UpdateAsync(role);
		}

		async Task<TRole> IRoleStore<TRole>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
		{
			return await _collection.FindByIdAsync(roleId);
		}

		async Task<TRole> IRoleStore<TRole>.FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
		{
			return await _collection.FindByNameAsync(normalizedRoleName);
		}

		void IDisposable.Dispose()
		{
		}
	}
}