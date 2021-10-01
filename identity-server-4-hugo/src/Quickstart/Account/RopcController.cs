// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using IdentiyServer4Hugo.InternalLogin;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using IdentiyServer4Hugo.Services;

namespace IdentityServerHost.Quickstart.UI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class RopcController : Controller
    {
        private ITokenValidator _tokenValidator;

        public RopcController(
            ITokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        
        [HttpGet]
        public async Task<IActionResult> Login(string token)
        {
           var result = await _tokenValidator.ValidateAccessTokenAsync(token);
            
           AuthenticationProperties props = null;
            props = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
            };

            // issue authentication cookie with subject ID and username
            var isuser = new IdentityServerUser(result.Claims.SingleOrDefault(c => c.Type == "sub").Value)
            {
                DisplayName = result.Claims.SingleOrDefault(c => c.Type == "name").Value
            };

            await HttpContext.SignInAsync(isuser, props);

            return Content("logged in");
        }

    }
}
