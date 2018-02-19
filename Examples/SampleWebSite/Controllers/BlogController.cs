using System;
using System.Linq;
using System.Threading.Tasks;
using Example.DefaultUser.Modules.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Mvc;

namespace Example.DefaultUser.Controllers
{
    [Authorize(Roles = "admin")]
    public class BlogController : Controller
    {
        static BlogController()
        {
            BlogArticle.DescendingIndex(x => x.Link);
            BlogArticle.DescendingIndex(x => x.DateTime);
        }

        // GET: Blog
        public ActionResult Index()
        {
            return View(BlogArticle.Queryable().OrderByDescending(x => x.DateTime).Take(20));
        }

        // GET: Blog/Create
        public ActionResult Create()
        {
            return View();
        }

        // GET: Blog/Details/5
        [AllowAnonymous]
        public async Task<ActionResult> Read(string id)
        {
            var article = await BlogArticle.FirstOrDefaultAsync(x => x.Link == id);
            if (article == null) return NotFound();

            //await BlogArticle.IncreaseAsync(article, x => x.Views, 1);

            return View(article);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Category(string id)
        {
            var article = await BlogArticle.WhereAsync(x => x.Category == id);
            if (article == null) return NotFound();

            ViewData["Title"] = id;

            return View(article);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Search(string q)
        {
            var articleList = await BlogArticle.FullTextSearchAsync(q);
            if (articleList == null) return NotFound();

            return View(articleList);
        }

        // POST: Blog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(BlogArticle article)
        {
            try
            {
                article.Id = string.Empty;
                article.DateTime = DateTime.Now;
                article.Author = await MongoIdentityUser.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);

                await BlogArticle.CreateAsync(article);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Blog/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            var article = await BlogArticle.FirstOrDefaultAsync(x => x.Link == id);
            if (article == null) return NotFound();

            await BlogArticle.UpdateAsync(article, x => x.Views, article.Views + 1);

            return View(article);
        }


        // POST: Blog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(BlogArticle article)
        {
            try
            {
                await BlogArticle.ReplaceAsync(article);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        // GET: Blog/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            var article = await BlogArticle.FirstOrDefaultAsync(x => x.Link == id);
            if (article == null) return NotFound();

            return View(article);
        }

        // POST: Blog/Delete/5
        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePost(string id)
        {
            try
            {
                await BlogArticle.DeleteAsync(x => x.Link == id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}