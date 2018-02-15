using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity.Mongo.Stores
{
    public class UserStore : IUserStore<ApplicationUser>,
        IUserClaimStore<ApplicationUser>,
        IUserLoginStore<ApplicationUser>,
        IUserRoleStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserSecurityStampStore<ApplicationUser>,
        IUserEmailStore<ApplicationUser>,
        IUserPhoneNumberStore<ApplicationUser>,
        IQueryableUserStore<ApplicationUser>,
        IUserTwoFactorStore<ApplicationUser>,
        IUserLockoutStore<ApplicationUser>,
        IUserAuthenticatorKeyStore<ApplicationUser>,
        IUserTwoFactorRecoveryCodeStore<ApplicationUser>,
        IRoleStore<ApplicationRole>,
        IQueryableRoleStore<ApplicationRole>
    {
        public IQueryable<ApplicationUser> Users => ApplicationUser.Queryable();

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var u = await ApplicationUser.FirstOrDefaultAsync(x => x.UserName == user.UserName);
            if (u != null) return IdentityResult.Failed(new IdentityError {Code = "Username already in use"});

            await ApplicationUser.CreateAsync(user);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            await ApplicationUser.DeleteAsync(user);
            return IdentityResult.Success;
        }

        public void Dispose()
        {
        }

        public Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
           return ApplicationUser.FirstOrDefaultAsync(x => x.Id == userId);
        }

        public Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return ApplicationUser.FirstOrDefaultAsync(x => x.NormalizedUserName == normalizedUserName);
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            await ApplicationUser.UpdateAsync(user);
            return IdentityResult.Success;
        }

        public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            foreach (var claim in claims)
            {
                await IdentityUserClaim.CreateAsync(new IdentityUserClaim(user.Id, claim));
            }
        }

        public async Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            var c = await IdentityUserClaim.FirstOrDefaultAsync(x=>x.UserId == user.Id && x.Type == claim.Type);

            if (c == null) await IdentityUserClaim.CreateAsync(new IdentityUserClaim(user.Id,newClaim));

            await IdentityUserClaim.UpdateAsync(c, x => x.Value, newClaim.Value);
        }

        public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            foreach (var claim in claims)
            {
                await IdentityUserClaim.DeleteAsync(x=>x.UserId == user.Id && x.Type == claim.Type);
            }
        }

        public async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            return (await IdentityUserClaim.WhereAsync(x => x.Type == claim.Type))
                .Select(cl => ApplicationUser.FirstOrDefault(user => user.Id == cl.UserId))
                .ToList();
        }

        public async Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            await IdentityUserLogin.CreateAsync(new IdentityUserLogin(user, login));
        }

        public async Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey,CancellationToken cancellationToken)
        {
            await IdentityUserLogin.DeleteAsync(x => x.UserId == user.Id && x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
        }

        public async Task<ApplicationUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var claim = await IdentityUserLogin.FirstOrDefaultAsync(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);

            return claim == null ? null : ApplicationUser.FirstOrDefault(x => x.Id == claim.Id);
        }

        public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            var role = await ApplicationRole.FirstOrDefaultAsync(x => x.NormalizedName == roleName) ?? await ApplicationRole.CreateAsync(new ApplicationRole(roleName));

            await RoleMembership.CreateAsync(new RoleMembership{ RoleId = role.Id, UserId = user.Id});
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            var role = await ApplicationRole.FirstOrDefaultAsync(x => x.Name == roleName);

            if (role != null)
                await RoleMembership.DeleteAsync(x => x.UserId == user.Id && x.RoleId == role.Id);
        }


        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await IdentityUserClaim.WhereAsync(x => x.UserId == user.Id))
                .Select(x => x.ToSecurityClaim())
                .ToList();
        }


        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var role = await ApplicationRole.FirstOrDefaultAsync(x => x.NormalizedName == roleName);

            if(role == null) return new List<ApplicationUser>();

            return (await RoleMembership.WhereAsync(x => x.RoleId == role.Id))
                    .Select(membership=> ApplicationUser.FirstOrDefault(x=>x.Id == membership.UserId))
                    .ToList();
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var memberships = (await RoleMembership.WhereAsync(x => x.UserId == user.Id));
            var roles = memberships.Select(
                async ms => (await ApplicationRole.FirstOrDefaultAsync(x => x.Id == ms.RoleId))?.Name);

            var list = new List<string>();
            foreach (var role in roles)
            {
                list.Add(await role);
            }
            return list;
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await IdentityUserLogin.WhereAsync(x => x.UserId == user.Id))
                .Select(x => x.ToUserLoginInfo())
                .ToList();
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public async Task<string> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await ApplicationUser.FirstOrDefaultAsync(x => x.Id == user.Id))?.Email;
        }

        public async Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await ApplicationUser.FirstOrDefaultAsync(x => x.Id == user.Id))?.EmailConfirmed ?? false;
        }

        public async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await ApplicationUser.FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);
        }

        public async Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await ApplicationUser.FirstOrDefaultAsync(x => x.Id == user.Id))?.AccessFailedCount ?? 0;
        }

        public async Task<bool> GetLockoutEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await ApplicationUser.FirstOrDefaultAsync(x => x.Id == user.Id))?.LockoutEnabled ?? false;
        }

        public async Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await ApplicationUser.FirstOrDefaultAsync(x => x.Id == user.Id))?.NormalizedEmail;
        }

        public async Task<string> GetPhoneNumberAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await ApplicationUser.FirstOrDefaultAsync(x => x.Id == user.Id))?.PhoneNumber;
        }

        public async Task<string> GetAuthenticatorKeyAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await ApplicationUser.FirstOrDefaultAsync(x => x.Id == user.Id))?.AuthenticatorKey;
        }

        public async Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await ApplicationUser.FirstOrDefaultAsync(x => x.Id == user.Id))?.PhoneNumberConfirmed ?? false;
        }

        public async Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await ApplicationUser.FirstOrDefaultAsync(x => x.Id == user.Id))?.TwoFactorEnabled ?? false;
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            var role = await ApplicationRole.FirstOrDefaultAsync(x => x.NormalizedName == roleName);
            if (role == null) return false;

            return await RoleMembership.AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id);
        }

        public async Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await ApplicationUser.FirstOrDefaultAsync(x => x.Id == user.Id))?.PasswordHash != null;
        }

        public async Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            await ApplicationUser.IncreaseAsync(user, x => x.AccessFailedCount, 1);
            return user.AccessFailedCount++;
        }
        public Task ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;
            return ApplicationUser.UpdateAsync(user, x => x.AccessFailedCount, 0);
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return ApplicationUser.UpdateAsync(user, x => x.EmailConfirmed, confirmed);
        }

        public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return ApplicationUser.UpdateAsync(user, x => x.NormalizedEmail, normalizedEmail);
        }

        public Task SetPhoneNumberAsync(ApplicationUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return ApplicationUser.UpdateAsync(user, x => x.PhoneNumber, phoneNumber);
        }

        public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.PhoneNumberConfirmed = confirmed;
            return ApplicationUser.UpdateAsync(user, x => x.PhoneNumberConfirmed, confirmed);
        }

        public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return ApplicationUser.UpdateAsync(user, x => x.TwoFactorEnabled, enabled);
        }

        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (await ApplicationUser.FirstOrDefaultAsync(x => x.Id == user.Id))?.LockoutEndDateUtc;
        }

        public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            user.LockoutEndDateUtc = lockoutEnd;
            return ApplicationUser.UpdateAsync(user, x => x.LockoutEndDateUtc, lockoutEnd);
        }

        public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;
            return ApplicationUser.UpdateAsync(user, x => x.LockoutEnabled, enabled);
        }

        public Task SetAuthenticatorKeyAsync(ApplicationUser user, string key, CancellationToken cancellationToken)
        {
            user.AuthenticatorKey = key;
            return ApplicationUser.UpdateAsync(user, x => x.AuthenticatorKey, key);
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return ApplicationUser.UpdateAsync(user, x => x.Email, email);
        }

        public async Task ReplaceCodesAsync(ApplicationUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            await TwoFactorRecoveryCode.DeleteAsync(x => x.UserId == user.Id);

            foreach (var code in recoveryCodes)
            {
                await TwoFactorRecoveryCode.CreateAsync(
                    new TwoFactorRecoveryCode { Code = code, UserId = user.Id, Redeemed = false });
            }
        }

        public async Task<bool> RedeemCodeAsync(ApplicationUser user, string code, CancellationToken cancellationToken)
        {
            var c = await TwoFactorRecoveryCode.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Code == code);

            if (c == null || c.Redeemed) return false;
            {
                await TwoFactorRecoveryCode.UpdateAsync(c, x => x.Redeemed, true);
                return true;
            }
        }

        public async Task<int> CountCodesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return (int) await TwoFactorRecoveryCode.CountAsync(x => x.UserId == user.Id);
        }

        IQueryable<ApplicationRole> IQueryableRoleStore<ApplicationRole>.Roles => ApplicationRole.Queryable();

        async Task<IdentityResult> IRoleStore<ApplicationRole>.CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            if (!ApplicationRole.Any(x => x.NormalizedName == role.NormalizedName)) await ApplicationRole.CreateAsync(role);

            return IdentityResult.Success;
        }

        async Task<IdentityResult> IRoleStore<ApplicationRole>.UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            await ApplicationRole.UpdateAsync(role);
            return IdentityResult.Success;
        }

        async Task<IdentityResult> IRoleStore<ApplicationRole>.DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            await ApplicationRole.DeleteAsync(role);
            await RoleMembership.DeleteAsync(x => x.RoleId == role.Id);

            return IdentityResult.Success;
        }

        Task<string> IRoleStore<ApplicationRole>.GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        Task<string> IRoleStore<ApplicationRole>.GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        async Task IRoleStore<ApplicationRole>.SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            await ApplicationRole.UpdateAsync(role, x => x.Name, roleName);
        }

        Task<string> IRoleStore<ApplicationRole>.GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        async Task IRoleStore<ApplicationRole>.SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            await ApplicationRole.UpdateAsync(role, x => x.NormalizedName, normalizedName);
        }

        Task<ApplicationRole> IRoleStore<ApplicationRole>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return ApplicationRole.FirstOrDefaultAsync(x => x.Id == roleId);
        }

        Task<ApplicationRole> IRoleStore<ApplicationRole>.FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return ApplicationRole.FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName);
        }

        void IDisposable.Dispose()
        {

        }
    }
}
