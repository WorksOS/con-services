using System;
using System.Collections.Generic;

namespace VSS.MasterData.WebAPI.ClientModel
{
	
	public class AssetSubscriptionModel
	{
		public Guid? AssetUID { get; set; }

		public string SubscriptionStatus { get; set; }

		public List<OwnerVisibility> OwnersVisibility { get; set; }
	}
}