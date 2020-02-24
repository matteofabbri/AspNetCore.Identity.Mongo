using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using SampleSite.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using MongoDB.Driver;

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
        public async Task<ActionResult> Edit(TestSiteUser user)
        {
            var filter = Builders<TestSiteUser>.Filter.Eq(x => x.Id, user.Id);
            await UserCollection.ReplaceOneAsync(filter, user);
            return Redirect("/user");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            await UserCollection.DeleteOneAsync(x => x.Id == id);
            return Redirect("/user");
        }
    }
}