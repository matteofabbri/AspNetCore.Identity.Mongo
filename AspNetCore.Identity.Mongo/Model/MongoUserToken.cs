using MongoDB.Bson.Serialization.Attributes;

namespace AspNetCore.Identity.Mongo.Model
{
    public class MongoUserToken
    {
        public MongoUserToken() { }

        public MongoUserToken(string loginProvider, string name, string value)
        {
            this.Id = CreateId(loginProvider, name);

            this.Value = value;
        }

        public MongoUserToken(string id, string value)
        {
            this.Id = id;

            this.Value = value;
        }

        public string Id { get; set; }

        public static string CreateId(string loginProvider, string name) => $"{loginProvider}+{name}";

        public string Value { get; set; }
    }
}
