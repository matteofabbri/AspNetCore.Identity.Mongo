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
using AspNetCore.Identity.Mongo.Mongo;
using MongoDB.Driver;

namespace SampleSite.Controllers
{
    public class HomeController : UserController
    {
        public HomeController(
  UserManager<TestSiteUser> userManager,
  SignInManager<TestSiteUser> signInManager,
  RoleManager<MongoRole> roleManager,

  IMongoCollection<TestSiteUser> userCollection,

  IEmailSender emailSender,
  ILogger<ManageController> logger,
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

            return Content("EVERYTHING IS FINE");
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
            if (!roleResult.Succeeded)
                throw new Exception("Add role fails");

            MongoRole role = await RoleManager.FindByNameAsync(TestData.RoleName);
            TestSiteUser user = await UserManager.FindByEmailAsync(TestData.Email);
            if (user == null)
            {
                user = new TestSiteUser { UserName = TestData.Username, Email = TestData.Email };
                IdentityResult result = await UserManager.CreateAsync(user, TestData.Password);
            }

            IdentityResult addRoleResult = await UserManager.AddToRoleAsync(user, TestData.RoleName);
            if (!addRoleResult.Succeeded)
                throw new Exception("Add role to user fails");

            TestSiteUser userWithRole = await UserManager.FindByEmailAsync(TestData.Email);
            if (!userWithRole.Roles.Any(r => r == role.Id))
                throw new Exception("Add role to user fails");

            IdentityResult removeRoleResult = await UserManager.RemoveFromRoleAsync(user, TestData.RoleName);
            if (!removeRoleResult.Succeeded)
                throw new Exception("Remove user from role fails");

            TestSiteUser userWithoutRole = await UserManager.FindByEmailAsync(TestData.Email);
            if (userWithoutRole.Roles.Any(r => r == role.Id))
                throw new Exception("Remove user from role fails");
        }
    }
}
