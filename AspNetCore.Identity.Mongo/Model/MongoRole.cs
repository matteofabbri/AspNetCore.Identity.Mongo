using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AspNetCore.Identity.Mongo.Model
{
	public class MongoRole
	{
	    [BsonRepresentation(BsonType.ObjectId)]
	    public string Id { get; set; }

        public MongoRole()
		{
		}

		public MongoRole(string name)
		{
			Name = name;
			NormalizedName = name.ToUpperInvariant();
		}

		public string Name { get; set; }
		public string NormalizedName { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}