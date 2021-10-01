using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace IdentiyServer4Hugo.Services
{
    public interface IAzureB2CUserManageService
    {
        Task DisableUser(string userId);
        Task EnableUser(string userId);
        Task<bool> GetUserPasswordResetStatus(string userId);
        Task SetUserPasswordResetStatus(string userId, bool status);
    }

    public class AzureB2CUserManageService : IAzureB2CUserManageService
    {
        private readonly IB2CCustomAttributeHelper _b2CCustomAttributeHelper;
        private readonly OidcConfig _oidcConfig;

        public AzureB2CUserManageService(IOptions<OidcConfig> oidcConfig, IB2CCustomAttributeHelper b2CCustomAttributeHelper)
        {
            _b2CCustomAttributeHelper = b2CCustomAttributeHelper;
            _oidcConfig = oidcConfig.Value;
        }
        
        public async Task DisableUser(string userId)
        {

            await changeUserEnabledStatus(userId, false);



        }

        public async Task EnableUser(string userId)
        {
            await changeUserEnabledStatus(userId, true);
        }

        public async Task<bool> GetUserPasswordResetStatus(string userId)
        {
            var graphServiceClient = createGraphClient();

            var user = await GetUserById(graphServiceClient, userId, buildFullAttributeName("UserRequiresPasswordReset"));

            if (user == null)
            {
                throw new Exception(string.Format("Failed to get user with id {0}", userId));
            }
            if(!user.AdditionalData.TryGetValue(buildFullAttributeName("UserRequiresPasswordReset"), out var result))
            {
                return false;
            }
            return (bool) result;
        }

        public async Task SetUserPasswordResetStatus(string userId, bool status)
        {
            var graphServiceClient = createGraphClient();
            // Fill custom attributes
            var customAttribute = buildFullAttributeName("UserRequiresPasswordReset");

                // Update user
                await graphServiceClient.Users[userId]
                    .Request()
                    .UpdateAsync(new User {AdditionalData = new Dictionary<string, object>
                        {
                            {customAttribute, status}
                        }});
                return ;
        }

        private string buildFullAttributeName(string attributeName)
        {
            // Get the complete name of the custom attribute (Azure AD extension)
            return _b2CCustomAttributeHelper.GetCompleteAttributeName(attributeName);
        }
        private async Task changeUserEnabledStatus(string userId, bool enabled)
        {

            // Configure GraphServiceClient with provider.
            var graphServiceClient = createGraphClient();

            var user = await GetUserById(graphServiceClient, userId);

            if (user==null)
            {
                for (int retry = 0; retry < 5; retry++)
                {
                    await Task.Delay(1 * 1000);

                    user = await GetUserById(graphServiceClient, userId);

                    if (user != null)
                        break;
                }
            }

            if (user == null)
                throw new Exception(string.Format("Failed to get user with id {0}", userId));

            var upUser = new User
            {
                AccountEnabled = enabled
            };

            await graphServiceClient.Users[userId]
                .Request()
                .UpdateAsync(upUser);



        }

        private GraphServiceClient createGraphClient()
        {
                IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                    .Create(_oidcConfig.B2CClientId)
                    .WithTenantId(_oidcConfig.B2CTenantName)
                    .WithClientSecret(_oidcConfig.B2CClientSecret)
                    .Build();

                // Create an authentication provider.
                ClientCredentialProvider authenticationProvider = new ClientCredentialProvider(confidentialClientApplication);
                // Configure GraphServiceClient with provider.
                return new GraphServiceClient(authenticationProvider);
        }

        public async Task<User> GetUserById(GraphServiceClient graphClient,  string userId, params string[] customAttributes)
        {
                var select = $"id,displayName,identities";
                if (customAttributes != null)
                {
                    foreach (var customAttribute in customAttributes)
                    {
                        select += "," +customAttribute;
                    }
                }
                // Get user by object ID
                var user = await graphClient.Users[userId]
                    .Request()
                    .Select(select)
                    .GetAsync();

                return user;
          
        }
    }

    
}
