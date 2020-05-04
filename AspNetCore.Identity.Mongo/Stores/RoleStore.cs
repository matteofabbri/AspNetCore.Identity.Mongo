using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AspNetCore.Identity.Mongo.Stores
{
    public class RoleStore<TRole> :
        IRoleClaimStore<TRole>,
        IQueryableRoleStore<TRole>
        
        where TRole : MongoRole
    {
        private readonly IMongoCollection<TRole> _collection;

        public RoleStore(IMongoCollection<TRole> collection)
        {
            _collection = collection;
        }

        public IQueryable<TRole> Roles => _collection.AsQueryable();

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            var found = await _collection.FirstOrDefaultAsync(x => x.NormalizedName == role.NormalizedName, cancellationToken).ConfigureAwait(false);

            if (found == null) await _collection.InsertOneAsync(role, cancellationToken: cancellationToken).ConfigureAwait(false);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            await _collection.ReplaceOneAsync(x => x.Id == role.Id, role, cancellationToken: cancellationToken).ConfigureAwait(false);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            await _collection.DeleteOneAsync(x => x.Id == role.Id, cancellationToken).ConfigureAwait(false);

            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Id.ToString());
        }

        public async Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            return (await _collection.FirstOrDefaultAsync(x => x.Id == role.Id, cancellationToken: cancellationToken).ConfigureAwait(false))?.Name ?? role.Name;
        }

        public async Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

            role.Name = roleName;

            await _collection.UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.Name, roleName), cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.NormalizedName);
        }

        public async Task SetNormalizedRoleNameAsync(TRole role, string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (string.IsNullOrEmpty(normalizedRoleName)) throw new ArgumentNullException(nameof(normalizedRoleName));

            role.NormalizedName = normalizedRoleName;

            await _collection.UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.NormalizedName, normalizedRoleName), cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(roleId)) throw new ArgumentNullException(nameof(roleId));

            return _collection.FirstOrDefaultAsync(x => x.Id == ObjectId.Parse(roleId), cancellationToken: cancellationToken);
        }

        public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(normalizedRoleName)) throw new ArgumentNullException(nameof(normalizedRoleName));

            return _collection.FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName, cancellationToken: cancellationToken);
        }

        public async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            cancellationToken.ThrowIfCancellationRequested();

            var dbRole = await _collection.FirstOrDefaultAsync(x => x.Id == role.Id, cancellationToken: cancellationToken).ConfigureAwait(false);

            return dbRole.Claims.Select(e => new Claim(e.ClaimType, e.ClaimValue)).ToList();
        }

        public async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (claim == null) throw new ArgumentNullException(nameof(claim));

            cancellationToken.ThrowIfCancellationRequested();

            var currentClaim = role.Claims.FirstOrDefault(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);

            if (currentClaim == null)
            {
                var identityRoleClaim = new IdentityRoleClaim<string>()
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                };

                role.Claims.Add(identityRoleClaim);

                await _collection.UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.Claims, role.Claims), cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        public Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (claim == null) throw new ArgumentNullException(nameof(claim));

            cancellationToken.ThrowIfCancellationRequested();

            role.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);

            return _collection.UpdateOneAsync(x => x.Id == role.Id, Builders<TRole>.Update.Set(x => x.Claims, role.Claims), cancellationToken: cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}