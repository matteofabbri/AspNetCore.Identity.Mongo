# AspNetCore.Identity.Mongo

This is a MongoDB provider for the ASP.NET Core Identity framework. It is completely written from scratch and provides support for all Identity framework interfaces:

* IUserClaimStore
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
* IUserAuthenticationTokenStore
* IUserTwoFactorRecoveryCodeStore
* IProtectedUserStore
* IRoleStore
* IRoleClaimStore
* IQueryableRoleStore

## Dot Net Core 2.2 and 3.0

[![NuGet](https://img.shields.io/nuget/v/AspNetCore.Identity.Mongo.svg)](https://www.nuget.org/packages/AspNetCore.Identity.Mongo/)

For 2.2 use Nuget packages of the 5 series ( latest 5.3 )

For 3.0 (3.1) use Nuget packages started from 6 series

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

## Migration guide to version 6.7.0+
Started from version 6.7.0 library has new functionality and improvements which can broke you current projects.<br>
[There](./docs/MigrationGuideToVersion6_7_0AndUpper.md) you can find information how to migrate to newest version.

## License
This project is licensed under the [MIT license](./blob/master/LICENSE.txt)