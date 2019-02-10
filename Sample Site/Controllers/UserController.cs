using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Collections;
using AspNetCore.Identity.Mongo.Model;
using SampleSite.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SampleSite.Controllers
{
    //[Authorize(Roles = "user")]
    public class UserController : Controller
    {
        private readonly UserManager<MaddalenaUser> _userManager;
        private readonly RoleManager<MongoRole> _roleManager;
        readonly IIdentityUserCollection<MaddalenaUser> _userUserCollection;

        public UserController(
            UserManager<MaddalenaUser> userManager,
            RoleManager<MongoRole> roleManager,
            IIdentityUserCollection<MaddalenaUser> userCollection)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userUserCollection = userCollection;
        }

        public ActionResult Index(string id) => View(_userManager.Users);

        public async Task<ActionResult> AddToRole(string roleName, string userName)
        {
            var u = await _userManager.FindByNameAsync(userName);

            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new MongoRole(roleName));

            if (u == null) return NotFound();

            await _userManager.AddToRoleAsync(u, roleName);

            return Redirect($"/user/edit/{userName}");
        }

        public async Task<ActionResult> Edit(string id)
        {
            var user = await _userManager.FindByNameAsync(id);

            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MaddalenaUser user)
        {
            await _userUserCollection.UpdateAsync(user);
            return Redirect("/user");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            var user = await _userUserCollection.FindByIdAsync(id);
            await _userUserCollection.DeleteAsync(user);
            return Redirect("/user");
        }
    }
}