using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCore.Identity.Mongo.Model
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