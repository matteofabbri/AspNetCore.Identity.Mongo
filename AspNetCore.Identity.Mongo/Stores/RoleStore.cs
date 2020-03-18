using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Mongo;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo.Stores
{
    public class RoleStore<TRole> :
       IRoleClaimStore<TRole>,
       IQueryableRoleStore<TRole> where TRole : MongoRole
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
            await _collection.ReplaceOneAsync(x => x.Id == role.Id, role, cancellationToken: cancellationToken);
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
            await _collection.UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.Name, roleName), cancellationToken: cancellationToken);
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

        public async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dbRole = await _collection.FirstOrDefaultAsync(x => x.Id == role.Id);
            return dbRole.Claims.Select(e => new Claim(e.ClaimType, e.ClaimValue)).ToList();
        }

        public async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentClaim = role.Claims
                                   .FirstOrDefault(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);

            if (currentClaim == null)
            {
                var identityRoleClaim = new IdentityRoleClaim<string>()
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                };

                role.Claims.Add(identityRoleClaim);
                await _collection.UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.Claims, role.Claims), cancellationToken: cancellationToken);
            }
        }

        public async Task AddClaimsAsync(TRole role, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var claim in claims)
            {
                var currentClaim = role.Claims
                                  .FirstOrDefault(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);

                if (currentClaim == null)
                {
                    var identityRoleClaim = new IdentityRoleClaim<string>()
                    {
                        ClaimType = claim.Type,
                        ClaimValue = claim.Value
                    };
                    role.Claims.Add(identityRoleClaim);
                }
            }
            await _collection.UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.Claims, role.Claims), cancellationToken: cancellationToken);

            //await Add(user, x => x.Claims, identityClaim);

        }

        public Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            role.Claims.RemoveAll(x => x.ClaimType == claim.Type);
            return _collection.UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.Claims, role.Claims), cancellationToken: cancellationToken);
        }

        public Task RemoveClaimsAsync(TRole role, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var claim in claims)
            {
                role.Claims.RemoveAll(x => x.ClaimType == claim.Type);
            }
            return _collection.UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.Claims, role.Claims), cancellationToken: cancellationToken);
        }

        void IDisposable.Dispose()
        {
        }
    }
}