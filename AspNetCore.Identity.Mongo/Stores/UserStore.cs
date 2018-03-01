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
    public class UserStore<T> : IUserStore<T>,
        IUserClaimStore<T>,
        IUserLoginStore<T>,
        IUserRoleStore<T>,
        IUserPasswordStore<T>,
        IUserSecurityStampStore<T>,
        IUserEmailStore<T>,
        IUserPhoneNumberStore<T>,
        IQueryableUserStore<T>,
        IUserTwoFactorStore<T>,
        IUserLockoutStore<T>,
        IUserAuthenticatorKeyStore<T>,
        IUserTwoFactorRecoveryCodeStore<T>,
        IRoleStore<MongoIdentityRole>,
        IQueryableRoleStore<MongoIdentityRole> where T: DBObject<T>, IMongoIdentityUser
    {
        public IQueryable<T> Users => DBObject<T>.Queryable();

        public async Task<IdentityResult> CreateAsync(T user, CancellationToken cancellationToken)
        {
            var u = await DBObject<T>.FirstOrDefaultAsync(x => x.UserName == user.UserName);
            if (u != null) return IdentityResult.Failed(new IdentityError {Code = "Username already in use"});

            await DBObject<T>.CreateAsync(user);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(T user, CancellationToken cancellationToken)
        {
            await DBObject<T>.DeleteAsync(user);
            return IdentityResult.Success;
        }

        public void Dispose()
        {
        }

        public Task<T> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
           return DBObject<T>.FirstOrDefaultAsync(x => x.Id == userId);
        }

        public Task<T> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return DBObject<T>.FirstOrDefaultAsync(x => x.NormalizedUserName == normalizedUserName);
        }

        public async Task<IdentityResult> UpdateAsync(T user, CancellationToken cancellationToken)
        {
            await DBObject<T>.UpdateAsync(user);
            return IdentityResult.Success;
        }

        public async Task AddClaimsAsync(T user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            foreach (var claim in claims)
            {
                await IdentityUserClaim.CreateAsync(new IdentityUserClaim(user.Id, claim));
            }
        }

        public async Task ReplaceClaimAsync(T user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            var c = await IdentityUserClaim.FirstOrDefaultAsync(x=>x.UserId == user.Id && x.Type == claim.Type);

            if (c == null) await IdentityUserClaim.CreateAsync(new IdentityUserClaim(user.Id,newClaim));

            await IdentityUserClaim.UpdateAsync(c, x => x.Value, newClaim.Value);
        }

        public async Task RemoveClaimsAsync(T user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            foreach (var claim in claims)
            {
                await IdentityUserClaim.DeleteAsync(x=>x.UserId == user.Id && x.Type == claim.Type);
            }
        }

        public async Task<IList<T>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            return (await IdentityUserClaim.WhereAsync(x => x.Type == claim.Type))
                .Select(cl => DBObject<T>.FirstOrDefault(user => user.Id == cl.UserId))
                .ToList();
        }

        public async Task AddLoginAsync(T user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            await IdentityUserLogin.CreateAsync(new IdentityUserLogin
            {
                UserId = user.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName,
                ProviderKey = login.ProviderKey
            });
        }

        public async Task RemoveLoginAsync(T user, string loginProvider, string providerKey,CancellationToken cancellationToken)
        {
            await IdentityUserLogin.DeleteAsync(x => x.UserId == user.Id && x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
        }

        public async Task<T> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var claim = await IdentityUserLogin.FirstOrDefaultAsync(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);

            return claim == null ? null : DBObject<T>.FirstOrDefault(x => x.Id == claim.Id);
        }

        public async Task AddToRoleAsync(T user, string roleName, CancellationToken cancellationToken)
        {
            var role = await MongoIdentityRole.FirstOrDefaultAsync(x => x.NormalizedName == roleName) ?? await MongoIdentityRole.CreateAsync(new MongoIdentityRole(roleName));

            await RoleMembership.CreateAsync(new RoleMembership{ RoleId = role.Id, UserId = user.Id});
        }

        public async Task RemoveFromRoleAsync(T user, string roleName, CancellationToken cancellationToken)
        {
            var role = await MongoIdentityRole.FirstOrDefaultAsync(x => x.Name == roleName);

            if (role != null)
                await RoleMembership.DeleteAsync(x => x.UserId == user.Id && x.RoleId == role.Id);
        }


        public Task<string> GetNormalizedUserNameAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetSecurityStampAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task<string> GetUserIdAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public async Task<IList<Claim>> GetClaimsAsync(T user, CancellationToken cancellationToken)
        {
            return (await IdentityUserClaim.WhereAsync(x => x.UserId == user.Id))
                .Select(x => x.ToSecurityClaim())
                .ToList();
        }


        public async Task<IList<T>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var role = await MongoIdentityRole.FirstOrDefaultAsync(x => x.NormalizedName == roleName);

            if(role == null) return new List<T>();

            return (await RoleMembership.WhereAsync(x => x.RoleId == role.Id))
                    .Select(membership=> DBObject<T>.FirstOrDefault(x=>x.Id == membership.UserId))
                    .ToList();
        }

        public async Task<IList<string>> GetRolesAsync(T user, CancellationToken cancellationToken)
        {
            var memberships = (await RoleMembership.WhereAsync(x => x.UserId == user.Id));
            var roles = memberships.Select(
                async ms => (await MongoIdentityRole.FirstOrDefaultAsync(x => x.Id == ms.RoleId))?.Name);

            var list = new List<string>();
            foreach (var role in roles)
            {
                list.Add(await role);
            }
            return list;
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(T user, CancellationToken cancellationToken)
        {
            return (await IdentityUserLogin.WhereAsync(x => x.UserId == user.Id))
                .Select(x => x.ToUserLoginInfo())
                .ToList();
        }

        public Task<string> GetPasswordHashAsync(T user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public async Task<string> GetEmailAsync(T user, CancellationToken cancellationToken)
        {
            return (await DBObject<T>.FirstOrDefaultAsync(x => x.Id == user.Id))?.Email;
        }

        public async Task<bool> GetEmailConfirmedAsync(T user, CancellationToken cancellationToken)
        {
            return (await DBObject<T>.FirstOrDefaultAsync(x => x.Id == user.Id))?.EmailConfirmed ?? false;
        }

        public async Task<T> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await DBObject<T>.FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);
        }

        public async Task<int> GetAccessFailedCountAsync(T user, CancellationToken cancellationToken)
        {
            return (await DBObject<T>.FirstOrDefaultAsync(x => x.Id == user.Id))?.AccessFailedCount ?? 0;
        }

        public async Task<bool> GetLockoutEnabledAsync(T user, CancellationToken cancellationToken)
        {
            return (await DBObject<T>.FirstOrDefaultAsync(x => x.Id == user.Id))?.LockoutEnabled ?? false;
        }

        public async Task<string> GetNormalizedEmailAsync(T user, CancellationToken cancellationToken)
        {
            return (await DBObject<T>.FirstOrDefaultAsync(x => x.Id == user.Id))?.NormalizedEmail;
        }

        public async Task<string> GetPhoneNumberAsync(T user, CancellationToken cancellationToken)
        {
            return (await DBObject<T>.FirstOrDefaultAsync(x => x.Id == user.Id))?.PhoneNumber;
        }

        public async Task<string> GetAuthenticatorKeyAsync(T user, CancellationToken cancellationToken)
        {
            return (await DBObject<T>.FirstOrDefaultAsync(x => x.Id == user.Id))?.AuthenticatorKey;
        }

        public async Task<bool> GetPhoneNumberConfirmedAsync(T user, CancellationToken cancellationToken)
        {
            return (await DBObject<T>.FirstOrDefaultAsync(x => x.Id == user.Id))?.PhoneNumberConfirmed ?? false;
        }

        public async Task<bool> GetTwoFactorEnabledAsync(T user, CancellationToken cancellationToken)
        {
            return (await DBObject<T>.FirstOrDefaultAsync(x => x.Id == user.Id))?.TwoFactorEnabled ?? false;
        }

        public async Task<bool> IsInRoleAsync(T user, string roleName, CancellationToken cancellationToken)
        {
            var role = await MongoIdentityRole.FirstOrDefaultAsync(x => x.NormalizedName == roleName);
            if (role == null) return false;

            return await RoleMembership.AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id);
        }

        public async Task<bool> HasPasswordAsync(T user, CancellationToken cancellationToken)
        {
            return (await DBObject<T>.FirstOrDefaultAsync(x => x.Id == user.Id))?.PasswordHash != null;
        }

        public async Task<int> IncrementAccessFailedCountAsync(T user, CancellationToken cancellationToken)
        {
            await DBObject<T>.IncreaseAsync(user, x => x.AccessFailedCount, 1);
            return user.AccessFailedCount++;
        }
        public Task ResetAccessFailedCountAsync(T user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;
            return DBObject<T>.UpdateAsync(user, x => x.AccessFailedCount, 0);
        }

        public Task SetEmailConfirmedAsync(T user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return DBObject<T>.UpdateAsync(user, x => x.EmailConfirmed, confirmed);
        }

        public Task SetNormalizedEmailAsync(T user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return DBObject<T>.UpdateAsync(user, x => x.NormalizedEmail, normalizedEmail);
        }

        public Task SetPhoneNumberAsync(T user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return DBObject<T>.UpdateAsync(user, x => x.PhoneNumber, phoneNumber);
        }

        public Task SetPhoneNumberConfirmedAsync(T user, bool confirmed, CancellationToken cancellationToken)
        {
            user.PhoneNumberConfirmed = confirmed;
            return DBObject<T>.UpdateAsync(user, x => x.PhoneNumberConfirmed, confirmed);
        }

        public Task SetTwoFactorEnabledAsync(T user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return DBObject<T>.UpdateAsync(user, x => x.TwoFactorEnabled, enabled);
        }

        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(T user, CancellationToken cancellationToken)
        {
            return (await DBObject<T>.FirstOrDefaultAsync(x => x.Id == user.Id))?.LockoutEndDateUtc;
        }

        public Task SetLockoutEndDateAsync(T user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            user.LockoutEndDateUtc = lockoutEnd;
            return DBObject<T>.UpdateAsync(user, x => x.LockoutEndDateUtc, lockoutEnd);
        }

        public Task SetLockoutEnabledAsync(T user, bool enabled, CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;
            return DBObject<T>.UpdateAsync(user, x => x.LockoutEnabled, enabled);
        }

        public Task SetAuthenticatorKeyAsync(T user, string key, CancellationToken cancellationToken)
        {
            user.AuthenticatorKey = key;
            return DBObject<T>.UpdateAsync(user, x => x.AuthenticatorKey, key);
        }

        public Task SetPasswordHashAsync(T user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task SetNormalizedUserNameAsync(T user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetSecurityStampAsync(T user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(T user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public Task SetEmailAsync(T user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return DBObject<T>.UpdateAsync(user, x => x.Email, email);
        }

        public async Task ReplaceCodesAsync(T user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            await TwoFactorRecoveryCode.DeleteAsync(x => x.UserId == user.Id);

            foreach (var code in recoveryCodes)
            {
                await TwoFactorRecoveryCode.CreateAsync(
                    new TwoFactorRecoveryCode { Code = code, UserId = user.Id, Redeemed = false });
            }
        }

        public async Task<bool> RedeemCodeAsync(T user, string code, CancellationToken cancellationToken)
        {
            var c = await TwoFactorRecoveryCode.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Code == code);

            if (c == null || c.Redeemed) return false;
            {
                await TwoFactorRecoveryCode.UpdateAsync(c, x => x.Redeemed, true);
                return true;
            }
        }

        public async Task<int> CountCodesAsync(T user, CancellationToken cancellationToken)
        {
            return (int) await TwoFactorRecoveryCode.CountAsync(x => x.UserId == user.Id);
        }

        IQueryable<MongoIdentityRole> IQueryableRoleStore<MongoIdentityRole>.Roles => MongoIdentityRole.Queryable();

        async Task<IdentityResult> IRoleStore<MongoIdentityRole>.CreateAsync(MongoIdentityRole role, CancellationToken cancellationToken)
        {
            if (!MongoIdentityRole.Any(x => x.NormalizedName == role.NormalizedName)) await MongoIdentityRole.CreateAsync(role);

            return IdentityResult.Success;
        }

        async Task<IdentityResult> IRoleStore<MongoIdentityRole>.UpdateAsync(MongoIdentityRole role, CancellationToken cancellationToken)
        {
            await MongoIdentityRole.UpdateAsync(role);
            return IdentityResult.Success;
        }

        async Task<IdentityResult> IRoleStore<MongoIdentityRole>.DeleteAsync(MongoIdentityRole role, CancellationToken cancellationToken)
        {
            await MongoIdentityRole.DeleteAsync(role);
            await RoleMembership.DeleteAsync(x => x.RoleId == role.Id);

            return IdentityResult.Success;
        }

        Task<string> IRoleStore<MongoIdentityRole>.GetRoleIdAsync(MongoIdentityRole role, CancellationToken cancellationToken) => Task.FromResult(role.Id);

        Task<string> IRoleStore<MongoIdentityRole>.GetRoleNameAsync(MongoIdentityRole role, CancellationToken cancellationToken) => Task.FromResult(role.Name);

        async Task IRoleStore<MongoIdentityRole>.SetRoleNameAsync(MongoIdentityRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            await MongoIdentityRole.UpdateAsync(role, x => x.Name, roleName);
        }

        Task<string> IRoleStore<MongoIdentityRole>.GetNormalizedRoleNameAsync(MongoIdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        async Task IRoleStore<MongoIdentityRole>.SetNormalizedRoleNameAsync(MongoIdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            await MongoIdentityRole.UpdateAsync(role, x => x.NormalizedName, normalizedName);
        }

        Task<MongoIdentityRole> IRoleStore<MongoIdentityRole>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return MongoIdentityRole.FirstOrDefaultAsync(x => x.Id == roleId);
        }

        Task<MongoIdentityRole> IRoleStore<MongoIdentityRole>.FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return MongoIdentityRole.FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName);
        }

        void IDisposable.Dispose()
        {

        }
    }
}
