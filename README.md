# About the project 
The goal of this POC is to provide a seamless user navigation across the two main websites: the main website (main-site) and the documentation website (documentation-host).

The proposed solution leverages on IdentityServer4 SSO.

![Hugo B2C architecture](/markdown-guidance/hugo-diagram.png)

# Main website (main-site)
The **main-site** folder contains the .NET Core solution for the main website. It relies on the OpenId Connect authentication added to the AuthenticationBuilder using the default scheme to authenticate the users and it is handled directly in the Startup.cs class.
OpenID Connect is an identity layer on top of the OAuth 2.0 protocol. It allows clients to request and receive information about authenticated sessions and end-users.

# Documentation website (documentation-host)
The **documentation-host** folder contains the .NET Core solution for the documentation website host. The wwwroot of the solution hosts the static website generated with [Hugo](https://gohugo.io) from the **documentation** folder.
It relies on the OpenId Connect authentication added to the AuthenticationBuilder using the default scheme to authenticate the users and it is handled directly in the Startup.cs class.
OpenID Connect is an identity layer on top of the OAuth 2.0 protocol. It allows clients to request and receive information about authenticated sessions and end-users.

To handle the redirect and authorization over static files the UseStaticFiles method was used and extended inside the Startup.cs class.
```csharp
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
                // sets the first_name cookie, retrieving it from the user claims 
                // can be used to show the authenticated user in the documentation static pages via js/html
                if (ctx.Context.Request.Cookies["first_name"] == null)
                {
                    ctx.Context.Response.Cookies.Append(
                        "first_name",
                        ctx.Context.User.Claims.FirstOrDefault(f => f.Type == "firstName")?.Value ?? ctx.Context.User.Identity.Name ?? "",
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
```
The static files (js and css) needed to make the host website work were moved in a dedicated folder outside of the wwwroot, again the UseStaticFiles method was used to accomplish this.
```csharp
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
    Path.Combine(Directory.GetCurrentDirectory(), "MyStaticFiles")),
    RequestPath = "/StaticFiles"
});
```

# Documentation static site (documentation)
The **documentation** folder hosts content and assets needed by Hugo to generate the static website, starting from markdown files residing in the **documentation/content** folder.
For editing instructions please refer to the [Hugo basic usage](https://gohugo.io/getting-started/usage) guide.
Please refer to [Hugo instructions](./hugo-readme.md)

# IdentityServer4 (identity-server-4-hugo)
The **identity-server-4-hugo** folder hosts Identity Server which is an open source OpenID Connect and OAuth 2.0 framework for ASP.NET Core.
It incorporates all the protocol implementations and extensibility points needed to integrate token-based authentication, single-sign-on and API access control in your applications.
For our POC we used in-memory static users and clients.
