using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentiyServer4Hugo
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
                }
            };
        }

        public static IEnumerable<Client> GetClients(List<Client> configClients)
        {
            // client credentials client
            var clients = new List<Client>();
            loadClientsFromConfig(clients, configClients);
            return clients;
        }

        private static void loadClientsFromConfig(List<Client> builtInClients, List<Client> clientsInConfig)
        {
            if (clientsInConfig != null)
            {
                foreach (var c in clientsInConfig)
                {
                    if (c.ClientSecrets != null)
                    {
                        // secrets are in clear text in config, I have to provide the hash of them
                        var newSecrets = new List<Secret>();
                        foreach (var s in c.ClientSecrets)
                        {
                            if (s.Value != null)
                            {
                                newSecrets.Add(new Secret(s.Value.Sha256()));
                            }
                        }

                        c.ClientSecrets = newSecrets;
                    }
                    builtInClients.Add(c);
                }
            }
        }

        public static IEnumerable<Client> GetClients()
        {
            var gt = new List<string>(GrantTypes.Implicit);
            gt.Add(GrantType.ResourceOwnerPassword);

            var gt2 = new List<string>();
            gt2.Add(GrantType.ResourceOwnerPassword);
            gt2.Add(GrantType.Hybrid);
            return new List<Client>
            {
                new Client
                {
                    ClientId = "main",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.Implicit,

                    // where to redirect to after login
                    RedirectUris = { 
                        "https://localhost:44307/signin-oidc",
                        "https://your-demo-app/signin-oidc"
                    },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { 
                        "https://localhost:44307/signout-callback-oidc",
                        "https://your-demo-app/signout-callback-oidc"
                    },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }
                },

                new Client
                {
                    ClientId = "docs",
                    ClientName = "Docs Client",
                    AllowedGrantTypes = GrantTypes.Implicit,

                    // where to redirect to after login
                    RedirectUris = {
                        "https://localhost:44330/signin-oidc",
                        "https://your-docs-host/signin-oidc"
                    },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = {
                        "https://localhost:44330/signout-callback-oidc",
                        "https://your-docs-host/signout-callback-oidc"
                    },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }
                },
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
                }
            };
        }
    }
}
