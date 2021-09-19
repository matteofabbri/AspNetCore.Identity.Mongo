# AspNetCore.Identity.Mongo [![NuGet](https://img.shields.io/nuget/v/AspNetCore.Identity.Mongo.svg)](https://www.nuget.org/packages/AspNetCore.Identity.Mongo/)

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

## Dot Net Core Versions support

Library supports **.Net 5.0**, **.Net Core 3.1**, **.Net Core 2.1**
simultaneously started from 8.3.0 nuget package.

## How to use:
AspNetCore.Identity.Mongo is installed from NuGet:
```
Install-Package AspNetCore.Identity.Mongo
```
The simplest way to set up:
```csharp
using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;

// At the ConfigureServices section in Startup.cs
services.AddIdentityMongoDbProvider<MongoUser>();
```

With Identity and Mongo options:
```csharp
using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;

// At the ConfigureServices section in Startup.cs
services.AddIdentityMongoDbProvider<MongoUser>(identity =>
   {
       identity.Password.RequiredLength = 8;
       // other options
   } ,
   mongo =>
   {
       mongo.ConnectionString = "mongodb://127.0.0.1:27017/identity";
       // other options
   });
```

Using User and Role models:
```csharp
using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;

// At the ConfigureServices section in Startup.cs
services.AddIdentityMongoDbProvider<MongoUser, MongoRole>(identity =>
    {
        identity.Password.RequiredLength = 8;
        // other options
    },
    mongo =>
    {
        mongo.ConnectionString = "mongodb://127.0.0.1:27017/identity";
        // other options
    });
```

Using different type of the primary key (default is `MongoDB.Bson.ObjectId`):
```csharp
using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;

public class ApplicationUser : MongoUser<string>
{
}

public class ApplicationRole : MongoRole<string>
{
}

// At the ConfigureServices section in Startup.cs
services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole, string>(identity =>
    {
        identity.Password.RequiredLength = 8;
        // other options
    },
    mongo =>
    {
        mongo.ConnectionString = "mongodb://127.0.0.1:27017/identity";
        // other options
    });
```
To add the stores only, use:
```csharp
using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;

// At the ConfigureServices section in Startup.cs
services
    .AddIdentityCore<MongoUser>()
    .AddRoles<MongoRole>()
    .AddMongoDbStores<MongoUser, MongoRole, ObjectId>(mongo =>
    {
        mongo.ConnectionString = "mongodb://127.0.0.1:27017/identity";
        // other options
    })
    .AddDefaultTokenProviders();
```

## Migration from lower versions
New releases could/will have the breaking changes.

Folder [docs](./docs) contains migration guides. E.g.:

[There](./docs/MigrationGuideToVersion6_7_0AndUpper.md) you can find information how to migrate from 6.0.0-6.3.5 to 6.7.x version.<br>
[There](./docs/MigrationGuideFromVersion3_1_5ToVersion6_7_0AndUpper.md) you can find information how to migrate from 3.1.5 to 6.7.x version.

If you have different version of the library and want to update it, please create a new issue. We will try to help you or will create new instruction.

## How to Contribute
Before create any issue/PR please look at the [CONTRIBUTING](./CONTRIBUTING.md)

## Code of conduct
See [CODE_OF_CONDUCT](./CODE_OF_CONDUCT.md)

## License
This project is licensed under the [MIT license](./blob/master/LICENSE.txt)
