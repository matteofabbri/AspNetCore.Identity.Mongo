using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;

namespace DbMigrator.V2
{
    public class MongoRole : IdentityRole<ObjectId>
    {
        public MongoRole()
        {
        }

        public MongoRole(string name)
        {
            Name = name;
            NormalizedName = name.ToUpperInvariant();
            Claims = new List<IdentityRoleClaim<string>>();
        }

        public override string ToString()
        {
            return Name;
        }

        public List<IdentityRoleClaim<string>> Claims { get; set; }
    }
}