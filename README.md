# Microsoft.AspNetCore.Identity.Mongo

This is a MongoDB provider for the ASP.NET Core 2 Identity framework.
Completly wrote from scratch provide support for all identity framework interfaces:

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

How to use


    services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole>("mongodb://localhost/maddalena", options =>
    {
            options.Password.RequiredLength = 6;
            
            options.Password.RequireLowercase = false;
            
            options.Password.RequireUppercase = false;
            
            options.Password.RequireNonAlphanumeric = false;
            
            options.Password.RequireDigit = false;
            
    });
    
