namespace AuthFlowHugoApp
{
    public class OidcConfig
    {
        public string MetaDataAddress { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string RedirectUrl { get; set; }

        public string LogOutRedirectUrl { get; set; }

        public bool EnableAnonymousAccess { get; set; } = false;
    }
}
