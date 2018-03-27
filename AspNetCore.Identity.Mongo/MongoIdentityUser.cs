using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Mongolino;
using Mongolino.Attributes;

namespace AspNetCore.Identity.Mongo
{
    public class MongoIdentityUser : ICollectionItem
    {
        public MongoIdentityUser()
        {
            Roles = new List<string>();
            Claims = new List<IdentityUserClaim>();
            Logins = new List<IdentityUserLogin>();
            Tokens = new List<IdentityUserToken>();
            RecoveryCodes = new List<TwoFactorRecoveryCode>();
        }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [AscendingIndex]
        public virtual string UserName { get; set; }

        [AscendingIndex]
        public virtual string NormalizedUserName { get; set; }

        public virtual string SecurityStamp { get; set; }

        [AscendingIndex]
        public virtual string Email { get; set; }

        public virtual string NormalizedEmail { get; set; }

        public virtual bool EmailConfirmed { get; set; }

        public string PhoneNumber { get; set; }

        public virtual bool PhoneNumberConfirmed { get; set; }

        public virtual bool TwoFactorEnabled { get; set; }

        public virtual DateTimeOffset? LockoutEndDateUtc { get; set; }

        public virtual bool LockoutEnabled { get; set; }

        public virtual int AccessFailedCount { get; set; }

        public string AuthenticatorKey { get; set; }

        [BsonIgnoreIfNull]
        public string PasswordHash { get; set; }

        [BsonIgnoreIfNull]
        public List<string> Roles { get; set; }

        [BsonIgnoreIfNull]
        public List<IdentityUserClaim> Claims { get; set; }

        [BsonIgnoreIfNull]
        public List<IdentityUserLogin> Logins { get; set; }

        [BsonIgnoreIfNull]
        public List<IdentityUserToken> Tokens { get; set; }

        [BsonIgnoreIfNull]
        public List<TwoFactorRecoveryCode> RecoveryCodes { get; set; }
    }
}