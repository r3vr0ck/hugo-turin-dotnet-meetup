using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IdentiyServer4Hugo.InternalLogin
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // from stored procedures
        public virtual DbSet<VerifyUserResult> VerifyUser { get; set; }
    }
}
