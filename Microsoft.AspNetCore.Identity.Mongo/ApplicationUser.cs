using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maddalena.Mongo;
using MongoDB.Bson.Serialization.Attributes;

namespace Maddalena.Identity
{
    public class ApplicationUser : DBObject<ApplicationUser>
    {
        // PEOPLE DATA
        public string Name { get; set; }

        public string MiddleName { get; set; }

        public string FamilyName { get; set; }

        [BsonIgnore]
        public string DisplayName
        {
            get
            {
                var join = $"{Name} {MiddleName} {FamilyName}";

                return string.IsNullOrWhiteSpace(join) ? UserName : join;
            }
        }

        //IDENTITY DATA
        public virtual string UserName { get; set; }

        public virtual string NormalizedUserName { get; set; }

        /// <summary>
        ///     A random value that must change whenever a users credentials change
        ///     (password changed, login removed)
        /// </summary>
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

        public override string ToString() => DisplayName;

        public async Task<IEnumerable<string>> GetRoles()
        {
            var memerships = (await RoleMembership.WhereAsync(x => x.UserId == Id)).ToArray();
            var roles = memerships.Select(memership => ApplicationRole.First(x => x.Id == memership.RoleId))
                .Select(x => x.Name)
                .ToArray();

            return roles;
        }
    }
}