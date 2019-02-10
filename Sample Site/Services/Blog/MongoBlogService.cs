using System.Collections.Generic;
using System.Threading.Tasks;
using SampleSite.Mongo;
using MongoDB.Driver;

namespace SampleSite.Blog
{
    public class MongoBlogService : IBlogService
    {
        private readonly IMongoCollection<BlogPost> _post;

        public MongoBlogService(string connectionString)
        {
            _post = MongoUtil.FromConnectionString<BlogPost>(connectionString, "blog");
        }

        public Task<IEnumerable<BlogPost>> GetPosts(int count, int skip = 0) => _post.TakeAsync(count, skip);

        public Task<IEnumerable<BlogPost>> GetPostsByCategory(string category) => _post.WhereAsync(x => x.Category == category);

        public Task<BlogPost> GetPostBySlug(string slug) => _post.FirstOrDefaultAsync(x => x.Slug == slug);

        public Task<BlogPost> GetPostById(string id) => _post.FirstOrDefaultAsync(x => x.Id == id);

        public Task<IEnumerable<string>> GetCategories() => Task.FromResult(new[] {"Code"} as IEnumerable<string>);

        public Task SavePost(BlogPost post)
        {
            if(string.IsNullOrWhiteSpace(post.Id))
            {
                return _post.InsertOneAsync(post);
            }
            else
            {
                return _post.ReplaceOneAsync(x => x.Id == post.Id, post);
            }
        }

        public Task DeletePost(BlogPost post) => _post.DeleteOneAsync(x => x.Id == post.Id);

        public async Task<IEnumerable<BlogPost>> GetByLanguage(BlogPostLanguage lang, int n, int skip = 0)
        {
            return await (await _post.FindAsync(x => x.Language == lang, new FindOptions<BlogPost, BlogPost>
            {
                Limit = n,
                Skip = skip
            })).ToListAsync();
        }
    }
}