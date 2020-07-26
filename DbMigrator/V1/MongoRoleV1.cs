
using Microsoft.AspNetCore.Identity;

namespace DbMigrator.V1
{
    public class MongoRoleV1 : IdentityRole
    {
        public MongoRoleV1()
        {
        }

        public MongoRoleV1(string name)
            : this()
        {
            this.Name = name;
            this.NormalizedName = (name.ToUpperInvariant());
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}