using System;
using SampleSite.Mongo;

namespace SampleSite.GridFs
{
    public class GridFile : MongoObject
    {
        public string GridName { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public DateTimeOffset LastModified { get; set; }

        public Acl Acl { get; set; } = new Acl();
    }
}
