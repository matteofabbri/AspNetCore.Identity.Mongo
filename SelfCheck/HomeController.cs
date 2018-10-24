using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SelfCheck.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<MongoUser> _userManager;
        private readonly SignInManager<MongoUser> _signInManager;
        private IIdentityUserCollection<MongoUser> _userCollection;

        private const string password = "PASSWORD";

        static MongoUser UserWithMail = new MongoUser
        {
            UserName = "User1",
            Email = "me@me.me",
        };

        static MongoUser UserWithNoMail = new MongoUser
        {
            UserName = "UserWithNoMail",
            Email = null,
        };


        public HomeController(UserManager<MongoUser> userManager, SignInManager<MongoUser> signInManager,
            IIdentityUserCollection<MongoUser> userCollection)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userCollection = userCollection;
        }

        public async Task<IActionResult> Index()
        {
            await CreateNoPassword();
            await Delete();
            await CreatePassword();
            await Delete();
            await DoubleCreateNoPassword();
            await DoubleCreatePassword();
            //FIXME
            //await AccessFailedAsync();
            await Claims();

            return Content("ANYTHING IS FINE");
        }

        async Task CreateNoPassword()
        {
            await _userManager.CreateAsync(UserWithMail);
            await _userManager.CreateAsync(UserWithNoMail);

            var users = (await _userCollection.GetAllAsync());
            if (users.Count() != 2) throw new Exception(nameof(CreateNoPassword));
        }

        async Task CreatePassword()
        {
            await _userManager.CreateAsync(UserWithMail, password);
            await _userManager.CreateAsync(UserWithNoMail, password);

            var users = (await _userCollection.GetAllAsync());
            if (users.Count() != 2) throw new Exception(nameof(CreateNoPassword));
        }

        async Task Delete()
        {
            await _userManager.DeleteAsync(UserWithMail);
            await _userManager.DeleteAsync(UserWithNoMail);

            var users = (await _userCollection.GetAllAsync());
            if (users.Count() != 2) throw new Exception(nameof(CreateNoPassword));
        }

        async Task DoubleCreateNoPassword()
        {
            await CreateNoPassword();
            await CreateNoPassword();

            if ((await _userCollection.GetAllAsync()).Count() != 2) throw new Exception(nameof(CreateNoPassword));
        }

        async Task DoubleCreatePassword()
        {
            await CreatePassword();
            await CreatePassword();

            if ((await _userCollection.GetAllAsync()).Count() != 2) throw new Exception(nameof(CreateNoPassword));
        }

        async Task AccessFailedAsync()
        {
            var actual = await _userManager.GetAccessFailedCountAsync(UserWithMail);

            if (actual != 0) throw new Exception(nameof(AccessFailedAsync));

            var r = await _userManager.AccessFailedAsync(UserWithMail);

            actual = await _userManager.GetAccessFailedCountAsync(UserWithMail);

            if (actual != 1) throw new Exception(nameof(AccessFailedAsync));
        }


        async Task Claims()
        {
            var claim = new Claim("FAKE CLAIM", "FAKE VALUE");

            if (!_userManager.SupportsUserClaim) throw new Exception(nameof(Claims));

            var c = await _userManager.GetClaimsAsync(UserWithMail);

            if (c.Count != 0) throw new Exception(nameof(Claims));

            await _userManager.AddClaimAsync(UserWithMail, claim);

            var claims = (await _userManager.GetClaimsAsync(UserWithMail));
            if (claims.Count != 1 || claims[0].Value != "FAKE VALUE") throw new Exception(nameof(Claims));

            var users = await _userManager.GetUsersForClaimAsync(claim);

            if (users.Count != 1) throw new Exception(nameof(Claims));

            await _userManager.RemoveClaimAsync(UserWithMail, claim);

            users = await _userManager.GetUsersForClaimAsync(claim);

            if (users.Count != 0) throw new Exception(nameof(Claims));

            await _userManager.AddClaimAsync(UserWithMail, claim);
            await _userManager.ReplaceClaimAsync(UserWithMail, claim,
                new Claim("REPLACEMENT CLAIM", "REPLACEMENT CLAIM"));

            claims = await _userManager.GetClaimsAsync(UserWithMail);
            if (claims.Count != 1 || claims[0].Value != "REPLACEMENT CLAIM") throw new Exception(nameof(Claims));

            await _userManager.RemoveClaimsAsync(UserWithMail, claims);
        }
    }
}
