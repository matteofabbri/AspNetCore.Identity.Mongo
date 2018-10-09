using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AspNetCore.Identity.Mongo.Model
{
    public class MongoUser
	{
	    [BsonRepresentation(BsonType.ObjectId)]
	    public string Id { get; set; }

        public MongoUser()
		{
			Roles = new List<string>();
			Claims = new List<IdentityUserClaim>();
			Logins = new List<IdentityUserLogin>();
			Tokens = new List<IdentityUserToken>();
			RecoveryCodes = new List<TwoFactorRecoveryCode>();
		}

		public virtual string UserName { get; set; }
		public virtual string NormalizedUserName { get; set; }
		public virtual string SecurityStamp { get; set; }
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

		public string PasswordHash { get; set; }

		public List<string> Roles { get; set; }

		public List<IdentityUserClaim> Claims { get; set; }

		public List<IdentityUserLogin> Logins { get; set; }

		public List<IdentityUserToken> Tokens { get; set; }

		public List<TwoFactorRecoveryCode> RecoveryCodes { get; set; }
	}
}