using IdentityServer4.Models;
using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HugoAppB2C
{
    public class ProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {

            var ci = context.Subject.Identity as ClaimsIdentity;
            var externalUserIdentifier = ci.Claims.SingleOrDefault(c => c.Type == "externalUserIdentifier");
            if (externalUserIdentifier != null)
            {
                context.IssuedClaims.Add(new Claim("externalUserIdentifier", externalUserIdentifier.Value));
            }
            var newUserClaim = ci.Claims.SingleOrDefault(c => c.Type == "newUser");
            if (newUserClaim != null)
            {
                context.IssuedClaims.Add(new Claim("newUser", newUserClaim.Value));
            }
            var firstName = ci.Claims.SingleOrDefault(c => c.Type == "firstName");
            if (firstName != null)
            {
                context.IssuedClaims.Add(new Claim("firstName", firstName.Value));
            }

            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.CompletedTask;
        }
    }
}
