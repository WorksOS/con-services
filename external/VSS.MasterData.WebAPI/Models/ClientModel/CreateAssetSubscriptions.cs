using System.Collections.Generic;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class CreateAssetSubscriptions
	{
		public List<CreateAssetSubscriptionEvent> CreateAssetSubscriptionEvents { get; set; }
	}
}