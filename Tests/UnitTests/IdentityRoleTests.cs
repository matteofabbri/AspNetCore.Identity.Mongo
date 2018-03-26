using AspNetCore.Identity.Mongo;

namespace Tests
{
	using MongoDB.Bson;
	using NUnit.Framework;
    
	// todo low - validate all tests work
	[TestFixture]
	public class IdentityRoleTests : AssertionHelper
	{
		[Test]
		public void ToBsonDocument_IdAssigned_MapsToBsonObjectId()
		{
			var role = new MongoIdentityRole();

			var document = role.ToBsonDocument();

			Expect(document["_id"], Is.TypeOf<BsonObjectId>());
		}

		[Test]
		public void Create_WithoutRoleName_HasIdAssigned()
		{
			var role = new MongoIdentityRole();

			var parsed = role.Id.SafeParseObjectId();
			Expect(parsed, Is.Not.Null);
			Expect(parsed, Is.Not.EqualTo(ObjectId.Empty));
		}

		[Test]
		public void Create_WithRoleName_SetsName()
		{
			var name = "admin";

			var role = new MongoIdentityRole(name);

			Expect(role.Name, Is.EqualTo(name));
		}

		[Test]
		public void Create_WithRoleName_SetsId()
		{
			var role = new MongoIdentityRole("admin");

			var parsed = role.Id.SafeParseObjectId();
			Expect(parsed, Is.Not.Null);
			Expect(parsed, Is.Not.EqualTo(ObjectId.Empty));
		}
	}
}