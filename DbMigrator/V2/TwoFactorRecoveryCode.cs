namespace DbMigrator.V2
{
	public class TwoFactorRecoveryCode
	{
		public string Code { get; set; }

		public bool Redeemed { get; set; }
	}
}