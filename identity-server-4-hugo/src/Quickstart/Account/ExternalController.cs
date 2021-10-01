using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using IdentiyServer4Hugo;
using IdentiyServer4Hugo.B2cConfig;
using IdentiyServer4Hugo.Services;
using Microsoft.Extensions.Options;

namespace IdentityServerHost.Quickstart.UI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class ExternalController : Controller
    {
        private readonly IAzureB2CUserManageService _azureB2CUserManageService;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ILogger<ExternalController> _logger;
        private readonly OidcConfig _oidcConfig;
        private readonly B2cSettings _b2CSettings;
        private readonly IEventService _events;

        public ExternalController(IAzureB2CUserManageService azureB2CUserManageService,
            IIdentityServerInteractionService interaction,
            IEventService events,
            ILogger<ExternalController> logger,IOptions<OidcConfig> oidcConfig,
            IOptions<B2cSettings> b2cSettings)
        {
            _azureB2CUserManageService = azureB2CUserManageService;
            _interaction = interaction;
            _logger = logger;
            _oidcConfig = oidcConfig.Value;
            _b2CSettings = b2cSettings.Value;
            _events = events;
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        public IActionResult Challenge(string scheme, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }
            
            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)), 
                Items =
                {
                    { "returnUrl", returnUrl }, 
                    { "scheme", scheme },
                }
            };

            return Challenge(props, scheme);
            
        }

        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            return await internalCallback();
        }
        
        private async Task<IActionResult> internalCallback(bool skipMustChangePasswordCheck=false)
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                _logger.LogDebug("External claims: {@claims}", externalClaims);
            }

            if (!skipMustChangePasswordCheck & _b2CSettings.CheckForUserMustChangePassword && result.Properties?.Items["scheme"] == _oidcConfig.B2CSignInSignOnFlowSchemeName &&
                userMustChangePassword(result.Principal?.Claims))
            {
                var externalUser = result.Principal;
                if (externalUser == null)
                {
                    throw new Exception("unexpected: result.Principal==null");
                }

                var url  = result.Properties.Items["returnUrl"] ?? "~/";
                return await ChallengeResetPasswordB2c(url);
            }

            // this allows us to collect any additional claims or properties
            // for the specific protocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);
            var subClaim = result.Principal.Claims.SingleOrDefault(c=> c.Type == AzureAdPrincipalHelper.ClaimsAzureAdB2CName)?.Value;
            subClaim ??= result.Principal.Claims.SingleOrDefault(c => c.Type == AzureAdPrincipalHelper.ClaimsAzureAdB2BUserName)?.Value;
            var provider = result.Properties.Items["scheme"];
            // issue authentication cookie for user
            var isuser = new IdentityServerUser(subClaim)
            {
                DisplayName = buildUserName(result.Principal.Claims.ToList()),
                IdentityProvider = provider,
                AdditionalClaims = additionalLocalClaims
            };

            await HttpContext.SignInAsync(isuser, localSignInProps);

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // retrieve return URL
            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            // check if external login is in the context of an OIDC request
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, subClaim, subClaim, isuser.DisplayName
                , true, context?.Client.ClientId));

            if (context != null)
            {
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage("Redirect", returnUrl);
                }
            }
            
            return Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> ChallengeResetPasswordB2c(string returnUrl = "Home")
        {
            // var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(CallBackResetPassword)),
                Items =
                {
                    { "returnUrl", returnUrl },
                    { "scheme", _oidcConfig.B2CResetPasswordSchemeName  },
                }
            }, _oidcConfig.B2CResetPasswordSchemeName);
        }

        [HttpGet]
        public async Task<IActionResult> CallBackResetPassword()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }
            var userIdClaim = result.Principal.FindFirst(JwtClaimTypes.Subject) ??
                              result.Principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new Exception("unexpected: userIdClaim == null");
            }
            var userId = userIdClaim.Value;
            await _azureB2CUserManageService.SetUserPasswordResetStatus(userId, false);
            return await internalCallback(true);
        }
        private bool userMustChangePassword(IEnumerable<Claim> principalClaims)
        {
            return (principalClaims.SingleOrDefault(c => c.Type == "extension_UserRequiresPasswordReset")?.Value) == "true";
        }

        private string buildUserName(List<Claim> principalClaims)
        {
            var firstName = principalClaims.SingleOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            firstName ??= principalClaims.SingleOrDefault(c => c.Type == "given_name")?.Value;
            firstName ??= principalClaims.SingleOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            firstName ??= principalClaims.SingleOrDefault(c => c.Type == "name")?.Value;
            var lastName = principalClaims.SingleOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
            lastName ??= principalClaims.SingleOrDefault(c => c.Type == JwtClaimTypes.FamilyName)?.Value;
            return firstName + " " + lastName;
        }

        private bool isNewUser(IEnumerable<Claim> principalClaims)
        {
            return (principalClaims.SingleOrDefault(c => c.Type == "newUser")?.Value) == "true";
        }

        //private (TestUser user, string provider, string providerUserId, IEnumerable<Claim> claims) FindUserFromExternalProvider(AuthenticateResult result)
        //{
        //    var externalUser = result.Principal;

        //    // try to determine the unique id of the external user (issued by the provider)
        //    // the most common claim type for that are the sub claim and the NameIdentifier
        //    // depending on the external provider, some other claim type might be used
        //    var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
        //                      externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
        //                      throw new Exception("Unknown userid");

        //    // remove the user id claim so we don't include it as an extra claim if/when we provision the user
        //    var claims = externalUser.Claims.ToList();
        //    claims.Remove(userIdClaim);

        //    var provider = result.Properties.Items["scheme"];
        //    var providerUserId = userIdClaim.Value;

        //    // find external user
        //    var user = _users.FindByExternalProvider(provider, providerUserId);

        //    return (user, provider, providerUserId, claims);
        //}

        //private TestUser AutoProvisionUser(string provider, string providerUserId, IEnumerable<Claim> claims)
        //{
        //    var user = _users.AutoProvisionUser(provider, providerUserId, claims.ToList());
        //    return user;
        //}

        // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
        // this will be different for WS-Fed, SAML2p or other protocols
        private void ProcessLoginCallback(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var idToken = externalResult.Properties.GetTokenValue("id_token");
            if (idToken != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
            }
        }
    }
}