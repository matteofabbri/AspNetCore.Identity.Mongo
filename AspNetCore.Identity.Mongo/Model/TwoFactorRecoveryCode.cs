namespace AspNetCore.Identity.Mongo.Model
{
	public class TwoFactorRecoveryCode
	{
		public string Code { get; set; }

		public bool Redeemed { get; set; }
	}
}