using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;

namespace AspNetCore.Identity.Mongo.Collections
{
	public interface IIdentityRoleCollection<TRole> where TRole : MongoRole
	{
		Task<TRole> FindByNameAsync(string normalizedName);
	    Task<TRole> FindByIdAsync(string roleId);
	    Task<IEnumerable<TRole>> GetAllAsync();
	    Task<TRole> CreateAsync(TRole obj);
	    Task UpdateAsync(TRole obj);
	    Task DeleteAsync(TRole obj);
    }
}