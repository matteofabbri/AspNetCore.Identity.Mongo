using AspNetCore.Identity.Mongo.Model;
using MongoDB.Bson.Serialization.Attributes;

namespace SampleSite.Identity
{
    [BsonIgnoreExtraElements]
    public class TestSiteUser : MongoUser
    {
    }
}
