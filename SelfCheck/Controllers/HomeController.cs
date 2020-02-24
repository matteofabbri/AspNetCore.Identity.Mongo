using Microsoft.AspNetCore.Mvc;
using SampleSite.Identity;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using SampleSite.Mailing;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using SampleSite.Exceptions;
using System;
using System.Linq;
using System.Security.Claims;
using AspNetCore.Identity.Mongo.Mongo;
using MongoDB.Driver;

namespace SampleSite.Controllers
{
    public class HomeController : UserController
    {
        public HomeController(UserManager<TestSiteUser> userManager,SignInManager<TestSiteUser> signInManager,RoleManager<MongoRole> roleManager,IMongoCollection<TestSiteUser> userCollection,IEmailSender emailSender,ILogger<ManageController> logger,
  UrlEncoder urlEncoder)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            EmailSender = emailSender;
            Logger = logger;
            UrlEncoder = urlEncoder;
            UserCollection = userCollection;
        }

        public async Task<IActionResult> Index()
        {
            await UserCollection.DeleteManyAsync(x => true);

            await Register(new Identity.AccountViewModels.RegisterViewModel
            {
                ConfirmPassword = TestData.Password,
                Password = TestData.Password,
                Email = TestData.Email,
                Username = TestData.Username
            });

            await Login(new Identity.AccountViewModels.LoginViewModel
            {
                Password = TestData.Password,
                RememberMe = true,
                Username = TestData.Username
            });

            await Logout();

            await TestConfirmEmail();

            await TestWrongPasswordLogin();

            await TestMissingUserLogin();

            await TestUserRoles();

            await TestAuthenticationTokens();

            await TestClaims();

            return Content("EVERYTHING IS FINE");
        }

        private async Task TestClaims()
        {
            TestSiteUser user = await UserManager.FindByEmailAsync(TestData.Email);
            var claim = new Claim(TestData.ClaimType, TestData.ClaimValue, TestData.ClaimIssuer); 

            if (!(await UserManager.AddClaimAsync(user,claim)).Succeeded) throw new ClaimFailsException("Failed add claim");

            if((await UserManager.GetClaimsAsync(user)).All(x => x.Value != TestData.ClaimValue)) throw new ClaimFailsException("Failed retrieve claim");

            await UserManager.RemoveClaimAsync(user, claim);

            if((await UserManager.GetClaimsAsync(user)).Any(x => x.Value == TestData.ClaimValue)) throw new ClaimFailsException("Failed removed claim");
        }

        private async Task TestAuthenticationTokens()
        {
            TestSiteUser user = await UserManager.FindByEmailAsync(TestData.Email);

            await UserManager.SetAuthenticationTokenAsync(user, TestData.LoginProvider, TestData.TokenName, TestData.TokenValue);
            var token = await UserManager.GetAuthenticationTokenAsync(user, TestData.LoginProvider, TestData.TokenName);

            if (token != TestData.TokenValue) throw new AutheticationTokenException("Authentication token fails");

            var res = await UserManager.RemoveAuthenticationTokenAsync(user, TestData.LoginProvider, TestData.TokenName);

            if (!res.Succeeded || await UserManager.GetAuthenticationTokenAsync(user, TestData.LoginProvider, TestData.TokenName) != null)
            {
                throw new AutheticationTokenException("Authentication token fails");
            }
        }

        private async Task TestConfirmEmail()
        {
            await ConfirmEmail(EmailSender.UserId, EmailSender.Token);

            var user = await UserCollection.FirstOrDefaultAsync(x => x.Id == EmailSender.UserId);
            if (!user.EmailConfirmed) throw new System.Exception("Confirm email fails");
        }

        private async Task TestWrongPasswordLogin()
        {
            var exceptionRaised = false;

            try
            {
                await Login(new Identity.AccountViewModels.LoginViewModel
                {
                    Password = "A VERY INVALID PASSWORD",
                    RememberMe = true,
                    Username = TestData.Username
                });
            }
            catch (InvalidLogin)
            {
                exceptionRaised = true;
            }

            if (!exceptionRaised) throw new Exception("Invalid login stop fails");
        }

        private async Task TestMissingUserLogin()
        {
            var exceptionRaised = false;

            try
            {
                await Login(new Identity.AccountViewModels.LoginViewModel
                {
                    Password = "A VERY INVALID PASSWORD",
                    RememberMe = true,
                    Username = "AN USER THAT DOES NOT EXISTS"
                });
            }
            catch (InvalidLogin)
            {
                exceptionRaised = true;
            }

            if (!exceptionRaised) throw new Exception("Invalid login stop fails");
        }

        private async Task TestUserRoles()
        {
            if (await RoleManager.RoleExistsAsync(TestData.RoleName))
                await RoleManager.DeleteAsync(await RoleManager.FindByNameAsync(TestData.RoleName));

            IdentityResult roleResult = await RoleManager.CreateAsync(new MongoRole(TestData.RoleName));
            if (!roleResult.Succeeded || !await RoleManager.RoleExistsAsync(TestData.RoleName))
                throw new Exception("Add role fails");


            MongoRole role = await RoleManager.FindByNameAsync(TestData.RoleName);
            TestSiteUser user = await UserManager.FindByEmailAsync(TestData.Email);
            if (user == null)
            {
                user = new TestSiteUser { UserName = TestData.Username, Email = TestData.Email };
                IdentityResult result = await UserManager.CreateAsync(user, TestData.Password);
            }

            IdentityResult addRoleResult = await UserManager.AddToRoleAsync(user, TestData.RoleName);
            if (!addRoleResult.Succeeded || ! await UserManager.IsInRoleAsync(user,TestData.RoleName))
                throw new Exception("Add role to user fails");


            IdentityResult removeRoleResult = await UserManager.RemoveFromRoleAsync(user, TestData.RoleName);
            if (!removeRoleResult.Succeeded|| await UserManager.IsInRoleAsync(user,TestData.RoleName))
                throw new Exception("Remove user from role fails");

            TestSiteUser userWithoutRole = await UserManager.FindByEmailAsync(TestData.Email);
            if (userWithoutRole.Roles.Any(r => r == role.Id))
                throw new Exception("Remove user from role fails");
        }
    }
}
