using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore.Authentication.SK.IdCard
{
    public class IdCardHandler : AuthenticationHandler<IdCardOptions>
    {
        public IdCardHandler(IOptionsMonitor<IdCardOptions> options, ILoggerFactory logger, UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            // Redirect user to authenticate site that requires client certificate
            var uri = new Uri(Options.AuthenticationSite);
            var redirect = UriHelper.BuildAbsolute(uri.Scheme, new HostString(uri.Host, uri.Port), uri.AbsolutePath,
                IdCardDefaults.AuthenticationRoute);
            Context.Response.Redirect(redirect);
            return Task.CompletedTask;
        }
    }
}