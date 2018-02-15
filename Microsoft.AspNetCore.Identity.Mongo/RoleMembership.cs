using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Mongolino;

namespace Microsoft.AspNetCore.Identity.Mongo
{
    public class RoleMembership : DBObject<RoleMembership>
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string RoleId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
    }
}
