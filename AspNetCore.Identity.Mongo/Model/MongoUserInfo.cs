using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace AspNetCore.Identity.Mongo.Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Database DTO object.")]
    public class MongoUserInfo<TUser>
        where TUser: MongoUser
    {
        public ObjectId Id { get; set; }

        [BsonRequired]
        public TUser User { get; set; }

        [BsonIgnoreIfNull]
        public List<ObjectId> Roles { get; set; }

        [BsonIgnoreIfNull]
        public List<MongoClaim> Claims { get; set; }

        [BsonIgnoreIfNull]
        public List<UserLoginInfo> Logins { get; set; }

        [BsonIgnoreIfNull]
        public List<MongoUserToken> Tokens { get; set; }

        [BsonIgnoreIfNull]
        public List<string> RecoveryCodes { get; set; }

    }
}
