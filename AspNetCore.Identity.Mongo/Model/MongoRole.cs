using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace AspNetCore.Identity.Mongo.Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Database DTO object.")]
    public class MongoRole : IdentityRole<ObjectId>
    {
        public MongoRole()
        {
        }

        public MongoRole(string name)
            : base(name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            Name = name;
            NormalizedName = name.ToUpperInvariant();
            Claims = new List<IdentityRoleClaim<string>>();
        }

        public override string ToString() => Name;

        public List<IdentityRoleClaim<string>> Claims { get; set; }
    }
}