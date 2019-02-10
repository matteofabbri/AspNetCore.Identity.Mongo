using System.Linq;
using System.Threading.Tasks;
using SampleSite.Blog;
using SampleSite.GridFs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleSite.Controllers
{
    [Authorize(Roles = "blog")]
    public class BlogController : Controller
    {
        private readonly IBlogService _blog;
        private readonly IGridFileSystem _gridFs;

        public BlogController(IBlogService blog, IGridFileSystem gridFs)
        {
            _blog = blog;
            _gridFs = gridFs;
        }

        public async Task<IActionResult> Index() => View(await _blog.GetPosts(200));

        [Route("/blog/upload")]
        public async Task<ActionResult> BlogUpload()
        {
            var file = Request.Form.Files[0];
            var gridName = await _gridFs.Upload(file.FileName, file.OpenReadStream());

            await _gridFs.SetOwner(gridName,User.Identity.Name);
            await _gridFs.MakePublic(gridName);

            return Json(new { location = gridName });
        }

        [AllowAnonymous]
        [Route("/read/{slug?}")]
        public async Task<IActionResult> Read(string slug)
        {
            var post = await _blog.GetPostBySlug(slug);

            if (post != null)
            {
                return View(post);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("/blog/edit/{id?}")]
        public async Task<IActionResult> Edit(string id)
        {
            ViewData["AllCats"] = (await _blog.GetCategories()).ToList();

            if (string.IsNullOrEmpty(id))
            {
                return View(new BlogPost());
            }

            var post = await _blog.GetPostById(id);

            if (post != null)
            {
                return View(post);
            }

            return NotFound();
        }

        [Route("/blog/{slug?}")]
        [HttpPost, AutoValidateAntiforgeryToken]
        public async Task<IActionResult> UpdatePost(BlogPost post)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", post);
            }

            var existing = await _blog.GetPostById(post.Id) ?? post;

            existing.Category = post.Category;
            existing.Title = post.Title.Trim();
            existing.Slug = post.Slug;
            existing.IsPublished = post.IsPublished;
            existing.Content = post.Content.Trim();
            existing.Excerpt = post.Excerpt.Trim();

            await _blog.SavePost(existing);

            return Redirect($"/read/{post.Slug}");
        }

        [Route("/blog/delete")]
        [HttpPost, AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _blog.GetPostById(id);

            if (existing != null)
            {
                await _blog.DeletePost(existing);
                return Redirect("/");
            }

            return NotFound();
        }
    }
}
