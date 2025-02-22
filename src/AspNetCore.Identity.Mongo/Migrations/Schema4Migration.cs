using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo.Migrations;

internal class Schema4Migration : BaseMigration
{
    public override int Version { get; } = 4;

    protected override void DoApply<TUser, TRole, TKeyUser, TKeyRole>(
        IMongoCollection<TUser> usersCollection,
        IMongoCollection<TRole> rolesCollection)
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
}