using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IdentiyServer4Hugo.InternalLogin
{
    [Keyless]
    public partial class VerifyUserResult
    {
        public int Security_User_Id { get; set; }
        public string EMail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Company_Name { get; set; }
        public string Street { get; set; }
        public string Location { get; set; }
        public string State { get; set; }
        public string Postal_Code { get; set; }
        public string Country { get; set; }
        public Nullable<System.DateTime> Last_Logged_In { get; set; }
        public int Logical_Cancel_Value { get; set; }
    }
}
