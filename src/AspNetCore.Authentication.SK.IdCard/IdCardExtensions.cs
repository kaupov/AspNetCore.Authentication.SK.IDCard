using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rfc2253;

namespace AspNetCore.Authentication.SK.IDCard
{
    public static class IdCardExtensions
    {
        private const string IdCardAllowedOriginsPolicyName = "IdCardAllowedOrigins";

        public static AuthenticationBuilder AddIdCard(this AuthenticationBuilder builder) =>
            builder.AddIdCard(IdCardDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddIdCard(this AuthenticationBuilder builder,
            Action<IdCardOptions> configureOptions) =>
            builder.AddIdCard(IdCardDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddIdCard(this AuthenticationBuilder builder, string authenticationScheme,
            Action<IdCardOptions> configureOptions) =>
            builder.AddIdCard(authenticationScheme, IdCardDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddIdCard(this AuthenticationBuilder builder,
            string authenticationScheme, string displayName, Action<IdCardOptions> configureOptions)
        {
            builder.AddCertificate(options =>
            {
                options.Events = new CertificateAuthenticationEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // When certificate authentication fails redirect user back to main site
                        var idCardOptions = context.HttpContext.RequestServices.GetService<IOptionsMonitor<IdCardOptions>>();
                        var mainSiteUri = new Uri(idCardOptions.Get(authenticationScheme).MainSite);
                        var redirect = UriHelper.BuildAbsolute(mainSiteUri.Scheme,
                            new HostString(mainSiteUri.Host, mainSiteUri.Port), mainSiteUri.AbsolutePath,
                            "/Identity/Account/ExternalLogin",
                            QueryString.Create(new List<KeyValuePair<string, string>>
                            {
                                new KeyValuePair<string, string>("handler", "Callback"),
                                new KeyValuePair<string, string>("remoteError", "Failed to authenticate")
                            }));
                        context.Response.Redirect(redirect);
                        return Task.CompletedTask;
                    }
                };
            });

            return builder.AddScheme<IdCardOptions, IdCardHandler>(authenticationScheme, displayName,
                configureOptions);
        }

        public static IServiceCollection AddIdCardAllowedOrigins(this IServiceCollection services, string[] allowedOrigins)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(IdCardAllowedOriginsPolicyName, builder =>
                {
                    builder.WithOrigins(allowedOrigins).AllowCredentials();
                });
            });
            return services;
        }

        public static IApplicationBuilder UseIdCardAuthentication(this IApplicationBuilder builder) =>
            UseIdCardAuthentication(builder, IdCardDefaults.AuthenticationScheme);

        public static IApplicationBuilder UseIdCardAuthentication(this IApplicationBuilder builder, string authenticationScheme)
        {
            builder.UseCors(IdCardAllowedOriginsPolicyName);

            builder.UseEndpoints(routeBuilder =>
            {
                routeBuilder.Map(IdCardDefaults.AuthenticationRoute, async context =>
                {
                    var properties = new AuthenticationProperties();
                    properties.Items["LoginProvider"] = authenticationScheme;

                    var name = DistinguishedName.Create(context.Connection.ClientCertificate.Subject);

                    var identity = new ClaimsIdentity(new Claim[] { }, IdCardDefaults.AuthenticationScheme);

                    FindAndAddClaim(identity, name, ClaimTypes.NameIdentifier, "SERIALNUMBER");
                    FindAndAddClaim(identity, name, ClaimTypes.GivenName, "G");
                    FindAndAddClaim(identity, name, ClaimTypes.Surname, "SN");
                    FindAndAddClaim(identity, name, ClaimTypes.SerialNumber, "SERIALNUMBER");
                    FindAndAddClaim(identity, name, ClaimTypes.Country, "C");

                    await context.SignInAsync(IdentityConstants.ExternalScheme, new ClaimsPrincipal(identity), properties);
                }).RequireCors(IdCardAllowedOriginsPolicyName);
            });

            return builder;
        }

        private static void FindAndAddClaim(ClaimsIdentity identity, DistinguishedName name, string claimType,
            string nameType)
        {
            var distinguishedName = name.Rdns.FirstOrDefault(dn => dn.Type.Value == nameType);

            if (distinguishedName == null)
                return;

            var value = distinguishedName.Value.Value;

            identity.AddClaim(new Claim(claimType, value));
        }
    }
}