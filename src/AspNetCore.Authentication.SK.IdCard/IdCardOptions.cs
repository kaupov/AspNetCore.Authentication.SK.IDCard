using Microsoft.AspNetCore.Authentication;

namespace AspNetCore.Authentication.SK.IDCard
{
    public class IdCardOptions : AuthenticationSchemeOptions
    {
        public string MainSite { get; set; }

        public string AuthenticationSite { get; set; }
    }
}