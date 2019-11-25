using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Mongo;
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

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
		{
			var found = await _collection.FirstOrDefaultAsync(x => x.NormalizedName == role.NormalizedName);
			if (found == null) await _collection.InsertOneAsync(role, new InsertOneOptions(), cancellationToken);
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
		{
			await _collection.ReplaceOneAsync(x=>x.Id == role.Id, role, cancellationToken: cancellationToken);
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
		{
            await _collection.DeleteOneAsync(x => x.Id == role.Id, cancellationToken);
            return IdentityResult.Success;
		}

		public async Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
		{
			return await Task.FromResult(role.Id);
		}
        
		public async Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
		{
			return (await _collection.FirstOrDefaultAsync(x => x.Id == role.Id))?.Name ?? role.Name;
		}

        public async Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
		{
			role.Name = roleName;
			await _collection.UpdateOneAsync(x=>x.Id == role.Id, Builders<TRole>.Update.Set(x=>x.Name, roleName), cancellationToken: cancellationToken);
		}

		public async Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
		{
			return await Task.FromResult(role.NormalizedName);
		}

        public async Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
		{
			role.NormalizedName = normalizedName;
            await _collection.UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.NormalizedName, normalizedName), cancellationToken: cancellationToken);
        }

        public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
		{
            return _collection.FirstOrDefaultAsync(x => x.Id == roleId);
		}

        public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
		{
			return _collection.FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName);
        }

		void IDisposable.Dispose()
		{
		}
	}
}