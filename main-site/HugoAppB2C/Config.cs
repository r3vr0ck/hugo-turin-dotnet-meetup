using System.Collections.Generic;
using IdentityServer4.Models;

namespace HugoAppB2C
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
                //new ApiScope("mobile"), 
                new ApiScope("testscope"),
                new ApiScope("identityserver.read"),
                new ApiScope("identityserver.write"),
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>();
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
    }
}
