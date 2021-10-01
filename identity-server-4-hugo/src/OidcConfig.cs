using System.Collections.Generic;
using IdentityServer4.Models;

namespace IdentiyServer4Hugo
{
    public class OidcConfig
    {
        public string B2CSignInSignOnFlowSchemeName { get; set; } = "b2c";
        public string B2CSignInSignOnFlow_2_SchemeName { get; set; } = "b2c_2";
        public string B2CResetPasswordSchemeName { get; set; } = "b2c_reset";
        public string B2CSignInSignOnFlowMetadataAddress { get; set; }
        public string B2CSignInSignOnFlow_2_MetadataAddress { get; set; }
        public string B2CResetPasswordFlowMetadataAddress { get; set; }
        public string B2BMetadataAddress { get; set; }
        public string B2CClientId { get; set; }
        public string B2CClientSecret { get; set; }
        public string B2BClientId { get; set; }
        public string B2BClientSecret { get; set; }
        public string ThisIdentityServerMetadataAddress { get; set; }

        public string B2CBackOfficeClientId { get; set; }
        public string B2CBackOfficeClientSecret { get; set; }
        public string B2cExtensionAppClientId { get; set; }
        public string B2CTenantName { get; set; }

        public List<Client> Clients { get; set; } = new List<Client>();
        public int SleepAfterPwdAutomaticResetInSeconds { get; set; } = 5;
    }
}

