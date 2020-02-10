using System;
using VSS.MasterData.WebAPI.Customer.KafkaModel;
using VSS.MasterData.WebAPI.DbModel;

namespace VSS.MasterData.WebAPI.Interfaces
{
	public interface ICustomerAssetService : ICustomerService
	{
		DbAssetCustomer GetAssetCustomer(Guid customerUid, Guid assetUid);
		DbAssetCustomer GetAssetCustomerByRelationType(Guid customerUid, Guid assetUid, int relationType);
		bool AssociateCustomerAsset(AssociateCustomerAssetEvent associateCustomerAsset);
		bool DissociateCustomerAsset(DissociateCustomerAssetEvent dissociateCustomerAsset);
	}
}
