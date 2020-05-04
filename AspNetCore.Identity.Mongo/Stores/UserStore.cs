using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Mongo;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Identity.Mongo.Stores
{
    public class UserStore<TUserInfo, TUser, TRole> :
        IUserClaimStore<TUser>,
        IUserLoginStore<TUser>,
        IUserRoleStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IQueryableUserStore<TUser>,
        IUserTwoFactorStore<TUser>,
        IUserLockoutStore<TUser>,
        IUserAuthenticatorKeyStore<TUser>,
        IUserAuthenticationTokenStore<TUser>,
        IUserTwoFactorRecoveryCodeStore<TUser>,
        IProtectedUserStore<TUser>

        where TUserInfo : MongoUserInfo<TUser>, new()
        where TUser : MongoUser
        where TRole : MongoRole
    {
        private readonly IMongoCollection<TUserInfo> _users;
        private readonly IMongoCollection<TRole> _roles;

        private readonly ILookupNormalizer _normalizer;

        protected static readonly FilterDefinitionBuilder<TUserInfo> Filter = Builders<TUserInfo>.Filter;
        protected static readonly UpdateDefinitionBuilder<TUserInfo> Update = Builders<TUserInfo>.Update;

        public UserStore(IMongoCollection<TUserInfo> users, IMongoCollection<TRole> roles, ILookupNormalizer normalizer)
        {
            _users = users;
            _roles = roles;
            _normalizer = normalizer;
        }

        public IQueryable<TUser> Users => from i in _users.AsQueryable() select i.User;

        public async Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            token.ThrowIfCancellationRequested();

            using (var session = await _users.Database.Client.StartSessionAsync(cancellationToken: token).ConfigureAwait(false))
            {
                session.StartTransaction();

                try
                {
                    var tokenId = MongoUserToken.CreateId(loginProvider, name);

                    await _users.UpdateOneAsync(session, i => i.Id == user.Id, Update.PullFilter(i => i.Tokens, t => t.Id == tokenId), cancellationToken: token).ConfigureAwait(false);

                    await _users.UpdateOneAsync(session, i => i.Id == user.Id, Update.Push(i => i.Tokens, new MongoUserToken(tokenId, value)), cancellationToken: token).ConfigureAwait(false);

                    await session.CommitTransactionAsync(token).ConfigureAwait(false);
                }
                catch
                {
                    await session.AbortTransactionAsync(token).ConfigureAwait(false);

                    throw;
                }
            }
        }

        public async Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            var tokenId = MongoUserToken.CreateId(loginProvider, name);

            await _users.UpdateOneAsync(i => i.Id == user.Id, Update.PullFilter(i => i.Tokens, t => t.Id == tokenId), cancellationToken: token).ConfigureAwait(false);
        }

        public async Task<string> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            token.ThrowIfCancellationRequested();

            var tokenId = MongoUserToken.CreateId(loginProvider, name);

            return await _users.Find(Filter.Eq(i => i.Id, user.Id) & Filter.ElemMatch(i => i.Tokens, t => t.Id == tokenId)).Project(i => i.Tokens[-1].Value).SingleOrDefaultAsync(token).ConfigureAwait(false);
        }

        public Task<string> GetAuthenticatorKeyAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.AuthenticatorKey);
        }

        public Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            user.AuthenticatorKey = key;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            var id = ObjectId.GenerateNewId();

            user.Id = id;

            var newUserInfo = new TUserInfo { Id = id, User = user };

            try
            {
                await _users.InsertOneAsync(newUserInfo, cancellationToken: token).ConfigureAwait(false);
            }
            catch (MongoException e)
            {
                return IdentityResult.Failed(new IdentityError { Description = e.Message });
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            await _users.DeleteOneAsync(x => x.Id == user.Id, token).ConfigureAwait(false);
            return IdentityResult.Success;
        }

        public async Task<TUser> FindByIdAsync(string userId, CancellationToken token)
        {
            if (userId == null) throw new ArgumentNullException(nameof(userId));

            token.ThrowIfCancellationRequested();

            var id = ObjectId.Parse(userId);

            return await _users.Find(u => u.Id == id).Project(i => i.User).SingleOrDefaultAsync(token).ConfigureAwait(false);
        }

        public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return await _users.Find(u => u.User.NormalizedUserName == normalizedUserName).Project(i => i.User).SingleOrDefaultAsync(token).ConfigureAwait(false);
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            await _users.UpdateOneAsync(x => x.Id == user.Id, Update.Set(i => i.User, user), cancellationToken: token).ConfigureAwait(false);

            return IdentityResult.Success;
        }

        public async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (claims == null) throw new ArgumentNullException(nameof(claims));

            token.ThrowIfCancellationRequested();

            await _users.UpdateOneAsync(x => x.Id == user.Id, Update.AddToSetEach(i => i.Claims, claims.Select(c => new MongoClaim(c))), cancellationToken: token).ConfigureAwait(false);
        }

        public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (claim == null) throw new ArgumentNullException(nameof(claim));
            if (newClaim == null) throw new ArgumentNullException(nameof(newClaim));

            token.ThrowIfCancellationRequested();

            await _users.UpdateOneAsync(Filter.Eq(i => i.Id, user.Id) & Filter.ElemMatch(i => i.Claims, c => c.Type == claim.Type && c.Value == claim.Value), Update.Set(i => i.Claims[-1], new MongoClaim(newClaim)), cancellationToken: token).ConfigureAwait(false);
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (claims == null) throw new ArgumentNullException(nameof(claims));

            token.ThrowIfCancellationRequested();

            return _users.UpdateOneAsync(i => i.Id == user.Id, Update.PullAll(i => i.Claims, claims.Select(c => new MongoClaim(c))), cancellationToken: token);
        }

        public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken token)
        {
            if (claim == null) throw new ArgumentNullException(nameof(claim));

            token.ThrowIfCancellationRequested();

            return await _users.Find(Filter.ElemMatch(i => i.Claims, c => c.Type == claim.Type && c.Value == claim.Value)).Project(i => i.User).ToListAsync(token).ConfigureAwait(false);
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.NormalizedUserName ?? _normalizer.NormalizeName(user.UserName));
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.UserName);
        }

        public async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return (await _users.Find(i => i.Id == user.Id).Project(i => i.Claims).SingleOrDefaultAsync(token).ConfigureAwait(false))?.Select(c => new Claim(c.Type, c.Value)).ToList() ?? new List<Claim>();
        }

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var name = normalizedName ?? _normalizer.NormalizeName(user.UserName);

            user.NormalizedUserName = name;

            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException(nameof(userName));

            token.ThrowIfCancellationRequested();

            user.UserName = userName;

            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.EmailConfirmed);
        }

        public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return _users.Find(i => i.User.NormalizedEmail == normalizedEmail).Project(i => i.User).SingleOrDefaultAsync(token);
        }

        public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.EmailConfirmed = confirmed;

            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(normalizedEmail)) throw new ArgumentNullException(nameof(normalizedEmail));

            token.ThrowIfCancellationRequested();

            user.NormalizedEmail = normalizedEmail ?? _normalizer.NormalizeEmail(user.Email);

            return Task.CompletedTask;
        }

        public async Task SetEmailAsync(TUser user, string email, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));

            token.ThrowIfCancellationRequested();

            await SetNormalizedEmailAsync(user, _normalizer.NormalizeEmail(user.Email), token).ConfigureAwait(false);
            
            user.Email = email;
        }

        public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            ++user.AccessFailedCount;

            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            user.AccessFailedCount = 0;

            return Task.FromResult(0);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.LockoutEnd);
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            user.LockoutEnd = lockoutEnd;

            return Task.CompletedTask;
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            user.LockoutEnabled = enabled;

            return Task.CompletedTask;
        }

        public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (login == null) throw new ArgumentNullException(nameof(login));

            token.ThrowIfCancellationRequested();

            return _users.UpdateOneAsync(i => i.Id == user.Id, Update.AddToSet(i => i.Logins, login), cancellationToken: token);
        }

        public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(loginProvider)) throw new ArgumentNullException(nameof(loginProvider));
            if (string.IsNullOrEmpty(providerKey)) throw new ArgumentNullException(nameof(providerKey));

            token.ThrowIfCancellationRequested();

            return _users.UpdateOneAsync(i => i.Id == user.Id, Update.PullFilter(i => i.Logins, l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey), cancellationToken: token);
        }

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken token)
        {
            if (string.IsNullOrEmpty(loginProvider)) throw new ArgumentNullException(nameof(loginProvider));
            if (string.IsNullOrEmpty(providerKey)) throw new ArgumentNullException(nameof(providerKey));

            token.ThrowIfCancellationRequested();

            return await _users.Find(Filter.ElemMatch(i => i.Logins, l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey)).Project(i => i.User).FirstOrDefaultAsync(token).ConfigureAwait(false);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return (await _users.Find(i => i.Id == user.Id).Project(i => i.Logins).SingleOrDefaultAsync(token).ConfigureAwait(false))?.Select(c => new UserLoginInfo(c.LoginProvider, c.ProviderKey, string.Empty)).ToList() ?? new List<UserLoginInfo>();
        }

        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.PasswordHash != null);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(passwordHash)) throw new ArgumentNullException(nameof(passwordHash));

            token.ThrowIfCancellationRequested();

            user.PasswordHash = passwordHash;

            return Task.CompletedTask;
        }

        public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(phoneNumber)) throw new ArgumentNullException(nameof(phoneNumber));

            token.ThrowIfCancellationRequested();

            user.PhoneNumber = phoneNumber;

            return Task.CompletedTask;
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            user.PhoneNumberConfirmed = confirmed;

            return Task.CompletedTask;
        }

        private Task<ObjectId> GetRoleIdByName(string roleName, CancellationToken token) => _roles.Find(r => r.NormalizedName == roleName).Project(r => r.Id).SingleOrDefaultAsync(token);

        public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

            token.ThrowIfCancellationRequested();

            var roleId = await GetRoleIdByName(roleName, token).ConfigureAwait(false);

            if (roleId == null) return;

            await _users.UpdateOneAsync(i => i.Id == user.Id, Update.AddToSet(i => i.Roles, roleId), cancellationToken: token).ConfigureAwait(false);
        }

        public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

            token.ThrowIfCancellationRequested();

            var roleId = await GetRoleIdByName(roleName, token).ConfigureAwait(false);

            if (roleId == null) return;

            await _users.UpdateOneAsync(i => i.Id == user.Id, Update.Pull(i => i.Roles, roleId), cancellationToken: token).ConfigureAwait(false);
        }


        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken token)
        {
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

            token.ThrowIfCancellationRequested();

            var roleId = await GetRoleIdByName(roleName, token).ConfigureAwait(false);

            if (roleId == null) return new List<TUser>();

            return await _users.Find(Filter.AnyEq(x => x.Roles, roleId)).Project(i => i.User).ToListAsync(token).ConfigureAwait(false);
        }

        public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            var roleIds = await _users.Find(i => i.Id == user.Id).Project(i => i.Roles).SingleOrDefaultAsync(token).ConfigureAwait(false);

            if (roleIds == null) return new List<string>();

            return await _roles.Find(Builders<TRole>.Filter.In(r => r.Id, roleIds)).Project(r => r.NormalizedName).ToListAsync(token).ConfigureAwait(false);
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

            token.ThrowIfCancellationRequested();

            var roleId = await GetRoleIdByName(roleName, token).ConfigureAwait(false);

            if (roleId == null) return false;

            return await _users.Find(Filter.Eq(u => u.Id, user.Id) & Filter.AnyEq(u => u.Roles, roleId)).AnyAsync(token).ConfigureAwait(false);
        }

        public Task<string> GetSecurityStampAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (stamp == null) throw new ArgumentNullException(nameof(stamp));

            token.ThrowIfCancellationRequested();

            user.SecurityStamp = stamp;

            return Task.CompletedTask;
        }

        public Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (recoveryCodes == null) throw new ArgumentNullException(nameof(recoveryCodes));

            token.ThrowIfCancellationRequested();

            return _users.UpdateOneAsync(i => i.Id == user.Id, Update.Set(i => i.RecoveryCodes, recoveryCodes), cancellationToken: token);
        }

        public async Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (code == null) throw new ArgumentNullException(nameof(code));

            token.ThrowIfCancellationRequested();

            var pullUpdateResult = await _users.UpdateOneAsync(i => i.Id == user.Id, Update.Pull(i => i.RecoveryCodes, code), cancellationToken: token).ConfigureAwait(false);

            return pullUpdateResult.IsModifiedCountAvailable && pullUpdateResult.ModifiedCount > 0;
        }

        private static readonly BsonDocument _recoveryCodesSizeQuery = BsonDocument.Parse("{ count: { $cond: { if: { $isArray: '$RecoveryCodes' }, then: { $size: '$RecoveryCodes' }, else: 0 }}}");

        public async Task<int> CountCodesAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return (await _users.Aggregate().Match(i => i.Id == user.Id).Project(_recoveryCodesSizeQuery).SingleOrDefaultAsync(token).ConfigureAwait(false))?.GetValue(1).ToInt32() ?? 0;
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken token)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            token.ThrowIfCancellationRequested();

            user.TwoFactorEnabled = enabled;

            return Task.CompletedTask;
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
