using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace AspNetCore.Identity.Mongo.Model;

public class MongoUser : MongoUser<ObjectId>
{
    public MongoUser() : base() { }

    public MongoUser(string userName) : base(userName) { }
}

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

    public List<string> Roles { get; set; }

    public List<IdentityUserClaim<string>> Claims { get; set; }

    public List<IdentityUserLogin<string>> Logins { get; set; }

    public List<IdentityUserToken<string>> Tokens { get; set; }
}