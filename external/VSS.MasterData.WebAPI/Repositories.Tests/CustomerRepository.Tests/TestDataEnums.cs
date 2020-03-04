namespace VSS.MasterData.WebAPI.CustomerRepository.Tests
{
	public class TestDataEnums
	{
		public enum RelationType
		{
			Owner = 0,
			Customer = 1,
			Dealer = 2,
			Operations = 3,
			Corporate = 4,
			SharedOwner = 5
		}

		public enum CustomerType
		{
			Customer = 0,
			Dealer = 1,
			Operations = 2,
			Corporate = 3,
			Account = 4
		}
	}
}
