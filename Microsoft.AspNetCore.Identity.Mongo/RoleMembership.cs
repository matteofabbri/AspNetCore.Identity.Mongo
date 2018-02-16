using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Mongolino;
using Mongolino.Attributes;

namespace Microsoft.AspNetCore.Identity.Mongo
{
    public class RoleMembership : DBObject<RoleMembership>
    {
        [AscendingIndex]
        [BsonRepresentation(BsonType.ObjectId)]
        public string RoleId { get; set; }

        [AscendingIndex]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
    }
}
