using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthFlowHugoApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation();


            var section = Configuration.GetSection("OidcConfig");
            var oidcConfig = section.Get<OidcConfig>();
            services.Configure<OidcConfig>(section);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.SignInScheme = "Cookies";

                options.Authority = oidcConfig.MetaDataAddress;
                options.RequireHttpsMetadata = true;

                options.ClientId = oidcConfig.ClientId;
                options.SaveTokens = true;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();
            app.UseAuthentication();

            var section = Configuration.GetSection("OidcConfig");
            var oidcConfig = section.Get<OidcConfig>();

            app.UseDefaultFiles();

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Add("Cache-Control", "no-store");

                    var req = ctx.Context?.Request?.Path.Value;

                    if (!ctx.Context.User.Identity.IsAuthenticated && !oidcConfig.EnableAnonymousAccess)
                    {
                        req = req == "/index.html" ? "/docs/" : req;

                        var returnUrl = string.Empty;
                        if (!string.IsNullOrEmpty(req))
                        {
                            returnUrl = string.Format("?returnUrl={0}", req);
                        }
                        ctx.Context.Response.Redirect(string.Format("/Home/Login{0}", returnUrl));
                    }
                    else
                    {
                        if (ctx.Context.Request.Cookies["first_name"] == null)
                        {
                            ctx.Context.Response.Cookies.Append(
                                "given_name",
                                ctx.Context.User.Claims.FirstOrDefault(f => f.Type == "given_name")?.Value ?? ctx.Context.User.Identity.Name ?? "",
                                new CookieOptions()
                                {
                                    Path = "/",
                                    HttpOnly = false,
                                    Secure = false
                                }
                            );
                        }

                        if (req == "/index.html")
                        {
                            ctx.Context.Response.Redirect("/docs/");
                        }
                    }
                }
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "MyStaticFiles")),
                RequestPath = "/StaticFiles"
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Login}/{id?}");
            });
        }
    }
}
