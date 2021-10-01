using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServerHost.Quickstart.UI
{
    public static class AzureAdPrincipalHelper
    {
        public const string ClaimsAzureAdB2BUserName = "preferred_username";
        public static string ClaimsAzureAdB2CUserName { get; } = "emails";

        public static string ClaimsAzureAdB2CName { get; } = "name";


        public static string GetClaimSafe(this IEnumerable<Claim> claims, string type)
        {
            var ret = claims.SingleOrDefault(c => c.Type == type);
            if (ret == null)
            {
                throw new Exception($"Could not find claim of type {type}");
            }

            return ret.Value;
        }
    }
}