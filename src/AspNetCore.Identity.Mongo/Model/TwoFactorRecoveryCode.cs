using System;

namespace AspNetCore.Identity.Mongo.Model
{
    [Obsolete("This property moved to Tokens and should not be used anymore! Will be removed in future versions.")]
    public class TwoFactorRecoveryCode
    {
        public string Code { get; set; }

        public bool Redeemed { get; set; }
    }
}