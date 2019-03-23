using Microsoft.AspNetCore.Mvc;
using SampleSite.Identity;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using SampleSite.Mailing;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Collections;
using AspNetCore.Identity.Mongo.Model;

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
            foreach (var user in await UserCollection.GetAllAsync())
            {
                await UserCollection.DeleteAsync(user);
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

            return Content("EVERYTHING IS FINE");
        }
    }
}
