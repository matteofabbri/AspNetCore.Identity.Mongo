using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace AspNetCore.Identity.Mongo.Model
{
    public class MongoRole : IdentityRole
	{
	    //public ObjectId _id { get; set; }

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