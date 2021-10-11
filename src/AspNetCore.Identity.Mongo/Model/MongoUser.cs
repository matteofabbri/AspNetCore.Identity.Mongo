using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AspNetCore.Identity.Mongo.Model
{
    public class MongoUser : MongoUser<ObjectId>
    {
        public MongoUser() : base() { }

        public MongoUser(string userName) : base(userName) { }
    }

    [BsonIgnoreExtraElements]
    public class MongoUser<TKey> : IdentityUser<TKey> where TKey : IEquatable<TKey>
    {
        public MongoUser()
        {
            Roles = new List<string>();
            Claims = new List<IdentityUserClaim<string>>();
            Logins = new List<IdentityUserLogin<string>>();
            Tokens = new List<IdentityUserToken<string>>();
        }

        public MongoUser(string userName) : this()
        {
            UserName = userName;
            NormalizedUserName = userName.ToUpperInvariant();
        }

        [BsonIgnore]
        [Obsolete("This property moved to Tokens and should not be used anymore! Will be removed in future versions.")]
        public string AuthenticatorKey { get; set; }

        public List<string> Roles { get; set; }

        public List<IdentityUserClaim<string>> Claims { get; set; }

        public List<IdentityUserLogin<string>> Logins { get; set; }

        public List<IdentityUserToken<string>> Tokens { get; set; }

        [BsonIgnore]
        [Obsolete("This property moved to Tokens and should not be used anymore! Will be removed in future versions.")]
        public List<TwoFactorRecoveryCode> RecoveryCodes { get; set; }
    }
}
