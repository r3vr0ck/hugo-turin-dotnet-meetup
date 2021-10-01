using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthFlowHugoApp.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login(string returnUrl = "")
        {
            returnUrl = !string.IsNullOrEmpty(returnUrl) ? string.Format("?returnUrl={0}", returnUrl) : "";

            var authenticationProperties = new AuthenticationProperties { RedirectUri = string.Format("/Home/Login{0}", returnUrl) };
            
            return Challenge(authenticationProperties, "oidc");
        }

        public IActionResult Signout()
        {
            Response.Cookies.Delete("given_name", new CookieOptions()
            {
                Secure = true,
            });
            return new SignOutResult(new[] { "oidc", CookieAuthenticationDefaults.AuthenticationScheme });
        }
    }
}
