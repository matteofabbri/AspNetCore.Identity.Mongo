# Microsoft.AspNetCore.Identity.Mongo

This is a MongoDB provider for the ASP.NET Core 2 Identity framework. It is completely written from scratch and provides support for all Identity framework interfaces:

* UserClaimStore
* IUserLoginStore
* IUserRoleStore
* IUserPasswordStore
* IUserSecurityStampStore
* IUserEmailStore
* IUserPhoneNumberStore
* IQueryableUserStore
* IUserTwoFactorStore
* IUserLockoutStore
* IUserAuthenticatorKeyStore
* IUserTwoFactorRecoveryCodeStore
* IRoleStore
* IQueryableRoleStore

## Dot Net Core 2.2 and 3.0 
For 2.2 use Nuget packages of the 5 series ( latest 5.3 )
For 3.0 use Nuget packages of the 6 series ( latest 6.3 )

## How to use:

```csharp
services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole>(identityOptions =>
{
    identityOptions.Password.RequiredLength = 6;
    identityOptions.Password.RequireLowercase = false;
    identityOptions.Password.RequireUppercase = false;
    identityOptions.Password.RequireNonAlphanumeric = false;
    identityOptions.Password.RequireDigit = false;
}, mongoIdentityOptions => {
    mongoIdentityOptions.ConnectionString = "mongodb://localhost/myDB";
});
```
