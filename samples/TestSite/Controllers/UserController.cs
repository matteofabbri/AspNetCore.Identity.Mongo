using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using SampleSite.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using TestSite.Models;

namespace SampleSite.Controllers
{
    //[Authorize(Roles = "user")]
    public class UserController : Controller
    {
        private readonly UserManager<TestSiteUser> _userManager;
        private readonly RoleManager<MongoRole> _roleManager;
        readonly IMongoCollection<TestSiteUser> _userUserCollection;

        public UserController(
            UserManager<TestSiteUser> userManager,
            RoleManager<MongoRole> roleManager,
            IMongoCollection<TestSiteUser> userCollection)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userUserCollection = userCollection;
        }

        public async Task<ActionResult> Index()
        {
            return View(_userManager.Users);
        }


        public async Task<ActionResult> AddToRole(string roleName, string userName)
        {
            var u = await _userManager.FindByNameAsync(userName);

            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new MongoRole(roleName));

            if (u == null) return NotFound();

            await _userManager.AddToRoleAsync(u, roleName);
            await _userManager.AddClaimAsync(u, new Claim(ClaimTypes.Role, roleName));

            return Redirect($"/user/edit/{userName}");
        }

        public async Task<ActionResult> CheckInRole(string roleName, string userName)
        {
            var u = await _userManager.FindByNameAsync(userName);

            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new MongoRole(roleName));

            if (u == null) return NotFound();

            var res = await _userManager.IsInRoleAsync(u, roleName);

            return Content(res.ToString());
        }

        public async Task<ActionResult> Edit(string id)
        {
            var user = await _userManager.FindByNameAsync(id);

            if (user == null) return NotFound();

            var model = new TestSiteUserViewModel
            {
                Id = user.Id.ToString(),
                AccessFailedCount = user.AccessFailedCount,
                ConcurrencyStamp = user.ConcurrencyStamp,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                NormalizedEmail = user.NormalizedEmail,
                NormalizedUserName = user.NormalizedUserName,
                PasswordHash = user.PasswordHash,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                SecurityStamp = user.SecurityStamp,
                TwoFactorEnabled = user.TwoFactorEnabled,
                UserName = user.UserName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TestSiteUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null) return NotFound();

            user.AccessFailedCount = model.AccessFailedCount;
            user.ConcurrencyStamp = model.ConcurrencyStamp;
            user.Email = model.Email;
            user.EmailConfirmed = model.EmailConfirmed;
            user.LockoutEnabled = model.LockoutEnabled;
            user.LockoutEnd = model.LockoutEnd;
            user.PhoneNumber = model.PhoneNumber;
            user.PhoneNumberConfirmed = model.PhoneNumberConfirmed;
            user.SecurityStamp = model.SecurityStamp;
            user.TwoFactorEnabled = model.TwoFactorEnabled;
            user.UserName = model.UserName;

            await _userManager.UpdateAsync(user);
            return Redirect("/user");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            var user = await _userUserCollection.DeleteOneAsync(x => x.Id == ObjectId.Parse(id));
            return Redirect("/user");
        }
    }
}