using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    public class UserStore<TUser, TRole> :
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

        where TUser : MongoUser
        where TRole : MongoRole
    {
        private readonly IRoleStore<TRole> _roleStore;

        private readonly IMongoCollection<TUser> _userCollection;

        private readonly ILookupNormalizer _normalizer;

        public UserStore(IMongoCollection<TUser> userCollection, IRoleStore<TRole> roleStore, ILookupNormalizer normalizer)
        {
            _userCollection = userCollection;
            _roleStore = roleStore;
            _normalizer = normalizer;
        }

        public IQueryable<TUser> Users => _userCollection.AsQueryable();

        private async Task UpdateAsync<TFieldValue>(TUser user, Expression<Func<TUser, TFieldValue>> expression, TFieldValue value, CancellationToken cancellationToken)
        {
            var updateDefinition = Builders<TUser>.Update.Set(expression, value);

            await _userCollection.UpdateOneAsync(x => x.Id == user.Id, updateDefinition, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private async Task AddToSetAsync<TFieldValue>(TUser user, Expression<Func<TUser, IEnumerable<TFieldValue>>> expression, TFieldValue value, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            await _userCollection.UpdateOneAsync(x => x.Id == user.Id, Builders<TUser>.Update.AddToSet(expression, value), cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private Task<TProjection> ByIdAsync<TProjection>(ObjectId id, Expression<Func<TUser, TProjection>> projection, CancellationToken cancellationToken)
        {
            return _userCollection.FirstOrDefaultAsync(x => x.Id == id, projection, cancellationToken);
        }

        public async Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            cancellationToken.ThrowIfCancellationRequested();

            if (user.Tokens == null) user.Tokens = new List<IdentityUserToken<string>>();

            var token = user.Tokens.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name);

            if (token == null)
            {
                token = new IdentityUserToken<string>
                {
                    LoginProvider = loginProvider,
                    Name = name,
                    Value = value
                };

                await AddToSetAsync(user, x => x.Tokens, token, cancellationToken).ConfigureAwait(false);
                user.Tokens.Add(token);
            }
            else
            {
                token.Value = value;
                await UpdateAsync(user, x => x.Tokens, user.Tokens, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var userTokens = user.Tokens ?? new List<IdentityUserToken<string>>();
            userTokens.RemoveAll(x => x.LoginProvider == loginProvider && x.Name == name);
            await UpdateAsync(user, x => x.Tokens, userTokens, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            cancellationToken.ThrowIfCancellationRequested();

            var tokens = await ByIdAsync(user.Id, user => user.Tokens, cancellationToken).ConfigureAwait(false) ?? user.Tokens;

            return tokens?.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name)?.Value;
        }

        public async Task<string> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, user => user.AuthenticatorKey, cancellationToken).ConfigureAwait(false)) ?? user.AuthenticatorKey;
        }

        public async Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            user.AuthenticatorKey = key;

            await UpdateAsync(user, x => x.AuthenticatorKey, key, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var u = await _userCollection.FirstOrDefaultAsync(x => x.UserName == user.UserName).ConfigureAwait(false);

            if (u != null) return IdentityResult.Failed(new IdentityError { Code = "Username already in use" });

            if (user.Email != null) user.NormalizedEmail = _normalizer.NormalizeEmail(user.Email);

            await _userCollection.InsertOneAsync(user, cancellationToken: cancellationToken).ConfigureAwait(false);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            await _userCollection.DeleteOneAsync(x => x.Id == user.Id, cancellationToken).ConfigureAwait(false);
            return IdentityResult.Success;
        }

        public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (userId == null) throw new ArgumentNullException(nameof(userId));

            cancellationToken.ThrowIfCancellationRequested();

            var id = ObjectId.Parse(userId);

            return _userCollection.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return _userCollection.FirstOrDefaultAsync(x => x.NormalizedUserName == normalizedUserName, cancellationToken: cancellationToken);
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            await SetEmailAsync(user, user.Email, cancellationToken).ConfigureAwait(false);
            await _userCollection.ReplaceOneAsync(x => x.Id == user.Id, user, cancellationToken: cancellationToken).ConfigureAwait(false);

            return IdentityResult.Success;
        }

        public async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (claims == null) throw new ArgumentNullException(nameof(claims));

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var claim in claims)
            {
                var identityClaim = new IdentityUserClaim<string>()
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                };

                user.Claims.Add(identityClaim);

                await AddToSetAsync(user, x => x.Claims, identityClaim, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (claim == null) throw new ArgumentNullException(nameof(claim));
            if (newClaim == null) throw new ArgumentNullException(nameof(newClaim));

            cancellationToken.ThrowIfCancellationRequested();

            var claims = user.Claims;

            claims.RemoveAll(x => x.ClaimType == claim.Type);
            claims.Add(new IdentityUserClaim<string>()
            {
                ClaimType = newClaim.Type,
                ClaimValue = newClaim.Value
            });
            user.Claims = claims;


            await UpdateAsync(user, x => x.Claims, claims, cancellationToken).ConfigureAwait(false);
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (claims == null) throw new ArgumentNullException(nameof(claims));

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var claim in claims)
            {
                user.Claims.RemoveAll(x => x.ClaimType == claim.Type);
            }

            return UpdateAsync(user, x => x.Claims, user.Claims, cancellationToken);
        }

        public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            if (claim == null) throw new ArgumentNullException(nameof(claim));

            cancellationToken.ThrowIfCancellationRequested();

            return (await _userCollection.WhereAsync(u => u.Claims.Any(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value), cancellationToken).ConfigureAwait(false)).ToList();

        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(user.NormalizedUserName ?? _normalizer.NormalizeName(user.UserName));
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(user.UserName);
        }

        public async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var claims = await ByIdAsync(user.Id, user => user.Claims, cancellationToken).ConfigureAwait(false);

            return claims?.Select(x => new Claim(x.ClaimType, x.ClaimValue))?.ToList() ?? new List<Claim>();
        }

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var name = normalizedName ?? _normalizer.NormalizeName(user.UserName);

            user.NormalizedUserName = name;
            return UpdateAsync(user, x => x.NormalizedUserName, name, cancellationToken);
        }

        public async Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException(nameof(userName));

            cancellationToken.ThrowIfCancellationRequested();

            await SetNormalizedUserNameAsync(user, _normalizer.NormalizeName(userName), cancellationToken)
                .ConfigureAwait(false);

            user.UserName = userName;

            await UpdateAsync(user, x => x.UserName, userName, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, user => user.Email, cancellationToken).ConfigureAwait(false)) ?? user.Email;
        }

        public async Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return await ByIdAsync(user.Id, user => user.EmailConfirmed, cancellationToken).ConfigureAwait(false);
        }

        public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return _userCollection.FirstOrDefaultAsync(a => a.NormalizedEmail == normalizedEmail);
        }

        public async Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, u => u.NormalizedEmail, cancellationToken).ConfigureAwait(false));
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.EmailConfirmed = confirmed;

            return UpdateAsync(user, x => x.EmailConfirmed, confirmed, cancellationToken);
        }

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(normalizedEmail)) throw new ArgumentNullException(nameof(normalizedEmail));

            cancellationToken.ThrowIfCancellationRequested();

            user.NormalizedEmail = normalizedEmail ?? _normalizer.NormalizeEmail(user.Email);

            return UpdateAsync(user, x => x.NormalizedEmail, user.NormalizedEmail, cancellationToken);
        }

        public async Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));

            cancellationToken.ThrowIfCancellationRequested();

            await SetNormalizedEmailAsync(user, _normalizer.NormalizeEmail(user.Email), cancellationToken).ConfigureAwait(false);
            
            user.Email = email;

            await UpdateAsync(user, x => x.Email, user.Email, cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return Math.Max(await ByIdAsync(user.Id, u => u.AccessFailedCount, cancellationToken).ConfigureAwait(false), user.AccessFailedCount);
        }

        public async Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, u => u.LockoutEnabled,cancellationToken).ConfigureAwait(false)) || user.LockoutEnabled;
        }

        public async Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            ++user.AccessFailedCount;

            await UpdateAsync(user, x => x.AccessFailedCount, user.AccessFailedCount, cancellationToken).ConfigureAwait(false);

            return user.AccessFailedCount;
        }

        public async Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            user.AccessFailedCount = 0;

            await UpdateAsync(user, x => x.AccessFailedCount, 0, cancellationToken).ConfigureAwait(false);
        }

        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return await ByIdAsync(user.Id, u => u.LockoutEnd, cancellationToken).ConfigureAwait(false) ?? user.LockoutEnd;
        }

        public async Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            user.LockoutEnd = lockoutEnd;

            await UpdateAsync(user, x => x.LockoutEnd, user.LockoutEnd, cancellationToken).ConfigureAwait(false);
        }

        public async Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            user.LockoutEnabled = enabled;
            await UpdateAsync(user, x => x.LockoutEnabled, user.LockoutEnabled, cancellationToken).ConfigureAwait(false);
        }

        public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (login == null) throw new ArgumentNullException(nameof(login));

            cancellationToken.ThrowIfCancellationRequested();

            var iul = new IdentityUserLogin<string>
            {
                UserId = user.Id.ToString(),
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName,
                ProviderKey = login.ProviderKey
            };

            user.Logins.Add(iul);

            return AddToSetAsync(user, x => x.Logins, iul, cancellationToken);
        }

        public async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(loginProvider)) throw new ArgumentNullException(nameof(loginProvider));
            if (string.IsNullOrEmpty(providerKey)) throw new ArgumentNullException(nameof(providerKey));

            cancellationToken.ThrowIfCancellationRequested();

            user.Logins.RemoveAll(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);

            await UpdateAsync(user, x => x.Logins, user.Logins, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(loginProvider)) throw new ArgumentNullException(nameof(loginProvider));
            if (string.IsNullOrEmpty(providerKey)) throw new ArgumentNullException(nameof(providerKey));

            cancellationToken.ThrowIfCancellationRequested();

            return await _userCollection.FirstOrDefaultAsync(u => u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey), cancellationToken).ConfigureAwait(false);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var logins = await ByIdAsync(user.Id, u => u.Logins, cancellationToken).ConfigureAwait(false);

            return logins?.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName))?.ToList() ?? new List<UserLoginInfo>();
        }

        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(user.PasswordHash);
        }

        public async Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return ((await ByIdAsync(user.Id, u => u.PasswordHash, cancellationToken).ConfigureAwait(false)) ?? user.PasswordHash) != null;
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(passwordHash)) throw new ArgumentNullException(nameof(passwordHash));

            cancellationToken.ThrowIfCancellationRequested();

            user.PasswordHash = passwordHash;

            return UpdateAsync(user, x => x.PasswordHash, passwordHash, cancellationToken);
        }

        public async Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, u => u.PhoneNumber, cancellationToken).ConfigureAwait(false)) ?? user.PhoneNumber;
        }

        public async Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, u => u.PhoneNumberConfirmed, cancellationToken).ConfigureAwait(false)) || user.PhoneNumberConfirmed;
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(phoneNumber)) throw new ArgumentNullException(nameof(phoneNumber));

            cancellationToken.ThrowIfCancellationRequested();

            user.PhoneNumber = phoneNumber;
            return UpdateAsync(user, x => x.PhoneNumber, phoneNumber, cancellationToken);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            user.PhoneNumberConfirmed = confirmed;
            return UpdateAsync(user, x => x.PhoneNumberConfirmed, confirmed, cancellationToken);
        }

        public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

            cancellationToken.ThrowIfCancellationRequested();

            var role = await _roleStore.FindByNameAsync(roleName, cancellationToken).ConfigureAwait(false);

            if (role == null) return;

            user.Roles.Add(role.Id);

            await AddToSetAsync(user, u => u.Roles, role.Id, cancellationToken).ConfigureAwait(false);

        }

        public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

            cancellationToken.ThrowIfCancellationRequested();

            var role = await _roleStore.FindByNameAsync(roleName, cancellationToken).ConfigureAwait(false);

            if (role == null) return;

            user.Roles.Remove(role.Id);

            await _userCollection.UpdateOneAsync(Builders<TUser>.Filter.Eq(u => u.Id, user.Id), Builders<TUser>.Update.Pull(u => u.Roles, role.Id), cancellationToken: cancellationToken).ConfigureAwait(false);
        }


        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

            cancellationToken.ThrowIfCancellationRequested();

            var role = await _roleStore.FindByNameAsync(roleName, cancellationToken).ConfigureAwait(false);
            
            if (role == null) return new List<TUser>();

            var users = await _userCollection.FindAsync(Builders<TUser>.Filter.AnyEq(x => x.Roles, role.Id), cancellationToken: cancellationToken).ConfigureAwait(false);

            return await users.ToListAsync().ConfigureAwait(false);
        }

        public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var roleIds = await ByIdAsync(user.Id, u => u.Roles, cancellationToken).ConfigureAwait(false);

            if (roleIds == null) return new List<string>();

            var roles = new List<string>();

            foreach (var roleId in roleIds)
            {
                var dbRole = await _roleStore.FindByIdAsync(roleId.ToString(), cancellationToken).ConfigureAwait(false);

                if (dbRole != null) roles.Add(dbRole.Name);
            }

            return roles;
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

            cancellationToken.ThrowIfCancellationRequested();

            var role = await _roleStore.FindByNameAsync(roleName, cancellationToken).ConfigureAwait(false);

            return await _userCollection.Find(Builders<TUser>.Filter.Eq(u => u.Id, user.Id) & Builders<TUser>.Filter.AnyEq(u => u.Roles, role.Id)).AnyAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, u => u.SecurityStamp, cancellationToken).ConfigureAwait(false)) ?? user.SecurityStamp;
        }

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.SecurityStamp = stamp;

            return UpdateAsync(user, x => x.SecurityStamp, user.SecurityStamp, cancellationToken);
        }

        public Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            user.RecoveryCodes = recoveryCodes.Select(x => new TwoFactorRecoveryCode { Code = x, Redeemed = false }).ToList();

            return UpdateAsync(user, x => x.RecoveryCodes, user.RecoveryCodes, cancellationToken);
        }

        public async Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var recoveryCodes = await ByIdAsync(user.Id, u => u.RecoveryCodes, cancellationToken).ConfigureAwait(false);

            if (recoveryCodes == null) return false;

            var c = recoveryCodes.FirstOrDefault(x => x.Code == code);

            if (c == null || c.Redeemed) return false;

            c.Redeemed = true;

            await UpdateAsync(user, x => x.RecoveryCodes, recoveryCodes, cancellationToken).ConfigureAwait(false);

            return true;
        }

        public async Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var recoveryCodes = await ByIdAsync(user.Id, u => u.RecoveryCodes, cancellationToken).ConfigureAwait(false);

            return recoveryCodes?.Count ?? user.RecoveryCodes?.Count ?? 0;
        }

        public async Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return await ByIdAsync(user.Id, u => u.TwoFactorEnabled, cancellationToken).ConfigureAwait(false);
        }

        public async Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            user.TwoFactorEnabled = enabled;

            await UpdateAsync(user, x => x.TwoFactorEnabled, enabled, cancellationToken).ConfigureAwait(false);
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
