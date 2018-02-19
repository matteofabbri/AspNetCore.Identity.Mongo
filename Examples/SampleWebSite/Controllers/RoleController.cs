using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Mvc;

namespace Example.DefaultUser.Controllers
{
    [Authorize(Roles = "admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<MongoIdentityRole> _roleManager;
        private readonly SignInManager<MongoIdentityUser> _signInManager;
        private readonly UserManager<MongoIdentityUser> _userManager;

        public RoleController(
            UserManager<MongoIdentityUser> userManager,
            SignInManager<MongoIdentityUser> signInManager,
            RoleManager<MongoIdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }


        // GET: Role
        public ActionResult Index()
        {
            return View(_roleManager.Roles);
        }

        // GET: Role/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var role = await _roleManager.FindByNameAsync(id);
            return View(role);
        }

        // GET: Role/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MongoIdentityRole role)
        {
            try
            {
                await _roleManager.CreateAsync(role);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Role/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Role/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: Role/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Role/Delete/5
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