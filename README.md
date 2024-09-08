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

## Dotnet Versions support

Starting from `v9.0.0` library only supports **.Net 6.0** and **.Net 8.0** as they are
only versions maintainable by Microsoft at the moment. [Supported Dotnet Versions](https://dotnet.microsoft.com/en-us/download/dotnet)

Library supports **.Net 6.0**, **.Net 5.0**, **.Net Core 3.1**, **.Net Core 2.1**
simultaneously started from 8.3.0 nuget package.

## MongoDB Indexes

**Important note!**

Starting from `v9.0.0` we no longer apply indexes on `"Users"` collection. Main reason for this change that
you are unable to change default indexes. If you delete index, it will appear again;
if you delete index and re-create it with different options, application won't start due to error.
You most likely have other indexes of your own, and now you have 2 places where they managed.
So it's up to user to decide which indexes should be used (if any), how and where manage them.

Here the old indexes in case someone needs them (collection name could be different):
```
db.Users.createIndex({ "NormalizedEmail" : 1 })
db.Users.createIndex({ "NormalizedUserName" : 1 })
```

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
