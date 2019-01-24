using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo.Collections
{
    public class IdentityUserCollection<TUser> : IIdentityUserCollection<TUser> where TUser : MongoUser
	{
	    private readonly IMongoCollection<TUser> _users;

        public IdentityUserCollection(string connectionString, string collectionName)
        {
            _users = MongoUtil.FromConnectionString<TUser>(connectionString, collectionName);
        }

		public async Task<TUser> FindByEmailAsync(string normalizedEmail)
		{
			return await _users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
		}

		public async Task<TUser> FindByUserNameAsync(string username)
		{
			return await _users.FirstOrDefaultAsync(u => u.UserName == username);
		}

		public async Task<TUser> FindByNormalizedUserNameAsync(string normalizedUserName)
		{
			return await _users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName);
		}

		public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey)
		{
			return await _users.FirstOrDefaultAsync(u =>
				u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey));
		}

		public async Task<IEnumerable<TUser>> FindUsersByClaimAsync(string claimType, string claimValue)
		{
			return await _users.WhereAsync(u => u.Claims.Any(c => c.ClaimType == claimType && c.ClaimType == claimValue));
		}

		public async Task<IEnumerable<TUser>> FindUsersInRoleAsync(string roleName)
		{
		    var filter = Builders<TUser>.Filter.AnyEq(x => x.Roles, roleName);
            var res = await _users.FindAsync(filter);
		    return res.ToEnumerable();
        }

        public async Task<IEnumerable<TUser>> GetAllAsync()
        {
            var res = await _users.FindAsync(x=>true);
            return res.ToEnumerable();
        }

        public async Task<TUser> CreateAsync(TUser obj)
        {
            await _users.InsertOneAsync(obj);
            return obj;
        }

	    public Task UpdateAsync(TUser obj) => _users.ReplaceOneAsync(x => x.Id == obj.Id, obj);

        public Task DeleteAsync(TUser obj) => _users.DeleteOneAsync(x => x.Id == obj.Id);

	    public Task<TUser> FindByIdAsync(string itemId) => _users.FirstOrDefaultAsync(x => x.Id == itemId);
	}
}