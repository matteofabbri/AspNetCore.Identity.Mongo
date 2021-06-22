using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Mongo;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AspNetCore.Identity.Mongo.Stores
{
    /// <summary>
    /// Creates a new instance of a persistence store for roles.
    /// </summary>
    /// <typeparam name="TRole">The type of the class representing a role</typeparam>
    public class RoleStore<TRole, TKey> :
        IRoleClaimStore<TRole>,
        IQueryableRoleStore<TRole>
        where TKey : IEquatable<TKey>
        where TRole : MongoRole<TKey>
    {
        /// <summary>
        /// Gets or sets the <see cref="IdentityErrorDescriber"/> for any error that occurred with the current operation.
        /// </summary>
        public IdentityErrorDescriber ErrorDescriber { get; set; }

        private readonly IMongoCollection<TRole> _collection;

        private bool _disposed;

        public RoleStore(IMongoCollection<TRole> collection, IdentityErrorDescriber describer)
        {
            _collection = collection;
            ErrorDescriber = describer ?? new IdentityErrorDescriber();
        }

        /// <summary>
        /// A navigation property for the roles the store contains.
        /// </summary>
        public IQueryable<TRole> Roles => _collection.AsQueryable();

        /// <summary>
        /// Creates a new role in a store as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role to create in the store.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null) throw new ArgumentNullException(nameof(role));

            await _collection.InsertOneAsync(role, cancellationToken: cancellationToken).ConfigureAwait(false);

            return IdentityResult.Success;
        }

        /// <summary>
        /// Updates a role in a store as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role to update in the store.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
        public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null) throw new ArgumentNullException(nameof(role));

            var currentConcurrencyStamp = role.ConcurrencyStamp;
            role.ConcurrencyStamp = Guid.NewGuid().ToString();

            var result = await _collection.ReplaceOneAsync(x => x.Id.Equals(role.Id) && x.ConcurrencyStamp.Equals(currentConcurrencyStamp), role, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result.IsAcknowledged && result.ModifiedCount == 0)
            {
                return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }

            return IdentityResult.Success;
        }

        /// <summary>
        /// Deletes a role from the store as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role to delete from the store.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null) throw new ArgumentNullException(nameof(role));

            var result = await _collection.DeleteOneAsync(x => x.Id.Equals(role.Id) && x.ConcurrencyStamp.Equals(role.ConcurrencyStamp), cancellationToken).ConfigureAwait(false);
            if (!result.IsAcknowledged && result.DeletedCount == 0)
            {
                return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }

            return IdentityResult.Success;
        }

        /// <summary>
        /// Gets the ID for a role from the store as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role whose ID should be returned.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that contains the ID of the role.</returns>
        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null) throw new ArgumentNullException(nameof(role));

            return Task.FromResult(ConvertIdToString(role.Id));
        }

        /// <summary>
        /// Gets the name of a role from the store as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role whose name should be returned.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that contains the name of the role.</returns>
        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null) throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.Name);
        }

        /// <summary>
        /// Sets the name of a role in the store as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role whose name should be set.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null) throw new ArgumentNullException(nameof(role));

            role.Name = roleName;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Get a role's normalized name as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role whose normalized name should be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that contains the name of the role.</returns>
        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null) throw new ArgumentNullException(nameof(role));

            return Task.FromResult(role.NormalizedName);
        }

        /// <summary>
        /// Set a role's normalized name as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role whose normalized name should be set.</param>
        /// <param name="normalizedName">The normalized name to set</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null) throw new ArgumentNullException(nameof(role));

            role.NormalizedName = normalizedRoleName;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Finds the role who has the specified ID as an asynchronous operation.
        /// </summary>
        /// <param name="id">The role ID to look for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that result of the look up.</returns>
        public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return _collection.FirstOrDefaultAsync(x => x.Id.Equals(ConvertIdFromString(roleId)), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Finds the role who has the specified normalized name as an asynchronous operation.
        /// </summary>
        /// <param name="normalizedName">The normalized role name to look for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that result of the look up.</returns>
        public Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return _collection.FirstOrDefaultAsync(x => x.NormalizedName == normalizedName, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get the claims associated with the specified <paramref name="role"/> as an asynchronous operation.
        /// </summary>
        /// <param name="role">The role whose claims should be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that contains the claims granted to a role.</returns>
        public async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (role == null) throw new ArgumentNullException(nameof(role));

            var dbRole = await _collection.FirstOrDefaultAsync(x => x.Id.Equals(role.Id), cancellationToken: cancellationToken).ConfigureAwait(false);

            return dbRole?.Claims.Select(e => new Claim(e.ClaimType, e.ClaimValue)).ToList() ?? new List<Claim>();
        }

        /// <summary>
        /// Adds the <paramref name="claim"/> given to the specified <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The role to add the claim to.</param>
        /// <param name="claim">The claim to add to the role.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (role == null) throw new ArgumentNullException(nameof(role));
            if (claim == null) throw new ArgumentNullException(nameof(claim));

            var identityRoleClaim = new IdentityRoleClaim<string>()
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            };

            role.Claims.Add(identityRoleClaim);

            await _collection.UpdateOneAsync(x => x.Id.Equals(role.Id), Builders<TRole>.Update.Set(x => x.Claims, role.Claims), cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes the <paramref name="claim"/> given from the specified <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The role to remove the claim from.</param>
        /// <param name="claim">The claim to remove from the role.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (role == null) throw new ArgumentNullException(nameof(role));
            if (claim == null) throw new ArgumentNullException(nameof(claim));

            role.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);

            return _collection.UpdateOneAsync(x => x.Id.Equals(role.Id), Builders<TRole>.Update.Set(x => x.Claims, role.Claims), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Throws if this class has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <summary>
        /// Dispose the stores
        /// </summary>
        public void Dispose() => _disposed = true;

        /// <summary>
        /// Converts the provided <paramref name="id"/> to a strongly typed key object.
        /// </summary>
        /// <param name="id">The id to convert.</param>
        /// <returns>An instance of <typeparamref name="TKey"/> representing the provided <paramref name="id"/>.</returns>
        public virtual TKey ConvertIdFromString(string id)
        {
            if (id == null)
            {
                return default(TKey);
            }
            return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id);
        }

        /// <summary>
        /// Converts the provided <paramref name="id"/> to its string representation.
        /// </summary>
        /// <param name="id">The id to convert.</param>
        /// <returns>An <see cref="string"/> representation of the provided <paramref name="id"/>.</returns>
        public virtual string ConvertIdToString(TKey id)
        {
            if (object.Equals(id, default(TKey)))
            {
                return null;
            }
            return id.ToString();
        }
    }
}