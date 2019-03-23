using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using SampleSite.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SampleSite.Controllers
{
    public class UserController : ManageController
    {
        public ActionResult ViewUser(string id) => View(UserManager.Users);

        public async Task<ActionResult> AddToRole(string roleName, string userName)
        {
            var u = await UserManager.FindByNameAsync(userName);

            if (!await RoleManager.RoleExistsAsync(roleName))
                await RoleManager.CreateAsync(new MongoRole(roleName));

            if (u == null) return NotFound();

            await UserManager.AddToRoleAsync(u, roleName);

            return Redirect($"/user/edit/{userName}");
        }

        public async Task<ActionResult> Edit(string id)
        {
            var user = await UserManager.FindByNameAsync(id);

            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MaddalenaUser user)
        {
            await UserCollection.UpdateAsync(user);
            return Redirect("/user");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            var user = await UserCollection.FindByIdAsync(id);
            await UserCollection.DeleteAsync(user);
            return Redirect("/user");
        }
    }
}