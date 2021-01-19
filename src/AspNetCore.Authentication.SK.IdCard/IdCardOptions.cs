using Microsoft.AspNetCore.Authentication;

namespace AspNetCore.Authentication.SK.IdCard
{
    public class IdCardOptions : AuthenticationSchemeOptions
    {
        public string MainSite { get; set; }

        public string AuthenticationSite { get; set; }
    }
}