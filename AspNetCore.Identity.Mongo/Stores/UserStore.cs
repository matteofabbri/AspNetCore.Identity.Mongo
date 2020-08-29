using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class UserStore<TUser, TRole, TKey> :
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
        where TKey : IEquatable<TKey>
        where TUser : MongoUser<TKey>
        where TRole : MongoRole<TKey>
    {
        private readonly IRoleStore<TRole> _roleStore;

        private readonly IMongoCollection<TUser> _userCollection;

        private readonly ILookupNormalizer _normalizer;

        private static readonly InsertOneOptions InsertOneOptions = new InsertOneOptions();

        private static readonly FindOptions<TUser> FindOptions = new FindOptions<TUser>();

        private static readonly ReplaceOptions ReplaceOptions = new ReplaceOptions();

        public UserStore(IMongoCollection<TUser> userCollection, IRoleStore<TRole> roleStore, ILookupNormalizer normalizer)
        {
            _userCollection = userCollection;
            _roleStore = roleStore;
            _normalizer = normalizer;

            EnsureIndex(x => x.NormalizedEmail);
            EnsureIndex(x => x.NormalizedUserName);
        }

        private void EnsureIndex(Expression<Func<TUser, object>> field)
        {
            var model = new CreateIndexModel<TUser>(Builders<TUser>.IndexKeys.Ascending(field));
            _userCollection.Indexes.CreateOne(model);
        }

        public IQueryable<TUser> Users => _userCollection.AsQueryable();

        private async Task UpdateAsync<TFieldValue>(TUser user, Expression<Func<TUser, TFieldValue>> expression, TFieldValue value, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var updateDefinition = Builders<TUser>.Update.Set(expression, value);

            await _userCollection.UpdateOneAsync(x => x.Id.Equals(user.Id), updateDefinition, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private async Task AddAsync<TFieldValue>(TUser user, Expression<Func<TUser, IEnumerable<TFieldValue>>> expression, TFieldValue value, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var addDefinition = Builders<TUser>.Update.AddToSet(expression, value);

            await _userCollection.UpdateOneAsync(x => x.Id.Equals(user.Id), addDefinition, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private Task<TUser> ByIdAsync(TKey id, CancellationToken cancellationToken)
        {
            return _userCollection.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
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

                await AddAsync(user, x => x.Tokens, token, cancellationToken).ConfigureAwait(false);
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

            var token = user?.Tokens?.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name);

            if (token == null)
            {
                user = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);
                return user?.Tokens?.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name)?.Value;
            }

            return token.Value;
        }

        public async Task<string> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true))?.AuthenticatorKey ?? user.AuthenticatorKey;
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

            var u = await _userCollection.FirstOrDefaultAsync(x => x.UserName == user.UserName).ConfigureAwait(true);
            if (u != null) return IdentityResult.Failed(new IdentityError { Code = "Username already in use" });

            await _userCollection.InsertOneAsync(user, InsertOneOptions, cancellationToken).ConfigureAwait(false);

            if (user.Email != null)
            {
                await SetEmailAsync(user, user.Email, cancellationToken).ConfigureAwait(false);
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            await _userCollection.DeleteOneAsync(x => x.Id.Equals(user.Id), cancellationToken).ConfigureAwait(false);
            return IdentityResult.Success;
        }

        public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return ByIdAsync(ConvertIdFromString(userId), cancellationToken);
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
            await _userCollection.ReplaceOneAsync(x => x.Id.Equals(user.Id), user, ReplaceOptions, cancellationToken).ConfigureAwait(false);

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

                await AddAsync(user, x => x.Claims, identityClaim, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (claim == null) throw new ArgumentNullException(nameof(claim));
            if (newClaim == null) throw new ArgumentNullException(nameof(newClaim));

            cancellationToken.ThrowIfCancellationRequested();

            user.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            user.Claims.Add(new IdentityUserClaim<string>()
            {
                ClaimType = newClaim.Type,
                ClaimValue = newClaim.Value
            });

            await UpdateAsync(user, x => x.Claims, user.Claims, cancellationToken).ConfigureAwait(false);
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (claims == null) throw new ArgumentNullException(nameof(claims));

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var claim in claims)
            {
                user.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
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

            var dbUser = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);
            return dbUser?.Claims?.Select(x => new Claim(x.ClaimType, x.ClaimValue))?.ToList() ?? new List<Claim>();
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

            return (await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(false))?.Email ?? user.Email;
        }

        public async Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(false))?.EmailConfirmed ?? user.EmailConfirmed;
        }

        public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return _userCollection.FirstOrDefaultAsync(a => a.NormalizedEmail == normalizedEmail, cancellationToken);
        }

        public async Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true))?.NormalizedEmail ?? user.NormalizedEmail;
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

            cancellationToken.ThrowIfCancellationRequested();

            user.NormalizedEmail = normalizedEmail ?? _normalizer.NormalizeEmail(user.Email);

            return UpdateAsync(user, x => x.NormalizedEmail, user.NormalizedEmail, cancellationToken);
        }

        public async Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            await SetNormalizedEmailAsync(user, _normalizer.NormalizeEmail(user.Email), cancellationToken).ConfigureAwait(false);

            user.Email = email;

            await UpdateAsync(user, x => x.Email, user.Email, cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(false))?.AccessFailedCount ?? user.AccessFailedCount;
        }

        public async Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(false))?.LockoutEnabled ?? user.LockoutEnabled;
        }

        public async Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            user.AccessFailedCount++;
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

            return (await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(false))?.LockoutEnd ?? user.LockoutEnd;
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

            return AddAsync(user, x => x.Logins, iul, cancellationToken);
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

            return await _userCollection.FirstOrDefaultAsync(u =>
                u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey), cancellationToken).ConfigureAwait(true);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var dbUser = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);

            return dbUser?.Logins?.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName))?.ToList() ?? new List<UserLoginInfo>();
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

            return (await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true))?.PasswordHash != null;
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            user.PasswordHash = passwordHash;

            return UpdateAsync(user, x => x.PasswordHash, passwordHash, cancellationToken);
        }

        public async Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true))?.PhoneNumber;
        }

        public async Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true))?.PhoneNumberConfirmed ?? false;
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

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

            cancellationToken.ThrowIfCancellationRequested();

            var role = await _roleStore.FindByNameAsync(roleName, cancellationToken).ConfigureAwait(true);
            if (role == null) return;

            user.Roles.Add(role.Id.ToString());

            await UpdateAsync(user, x => x.Roles, user.Roles, cancellationToken).ConfigureAwait(false);
        }

        public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var role = await _roleStore.FindByNameAsync(roleName, cancellationToken).ConfigureAwait(true);

            if (role == null) return;

            user.Roles.Remove(role.Id.ToString());

            await UpdateAsync(user, x => x.Roles, user.Roles, cancellationToken).ConfigureAwait(false);
        }


        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var role = await _roleStore.FindByNameAsync(roleName, cancellationToken).ConfigureAwait(true);
            if (role == null) return new List<TUser>();

            var filter = Builders<TUser>.Filter.AnyEq(x => x.Roles, role.Id.ToString());
            return (await _userCollection.FindAsync(filter, FindOptions, cancellationToken).ConfigureAwait(true)).ToList();
        }

        public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var userDb = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);

            if (userDb == null) return new List<string>();

            var roles = new List<string>();

            foreach (var item in userDb.Roles)
            {
                var dbRole = await _roleStore.FindByIdAsync(item, cancellationToken).ConfigureAwait(true);

                if (dbRole != null)
                {
                    roles.Add(dbRole.Name);
                }
            }
            return roles;
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var dbUser = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);

            var role = await _roleStore.FindByNameAsync(roleName, cancellationToken)
                .ConfigureAwait(true);

            if (role == null) return false;

            return dbUser?.Roles.Contains(role.Id.ToString()) ?? false;
        }

        public async Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return (await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true))?.SecurityStamp ?? user.SecurityStamp;
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

            var dbUser = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);

            if (dbUser == null) return false;

            var c = user.RecoveryCodes.FirstOrDefault(x => x.Code == code);

            if (c == null || c.Redeemed) return false;

            c.Redeemed = true;

            await UpdateAsync(user, x => x.RecoveryCodes, user.RecoveryCodes, cancellationToken).ConfigureAwait(false);

            return true;
        }

        public async Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var foundUser = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);

            return foundUser?.RecoveryCodes?.Count ?? user.RecoveryCodes.Count;
        }

        public async Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var foundUser = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);

            return foundUser?.TwoFactorEnabled ?? user.TwoFactorEnabled;
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
