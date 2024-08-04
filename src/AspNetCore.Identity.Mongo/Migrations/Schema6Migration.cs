using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo.Migrations
{
    internal class Schema6Migration : BaseMigration
    {
        public override int Version { get; } = 6;

        protected override void DoApply<TUser, TRole, TKey>(
            IMongoCollection<TUser> usersCollection,
            IMongoCollection<TRole> rolesCollection)
        {
            usersCollection.UpdateMany(x => true,
                Builders<TUser>.Update.Unset(x => x.AuthenticatorKey)
                    .Unset(x => x.RecoveryCodes));
        }
    }
}