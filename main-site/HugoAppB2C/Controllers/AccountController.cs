using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HugoAppB2C.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login(string returnUrl = "")
        {
            var authenticationProperties = new AuthenticationProperties { RedirectUri = "Home/Privacy" };

            return Challenge(authenticationProperties, "oidc");
        }

        public IActionResult Signout()
        {
            return new SignOutResult(new[] { "oidc", CookieAuthenticationDefaults.AuthenticationScheme });
        }
    }
}
