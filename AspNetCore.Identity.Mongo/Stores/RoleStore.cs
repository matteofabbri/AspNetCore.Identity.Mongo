using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using Maddalena.Mongo;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo.Stores
{
	public class RoleStore<TRole> : IQueryableRoleStore<TRole> where TRole : MongoRole
	{
		private readonly IMongoCollection<TRole> _collection;

		public RoleStore(IMongoCollection<TRole> collection)
		{
			_collection = collection;
		}

		IQueryable<TRole> IQueryableRoleStore<TRole>.Roles
        {
            get
            {
                var task = _collection.All();
                Task.WaitAny(task);
                return task.Result.AsQueryable();
            }
        }

        async Task<IdentityResult> IRoleStore<TRole>.CreateAsync(TRole role, CancellationToken cancellationToken)
		{
			var found = await _collection.FirstOrDefaultAsync(x => x.NormalizedName == role.NormalizedName);
			if (found == null) await _collection.InsertOneAsync(role);
			return IdentityResult.Success;
		}

		async Task<IdentityResult> IRoleStore<TRole>.UpdateAsync(TRole role, CancellationToken cancellationToken)
		{
			await _collection.ReplaceOneAsync(x=>x.Id == role.Id, role, cancellationToken: cancellationToken);
			return IdentityResult.Success;
		}

		async Task<IdentityResult> IRoleStore<TRole>.DeleteAsync(TRole role, CancellationToken cancellationToken)
		{
            await _collection.DeleteOneAsync(x => x.Id == role.Id, cancellationToken);
            return IdentityResult.Success;
		}

		async Task<string> IRoleStore<TRole>.GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
		{
			return await Task.FromResult(role.Id);
		}
        
		async Task<string> IRoleStore<TRole>.GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
		{
			return (await _collection.FirstOrDefaultAsync(x => x.Id == role.Id))?.Name ?? role.Name;
		}

		async Task IRoleStore<TRole>.SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
		{
			role.Name = roleName;
			await _collection.UpdateOneAsync(x=>x.Id == role.Id, Builders<TRole>.Update.Set(x=>x.Name, roleName));
		}

		async Task<string> IRoleStore<TRole>.GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
		{
			return await Task.FromResult(role.NormalizedName);
		}

		async Task IRoleStore<TRole>.SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
		{
			role.NormalizedName = normalizedName;
            await _collection.UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.NormalizedName, normalizedName));
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