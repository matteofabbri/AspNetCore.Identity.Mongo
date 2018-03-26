using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Mongolino;

namespace AspNetCore.Identity.Mongo.Stores
{
    public class UserStore<TUser> :
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
        IUserTwoFactorRecoveryCodeStore<TUser> where TUser : MongoIdentityUser
    {
        private readonly Collection<TUser> _userCollection;

        public UserStore(Collection<TUser> userCollection)
        {
            _userCollection = userCollection;
        }

        public IQueryable<TUser> Users => _userCollection.Queryable();

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            var u = await _userCollection.FirstOrDefaultAsync(x => x.UserName == user.UserName);
            if (u != null) return IdentityResult.Failed(new IdentityError {Code = "Username already in use"});

            await _userCollection.CreateAsync(user);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            await _userCollection.DeleteAsync(user);
            return IdentityResult.Success;
        }

        public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return _userCollection.FirstOrDefaultAsync(x => x.Id == userId);
        }

        public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return _userCollection.FirstOrDefaultAsync(x => x.NormalizedUserName == normalizedUserName);
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            await _userCollection.UpdateAsync(user);
            return IdentityResult.Success;
        }

        public async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (user.Claims == null) user.Claims = new List<IdentityUserClaim>();

            user.Claims.AddRange(claims.Select(claim => new IdentityUserClaim(claim)));
            await _userCollection.UpdateAsync(user);
        }

        public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim,
            CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);
            user?.Claims?.RemoveAll(x => x.Type == claim.Type);
            dbUser?.Claims?.RemoveAll(x => x.Type == claim.Type);

            await _userCollection.UpdateAsync(dbUser);
        }

        public async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);

            foreach (var claim in claims)
            {
                user?.Claims?.RemoveAll(x => x.Type == claim.Type);
                dbUser?.Claims?.RemoveAll(x => x.Type == claim.Type);
            }

            await _userCollection.UpdateAsync(dbUser);
        }

        public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            return (await _userCollection.WhereAsync(u =>
                u.Claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))).ToList();
        }

        public async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            if (user.Logins == null) user.Logins = new List<IdentityUserLogin>();

            user.Logins.Add(new IdentityUserLogin
            {
                UserId = user.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName,
                ProviderKey = login.ProviderKey
            });

            await _userCollection.UpdateAsync(user);
        }

        public async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);
            user.Logins.RemoveAll(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
            dbUser.Logins.RemoveAll(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);

            await _userCollection.UpdateAsync(dbUser);
        }

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            return await _userCollection.FirstOrDefaultAsync(u =>
                u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey));
        }

        public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            await _userCollection.AddToAsync(user, x => x.Roles, roleName);
        }

        public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);
            dbUser.Roles.Remove(roleName);

            await _userCollection.UpdateAsync(dbUser);
        }


        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);
            return dbUser?.Claims?.Select(x => x.ToSecurityClaim())?.ToList() ?? new List<Claim>();
        }


        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            return (await _userCollection.AnyEqualAsync(x => x.Roles, roleName)).ToList();
        }

        public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            return (await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id))?.Roles ?? new List<string>();
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);
            return dbUser?.Logins?.Select(x => x.ToUserLoginInfo())?.ToList() ?? new List<UserLoginInfo>();
        }

        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public async Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            return (await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id))?.Email;
        }

        public async Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            return (await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id))?.EmailConfirmed ?? false;
        }

        public async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await _userCollection.FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);
        }

        public async Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            return (await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id))?.AccessFailedCount ?? 0;
        }

        public async Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            return (await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id))?.LockoutEnabled ?? false;
        }

        public async Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            return (await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id))?.NormalizedEmail;
        }

        public async Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
        {
            return (await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id))?.PhoneNumber;
        }

        public async Task<string> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken)
        {
            return (await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id))?.AuthenticatorKey;
        }

        public async Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            return (await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id))?.PhoneNumberConfirmed ?? false;
        }

        public async Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            return (await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id))?.TwoFactorEnabled ?? false;
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);
            return dbUser?.Roles.Contains(roleName) ?? false;
        }

        public async Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
        {
            return (await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id))?.PasswordHash != null;
        }

        public async Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            await _userCollection.IncreaseAsync(user, x => x.AccessFailedCount, 1);
            return user.AccessFailedCount++;
        }

        public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;
            return _userCollection.UpdateAsync(user, x => x.AccessFailedCount, 0);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return _userCollection.UpdateAsync(user, x => x.EmailConfirmed, confirmed);
        }

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return _userCollection.UpdateAsync(user, x => x.NormalizedEmail, normalizedEmail);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return _userCollection.UpdateAsync(user, x => x.PhoneNumber, phoneNumber);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.PhoneNumberConfirmed = confirmed;
            return _userCollection.UpdateAsync(user, x => x.PhoneNumberConfirmed, confirmed);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return _userCollection.UpdateAsync(user, x => x.TwoFactorEnabled, enabled);
        }

        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
        {
            return (await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id))?.LockoutEndDateUtc;
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            user.LockoutEndDateUtc = lockoutEnd;
            return _userCollection.UpdateAsync(user, x => x.LockoutEndDateUtc, lockoutEnd);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;
            return _userCollection.UpdateAsync(user, x => x.LockoutEnabled, enabled);
        }

        public Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken)
        {
            user.AuthenticatorKey = key;
            return _userCollection.UpdateAsync(user, x => x.AuthenticatorKey, key);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return _userCollection.UpdateAsync(user, x => x.Email, email);
        }

        public async Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes,
            CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);
            if (dbUser == null) return;

            user.RecoveryCodes = recoveryCodes.Select(x => new TwoFactorRecoveryCode {Code = x, Redeemed = false})
                .ToList();
            await _userCollection.UpdateAsync(dbUser);
        }

        public async Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);
            if (dbUser == null) return false;

            var c = user.RecoveryCodes.FirstOrDefault(x => x.Code == code);

            if (c == null || c.Redeemed) return false;

            c.Redeemed = true;
            await _userCollection.UpdateAsync(dbUser);
            return true;
        }

        public async Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);
            return dbUser?.RecoveryCodes.Count ?? 0;
        }

        public async Task SetTokenAsync(TUser user, string loginProvider, string name, string value,
            CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);
            if (dbUser == null) return;

            if (dbUser.Tokens == null) dbUser.Tokens = new List<IdentityUserToken>();

            var token = dbUser.Tokens.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name);

            if (token == null)
            {
                token = new IdentityUserToken {LoginProvider = loginProvider, Name = name, Value = value};
                dbUser.Tokens.Add(token);
            }
            else
            {
                token.Value = value;
            }

            await _userCollection.UpdateAsync(dbUser);
        }

        public async Task RemoveTokenAsync(TUser user, string loginProvider, string name,
            CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);
            if (dbUser?.Tokens == null) return;

            dbUser.Tokens.RemoveAll(x => x.LoginProvider == loginProvider && x.Name == name);

            await _userCollection.UpdateAsync(dbUser);
        }

        public async Task<string> GetTokenAsync(TUser user, string loginProvider, string name,
            CancellationToken cancellationToken)
        {
            var dbUser = await _userCollection.FirstOrDefaultAsync(x => x.Id == user.Id);
            return dbUser?.Tokens?.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name)?.Value;
        }

        void IDisposable.Dispose()
        {
        }
    }
}