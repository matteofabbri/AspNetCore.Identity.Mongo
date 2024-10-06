using AspNetCore.Identity.Mongo.Model;
using MongoDB.Bson.Serialization.Attributes;

namespace TestSite.Services.Identity;

[BsonIgnoreExtraElements]
public class TestSiteUser : MongoUser
{
}