using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleSite.Blog
{
    public interface IBlogService
    {
        Task<IEnumerable<BlogPost>> GetPosts(int count, int skip = 0);

        Task<IEnumerable<BlogPost>> GetPostsByCategory(string category);

        Task<BlogPost> GetPostBySlug(string slug);

        Task<BlogPost> GetPostById(string id);

        Task<IEnumerable<string>> GetCategories();

        Task SavePost(BlogPost post);

        Task DeletePost(BlogPost post);

        Task<IEnumerable<BlogPost>> GetByLanguage(BlogPostLanguage lang, int n, int skip=0);
    }
}
