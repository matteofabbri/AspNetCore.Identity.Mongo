using AspNetCore.Identity.Mongo.Model;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace AspNetCore.Identity.Mongo
{
	internal static class Mapping
	{
		public static void Setup()
		{
			BsonClassMap.RegisterClassMap<MongoUser>(cm =>
			{
				cm.AutoMap();
				cm.MapIdMember(c => c.Id).SetIdGenerator(GuidGenerator.Instance);
				cm.MapField(c => c.PasswordHash).SetIgnoreIfNull(true);
				cm.MapField(c => c.Roles).SetIgnoreIfNull(true);
				cm.MapField(c => c.Claims).SetIgnoreIfNull(true);
				cm.MapField(c => c.Logins).SetIgnoreIfNull(true);
				cm.MapField(c => c.Tokens).SetIgnoreIfNull(true);
				cm.MapField(c => c.RecoveryCodes).SetIgnoreIfNull(true);
			});

			BsonClassMap.RegisterClassMap<MongoRole>(cm =>
			{
				cm.AutoMap();
				cm.MapIdMember(c => c.Id).SetIdGenerator(GuidGenerator.Instance);
			});
		}
	}
}