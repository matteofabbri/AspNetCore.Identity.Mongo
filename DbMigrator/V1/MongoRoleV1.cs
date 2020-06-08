// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// MongoRole
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