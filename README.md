# Service Notice
> Im super greatful of all contributions that are coming to this repo and for the help that i'm recieving from this community.

> A very special thanks should go to **Vova3211** and **DNemtsov** which are preserving my menthal healt and spending lot of their times to answering your questions.

> However my condition of being an italian living in Czechia and working for a german company makes me the perfect trigger for Microsoft IP Geofence Alarm.

> For this reason is two damn months that Nuget.org dont let me publish the latest version of the library.
> The latest news is that i will be may able to access the web site to publish in 30 days, but they already said that other two time so I will not count so much on that

> I'm trying to get in touch with MS help team to solve this issue, but in the meanwhile if you want to use the latest version please checkout this repo instead on rely on Nuget packages

> **Picky sys-admin corner**: One of the problem is that nuget.org is not ever bothering to send me a confirmation email, and yes i checked it deep down to the wireshark level



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
services.AddIdentityMongoDbProvider<AspNetCore.Identity.Mongo.Model.MongoUser, AspNetCore.Identity.Mongo.Model.MongoRole>(identityOptions =>
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
[There](./docs/MigrationGuideToVersion6_7_0AndUpper.md) you can find information how to migrate from 6.0.0-6.3.5 to newest version.
[There](./docs/MigrationGuideFromVersion3_1_5ToVersion6_7_0AndUpper.md) you can find information how to migrate from 3.1.5 to newest version.

If you has different version of library and want to update it, just create new issue. We will try to help you or will create new instruction.

## License
This project is licensed under the [MIT license](./blob/master/LICENSE.txt)
