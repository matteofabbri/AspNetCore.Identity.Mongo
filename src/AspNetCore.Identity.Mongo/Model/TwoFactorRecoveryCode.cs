using System;

namespace AspNetCore.Identity.Mongo.Model
{
    internal class TwoFactorRecoveryCode
    {
        public string Code { get; set; }

        public bool Redeemed { get; set; }
    }
}