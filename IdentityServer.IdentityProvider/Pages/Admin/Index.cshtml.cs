using IdentityServerAspNetIdentity.Pages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerAspNetIdentity.Pages.Admin;

[SecurityHeaders]
[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {

    }
}