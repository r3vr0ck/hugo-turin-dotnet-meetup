using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using TokenValidationResult = IdentityServer4.Validation.TokenValidationResult;

namespace IdentiyServer4Hugo.Services
{
    // THERE IS NO NEED FOR THSI TO VALIDATE JWT EMITTED BY Identity server,
    // There is an existing ITokenValidator implementation that does it ut of the box
    public interface ITokenValidator
    {
        Task<TokenValidationResult> ValidateAccessTokenAsync(string token,
            string expectedScope = null);
    }

    public class TokenValidator : ITokenValidator
    {
        private readonly IOptions<OidcConfig> _oidcOptions;

        public TokenValidator(IOptions<OidcConfig> oidcOptions)
        {
            _oidcOptions = oidcOptions;
        }
        public async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            var result = new TokenValidationResult();
            try { 
                
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                // .well-known/oauth-authorization-server or .well-known/openid-configuration
                _oidcOptions.Value.ThisIdentityServerMetadataAddress,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());

                var discoveryDocument = await configurationManager.GetConfigurationAsync();
                var signingKeys = discoveryDocument.SigningKeys;

                var handler = new JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear();

                var validationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    RequireAudience = false,
                    SaveSigninToken = false,
                    //TryAllIssuerSigningKeys = true,
                    ValidateActor = false,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = false,
                    ValidateLifetime = true,
                    ValidateTokenReplay = false,

                   IssuerSigningKeys = signingKeys,
                };
           
                var claimsPrincipal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, validationParameters, out var rawValidatedToken);

                var rawToken = (JwtSecurityToken)rawValidatedToken;
                result.Claims = rawToken.Claims;
                result.Jwt = token;
                result.IsError= false;
            }
            catch (SecurityTokenValidationException stvex)
            {
                result.IsError = true;
                result.ErrorDescription = $"Token failed validation: {stvex.ToString()}";
            }
            catch (ArgumentException argex)
            {
                // The token was not well-formed or was invalid for some other reason.
                // TODO: Log it or display an error.
                result.IsError = true;
                result.ErrorDescription = $"Token was invalid: {argex.Message}";
            }
            catch (Exception ex)
            {
                // The token was not well-formed or was invalid for some other reason.
                // TODO: Log it or display an error.
                result.IsError = true;
                result.ErrorDescription = $"Generic error in Token validation: {ex.ToString()}";
            }
            return result;
        }



    }
}