using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo.Migrations;

internal class Schema5Migration : BaseMigration
{
    public override int Version { get; } = 5;

    protected override void DoApply<TUser, TRole, TKeyUser, TKeyRole>(
        IMongoCollection<TUser> usersCollection,
        IMongoCollection<TRole> rolesCollection)
    {
        usersCollection.UpdateMany(x => true,
            Builders<TUser>.Update.Unset(x => x.AuthenticatorKey)
                .Unset(x => x.RecoveryCodes));
    }
}