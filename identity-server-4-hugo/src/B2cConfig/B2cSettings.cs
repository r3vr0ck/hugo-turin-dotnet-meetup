using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentiyServer4Hugo.B2cConfig
{
    public class B2cSettings {
        public bool DisableB2cUSerAfterRegistration { get; set; }
        public bool CheckForUserMustChangePassword { get; set; } = true;
    }
}
