using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;

namespace DbMigrator.V2
{
    public class MongoRoleV2 : IdentityRole<ObjectId>
    {
        public MongoRoleV2()
        {
        }

        public MongoRoleV2(string name)
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