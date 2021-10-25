using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Mongo;
using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo.Migrations
{
    internal static class Migrator
    {
        //Starting from 4 in case we want to implement migrations for previous versions
        public static int CurrentVersion = 6;

        public static void Apply<TUser, TRole, TKey>(IMongoCollection<MigrationHistory> migrationCollection, IMongoCollection<TUser> usersCollection, IMongoCollection<TRole> rolesCollection)
            where TKey : IEquatable<TKey>
            where TUser : MigrationMongoUser<TKey>
            where TRole : MongoRole<TKey>
        {
            var history = migrationCollection.Find(_ => true).ToList();

            if (history.Count > 0)
            {
                var lastHistory = history.OrderBy(x => x.DatabaseVersion).Last();

                if (lastHistory.DatabaseVersion == CurrentVersion)
                {
                    return;
                }

                // 4 -> 5
                if (lastHistory.DatabaseVersion == 4)
                {
                    var users = usersCollection.Find(x => !string.IsNullOrEmpty(x.AuthenticatorKey)).ToList();
                    foreach (var user in users)
                    {
                        var tokens = user.Tokens;
                        tokens.Add(new Microsoft.AspNetCore.Identity.IdentityUserToken<string>()
                        {
                            UserId = user.Id.ToString(),
                            Value = user.AuthenticatorKey,
                            LoginProvider = "[AspNetUserStore]",
                            Name = "AuthenticatorKey"
                        });
                        usersCollection.UpdateOne(x => x.Id.Equals(user.Id),
                            Builders<TUser>.Update.Set(x => x.Tokens, tokens)
                            .Set(x => x.AuthenticatorKey, null));
                    }
                }

                // 5 -> 6
                if (lastHistory.DatabaseVersion == 5)
                {
                    usersCollection.UpdateMany(x => true,
                        Builders<TUser>.Update.Unset(x => x.AuthenticatorKey)
                        .Unset(x => x.RecoveryCodes));
                }
            }

            migrationCollection.InsertOne(new MigrationHistory
            {
                InstalledOn = DateTime.Now,
                DatabaseVersion = CurrentVersion
            });
        }
    }
}
