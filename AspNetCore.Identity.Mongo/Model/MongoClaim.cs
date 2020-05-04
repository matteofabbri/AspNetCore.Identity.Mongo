namespace AspNetCore.Identity.Mongo.Model
{
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    using System.Security.Claims;

    public sealed class MongoClaim
    {
        public MongoClaim() { }

        public MongoClaim(Claim claim)
        {
            if (claim == null) throw new ArgumentNullException(nameof(claim));

            this.Type = claim.Type;
            this.Value = claim.Value;
        }

        [BsonRequired]
        public string Type { get; set; }

        [BsonRequired]
        public string Value { get; set; }

        public override string ToString() => $"{Type}={Value}";
    }
}

