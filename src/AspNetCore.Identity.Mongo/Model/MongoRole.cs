using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;

namespace AspNetCore.Identity.Mongo.Model
{
    public class MongoRole : MongoRole<ObjectId>
    {
        public MongoRole() : base() { }

        public MongoRole(string name) : base(name) { }
    }

    public class MongoRole<TKey> : IdentityRole<TKey> where TKey : IEquatable<TKey>
    {
        public MongoRole()
        {
            Claims = new List<IdentityRoleClaim<string>>();
        }

        public MongoRole(string name) : this()
        {
            Name = name;
            NormalizedName = name.ToUpperInvariant();
        }

        public override string ToString()
        {
            return Name;
        }

        public List<IdentityRoleClaim<string>> Claims { get; set; }
    }
}