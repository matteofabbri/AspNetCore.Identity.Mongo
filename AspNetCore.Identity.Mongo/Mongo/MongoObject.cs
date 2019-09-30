using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Maddalena.Mongo
{
	public abstract class MongoObject
	{
	    [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
	}
}