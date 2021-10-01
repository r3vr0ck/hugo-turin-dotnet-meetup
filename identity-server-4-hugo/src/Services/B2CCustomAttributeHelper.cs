using Microsoft.Extensions.Options;

namespace IdentiyServer4Hugo.Services
{
    public interface IB2CCustomAttributeHelper
    {
        public string GetCompleteAttributeName(string attributeName);
    }
    public class B2CCustomAttributeHelper : IB2CCustomAttributeHelper
    {
        private readonly string _b2CExtensionAppClientId;

        public B2CCustomAttributeHelper(IOptions<OidcConfig> oidcConfig)
        {
            _b2CExtensionAppClientId = oidcConfig.Value.B2cExtensionAppClientId.Replace("-", "");
        }

        public string GetCompleteAttributeName(string attributeName)
        {
            if (string.IsNullOrWhiteSpace(attributeName))
            {
                throw new System.ArgumentException("Parameter cannot be null", nameof(attributeName));
            }

            return $"extension_{_b2CExtensionAppClientId}_{attributeName}";
        }
    }
}