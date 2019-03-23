using Microsoft.AspNetCore.Mvc;
using SampleSite.Identity;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using SampleSite.Mailing;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Collections;
using AspNetCore.Identity.Mongo.Model;
using SampleSite.Exceptions;
using System;

namespace SampleSite.Controllers
{
    public class HomeController : UserController
    {
        public HomeController(
  UserManager<MaddalenaUser> userManager,
  SignInManager<MaddalenaUser> signInManager,
  RoleManager<MongoRole> roleManager,

  IIdentityUserCollection<MaddalenaUser> userCollection,

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
            foreach (var delUser in await UserCollection.GetAllAsync())
            {
                await UserCollection.DeleteAsync(delUser);
            }

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

            return Content("EVERYTHING IS FINE");
        }

        private async Task TestConfirmEmail()
        {
            await ConfirmEmail(EmailSender.UserId, EmailSender.Token);

            var user = await UserCollection.FindByIdAsync(EmailSender.UserId);

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
    }
}
