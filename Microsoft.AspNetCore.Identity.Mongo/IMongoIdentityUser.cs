using System;

namespace Microsoft.AspNetCore.Identity.Mongo
{
    public interface IMongoIdentityUser
    {
        string UserName { get; set; }
        string NormalizedUserName { get; set; }
        string SecurityStamp { get; set; }
        string Email { get; set; }
        string NormalizedEmail { get; set; }
        bool EmailConfirmed { get; set; }
        string PhoneNumber { get; set; }
        bool PhoneNumberConfirmed { get; set; }
        bool TwoFactorEnabled { get; set; }
        DateTimeOffset? LockoutEndDateUtc { get; set; }
        bool LockoutEnabled { get; set; }
        int AccessFailedCount { get; set; }
        string AuthenticatorKey { get; set; }
        string PasswordHash { get; set; }
        string Id { get; set; }
    }
}