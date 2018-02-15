using MongoDB.Bson.Serialization.Attributes;
using Mongolino;
using Mongolino.Attributes;

namespace Microsoft.AspNetCore.Identity.Mongo
{
    public class MongoIdentityRole : DBObject<MongoIdentityRole>
    {
        public MongoIdentityRole()
        {
        }

        public MongoIdentityRole(string name)
        {
            Name = name;
            NormalizedName = name.ToUpperInvariant();
        }


        [AscendingIndex]
        public string Name { get; set; }

        [AscendingIndex]
        public string NormalizedName { get; set; }

        public override string ToString() => Name;

        [BsonIgnore]
        public long MembersCount => RoleMembership.Count(x => x.RoleId == Id);

    }
}