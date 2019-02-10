using System;
using System.ComponentModel.DataAnnotations;
using SampleSite.Mongo;

namespace SampleSite.Blog
{
    public class BlogPost : MongoObject
    {
        public BlogPostLanguage Language { get; set; }

        [Required]
        public string Title { get; set; }

        public string Slug { get; set; }

        [Required]
        public string Excerpt { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime PubDate { get; set; } = DateTime.UtcNow;

        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        public bool IsPublished { get; set; } = true;

        public string Category { get; set; }
    }
}
