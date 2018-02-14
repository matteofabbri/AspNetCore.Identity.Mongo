using Mongolino;
using MongoDB.Bson.Serialization.Attributes;

namespace Maddalena.Identity
{
    public class ApplicationRole : DBObject<ApplicationRole>
    {
        public ApplicationRole()
        {
            AscendingIndex(x=>x.Name);
            AscendingIndex(x=>x.NormalizedName);
        }

        public ApplicationRole(string name)
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