using MongoDB.Bson.Serialization.Attributes;
using Mongolino;

namespace Microsoft.AspNetCore.Identity.Mongo
{
    public class MongoIdentityRole : DBObject<MongoIdentityRole>
    {
        public MongoIdentityRole()
        {
            AscendingIndex(x=>x.Name);
            AscendingIndex(x=>x.NormalizedName);
        }

        public MongoIdentityRole(string name)
        {
            Name = name;
            NormalizedName = name.ToUpperInvariant();
        }

        public string Name { get; set; }

        public string NormalizedName { get; set; }

        public override string ToString() => Name;

        [BsonIgnore]
        public long MembersCount => RoleMembership.Count(x => x.RoleId == Id);

    }
}