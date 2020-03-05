namespace VSS.MasterData.WebAPI.Utilities.Enums
{
	public enum DeleteType
	{
		Remove,
		RemoveCustomer,
		RemoveDealer
	}

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

	//public enum Action
	//{
	//	Create = 0,
	//	Update = 1,
	//	Delete = 2
	//}

	public enum AssetCustomerRelationShipType
	{
		Owner = 0,
		Customer = 1,
		Dealer = 2,
		Operations = 3,
		Corporate = 4,
		SharedOwner = 5
	}
}