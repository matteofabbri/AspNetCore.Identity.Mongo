using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer4Demo
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
            };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                // backward compat
                new ApiScope("api"),
                
                // more formal
                new ApiScope("api.scope1"),
                new ApiScope("api.scope2"),
                
                // scope without a resource
                new ApiScope("scope2"),
                
                // policyserver
                new ApiScope("policyserver.runtime"),
                new ApiScope("policyserver.management")
            };
        }
        
        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "Demo API")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) },
                    
                    Scopes = { "api", "api.scope1", "api.scope2" }
                },

                // PolicyServer demo (audience should match scope)
                new ApiResource("policyserver.runtime")
                {
                    Scopes = { "policyserver.runtime" }
                },
                new ApiResource("policyserver.management")
                {
                    Scopes = { "policyserver.runtime" }
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                // non-interactive
                new Client
                {
                    ClientId = "m2m",
                    ClientName = "Machine to machine (client credentials)",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "api", "api.scope1", "api.scope2", "scope2", "policyserver.runtime", "policyserver.management" },
                },
                new Client
                {
                    ClientId = "m2m.short",
                    ClientName = "Machine to machine with short access token lifetime (client credentials)",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "api", "api.scope1", "api.scope2", "scope2" },
                    AccessTokenLifetime = 75
                },

                // interactive
                new Client
                {
                    ClientId = "interactive.confidential",
                    ClientName = "Interactive client (Code with PKCE)",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    AllowedScopes = { "openid", "profile", "email", "api", "api.scope1", "api.scope2", "scope2" },

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding
                },
                new Client
                {
                    ClientId = "interactive.confidential.short",
                    ClientName = "Interactive client with short token lifetime (Code with PKCE)",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    ClientSecrets = { new Secret("secret".Sha256()) },
                    RequireConsent = false,

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequirePkce = true,
                    AllowedScopes = { "openid", "profile", "email", "api", "api.scope1", "api.scope2", "scope2" },

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    
                    AccessTokenLifetime = 75
                },

                new Client
                {
                    ClientId = "interactive.public",
                    ClientName = "Interactive client (Code with PKCE)",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    RequireClientSecret = false,

                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = { "openid", "profile", "email", "api", "api.scope1", "api.scope2", "scope2" },

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding
                },
                new Client
                {
                    ClientId = "interactive.public.short",
                    ClientName = "Interactive client with short token lifetime (Code with PKCE)",

                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },

                    RequireClientSecret = false,

                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = { "openid", "profile", "email", "api", "api.scope1", "api.scope2", "scope2" },

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    
                    AccessTokenLifetime = 75
                },

                new Client
                {
                    ClientId = "device",
                    ClientName = "Device Flow Client",

                    AllowedGrantTypes = GrantTypes.DeviceFlow,
                    RequireClientSecret = false,

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    
                    AllowedScopes = { "openid", "profile", "email", "api", "api.scope1", "api.scope2", "scope2" }
                },
                
                // oidc login only
                new Client
                {
                    ClientId = "login",
                    
                    RedirectUris = { "https://notused" },
                    PostLogoutRedirectUris = { "https://notused" },
                    
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = { "openid", "profile", "email" },
                }
            };
        }
    }
}
