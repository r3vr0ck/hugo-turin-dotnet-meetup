using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using IdentityServer4;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using IdentiyServer4Hugo.B2cConfig;
using IdentiyServer4Hugo.InternalLogin;
using IdentiyServer4Hugo.Quickstart;
using IdentiyServer4Hugo.Services;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace IdentiyServer4Hugo
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


            services.AddScoped<Services.ITokenValidator, TokenValidator>();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            
            // cookie policy to deal with temporary browser incompatibilities
            services.AddSameSiteCookiePolicy();
            // configure identity server with in-memory stores, keys, clients and scopes
            services.AddIdentityServer()
            .AddDeveloperSigningCredential()
            .AddInMemoryIdentityResources(Config.GetIdentityResources())
            .AddInMemoryApiResources(Config.GetApis())
            .AddInMemoryClients(Config.GetClients())
            .AddTestUsers(TestUsers.Users);
        }

        public void Configure(IApplicationBuilder app)
        {

           
            app.UseIdentityServer();

            app.UseStaticFiles();
            
            
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });

        }
    }
}