using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Mongolino;
using Mongolino.Attributes;

namespace AspNetCore.Identity.Mongo
{
    public class MongoIdentityRole : ICollectionItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

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
    }
}