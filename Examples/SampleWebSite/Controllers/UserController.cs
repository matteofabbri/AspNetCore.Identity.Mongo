using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Mvc;

namespace Example.DefaultUser.Controllers
{
    //[Authorize(Roles = "admin")]
    public class UserController : Controller
    {
        private readonly RoleManager<MongoIdentityRole> _roleManager;
        private readonly UserManager<MongoIdentityUser> _userManager;

        public UserController(UserManager<MongoIdentityUser> userManager, RoleManager<MongoIdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: User
        public ActionResult Index()
        {
            return View(_userManager.Users);
        }

        public async Task<ActionResult> AddToRole(string role, string user)
        {
            var u = await _userManager.FindByNameAsync(user);

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new MongoIdentityRole(role));

            if (u == null) return NotFound();

            await _userManager.AddToRoleAsync(u, role);

            return RedirectToAction(nameof(Index));
        }

        // GET: User/Edit/5
        public ActionResult Edit(string id)
        {
            var user = _userManager.FindByNameAsync(id);

            if (user == null) return NotFound();

            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, MongoIdentityUser user)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: User/Delete/5
        public ActionResult Delete(string id)
        {
            var user = _userManager.FindByNameAsync(id);

            if (user == null) return NotFound();

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}