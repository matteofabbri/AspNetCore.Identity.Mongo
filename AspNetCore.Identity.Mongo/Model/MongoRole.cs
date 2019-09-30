using Microsoft.AspNetCore.Identity;

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
		}

		public override string ToString()
		{
			return Name;
		}
	}
}