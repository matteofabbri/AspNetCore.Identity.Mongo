using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo.Collections
{

    public class IdentityRoleCollection<TRole> : IIdentityRoleCollection<TRole> where TRole : MongoRole
	{
	    private readonly IMongoCollection<TRole> _roles;

		public IdentityRoleCollection(string connectionString, string collectionName)
		{
		    _roles = MongoUtil.FromConnectionString<TRole>(connectionString, collectionName);
		}

		public async Task<TRole> FindByNameAsync(string normalizedName)
		{
			return await _roles.FirstOrDefaultAsync(x => x.NormalizedName == normalizedName);
		}

		public async Task<TRole> FindByIdAsync(string roleId)
		{
			return await _roles.FirstOrDefaultAsync(x => x.Id == roleId);
		}

        public async Task<IEnumerable<TRole>> GetAllAsync()
        {
            return (await _roles.FindAsync(x => true)).ToEnumerable();
        }

        public async Task<TRole> CreateAsync(TRole obj)
        {
            await _roles.InsertOneAsync(obj);
            return obj;
        }

        public Task UpdateAsync(TRole obj) => _roles.ReplaceOneAsync(x=>x.Id == obj.Id, obj);

        public Task DeleteAsync(TRole obj) => _roles.DeleteOneAsync(x => x.Id == obj.Id);
    }
}