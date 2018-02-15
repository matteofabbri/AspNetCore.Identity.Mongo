using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using Mongolino;

namespace Microsoft.AspNetCore.Identity.Mongo
{
    public class MongoIdentityUser : DBObject<MongoIdentityUser>, IMongoIdentityUser
    {
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

        [BsonIgnoreIfNull]
        public virtual string PasswordHash { get; set; }

        public async Task<IEnumerable<string>> GetRoles()
        {
            var memerships = (await RoleMembership.WhereAsync(x => x.UserId == Id)).ToArray();
            var roles = memerships.Select(memership => MongoIdentityRole.First(x => x.Id == memership.RoleId))
                .Select(x => x.Name)
                .ToArray();

            return roles;
        }
    }
}