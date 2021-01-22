using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace AspNetCore.Authentication.SK.IdCard.Areas.Identity.Pages.Account
{
    public class IdCardAuthenticationModel : PageModel
    {
        private readonly IOptionsMonitor<IdCardOptions> _optionsMonitor;

        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private IdCardOptions _options;

        public string MainSite => _options.MainSite;

        public string AuthenticationSite => _options.AuthenticationSite;

        public IdCardAuthenticationModel(IOptionsMonitor<IdCardOptions> options, IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _optionsMonitor = options;
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        public override async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            var authenticationSchemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
            var authenticationScheme = authenticationSchemes.Single(scheme => scheme.HandlerType == typeof(IdCardHandler));
            _options = _optionsMonitor.Get(authenticationScheme.Name);

            await base.OnPageHandlerSelectionAsync(context);
        }

        public void OnGet()
        {
        }
    }
}
