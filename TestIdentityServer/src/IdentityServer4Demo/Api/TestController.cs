using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace IdentityServer4Demo.Api
{
    [Route("/api/test")]
    [Authorize(AuthenticationSchemes = IdentityServerConstants.LocalApi.AuthenticationScheme)]
    public class TestController : ControllerBase
    {
        public IActionResult Get()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return new JsonResult(claims);
        }
    }
}