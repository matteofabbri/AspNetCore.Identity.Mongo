using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using System;

namespace AspNetCore.Identity.Mongo.Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Database DTO object.")]
    public class MongoUser : IdentityUser<ObjectId>
    {
        public MongoUser()
        {
        }

        public MongoUser(string userName)
            : base(userName)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException(nameof(userName));
            
            UserName = userName;
            NormalizedUserName = userName.ToUpperInvariant();
        }

        public string AuthenticatorKey { get; set; }

        public override string ToString() => UserName;
    }
}