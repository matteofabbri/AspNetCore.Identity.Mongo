using AspNetCore.Identity.Mongo;

namespace Tests
{
    using MongoDB.Bson;
	using NUnit.Framework;

	// todo low - validate all tests work
	[TestFixture]
	public class IdentityUserTests : AssertionHelper
	{
		[Test]
		public void ToBsonDocument_IdAssigned_MapsToBsonObjectId()
		{
			var user = new MongoIdentityUser();

			var document = user.ToBsonDocument();

			Expect(document["_id"], Is.TypeOf<BsonObjectId>());
		}

		[Test]
		public void Create_NewIdentityUser_HasIdAssigned()
		{
			var user = new MongoIdentityUser();

			var parsed = user.Id.SafeParseObjectId();
			Expect(parsed, Is.Not.Null);
			Expect(parsed, Is.Not.EqualTo(ObjectId.Empty));
		}

		[Test]
		public void Create_NoPassword_DoesNotSerializePasswordField()
		{
			// if a particular consuming application doesn't intend to use passwords, there's no reason to store a null entry except for padding concerns, if that is the case then the consumer may want to create a custom class map to serialize as desired.

			var user = new MongoIdentityUser();

			var document = user.ToBsonDocument();

			Expect(document.Contains("PasswordHash"), Is.False);
		}

		[Test]
		public void Create_NullLists_DoesNotSerializeNullLists()
		{
			// serialized nulls can cause havoc in deserialization, overwriting the constructor's initial empty list 
			var user = new MongoIdentityUser();
			user.Roles = null;
			user.Tokens = null;
			user.Logins = null;
			user.Claims = null;

			var document = user.ToBsonDocument();

			Expect(document.Contains("Roles"), Is.False);
			Expect(document.Contains("Tokens"), Is.False);
			Expect(document.Contains("Logins"), Is.False);
			Expect(document.Contains("Claims"), Is.False);
		}

		[Test]
		public void Create_NewIdentityUser_ListsNotNull()
		{
			var user = new MongoIdentityUser();

			Expect(user.Logins, Is.Empty);
			Expect(user.Tokens, Is.Empty);
			Expect(user.Roles, Is.Empty);
			Expect(user.Claims, Is.Empty);
		}
	}
}