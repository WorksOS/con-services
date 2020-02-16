using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM.Interfaces
{
	public interface ICustomerService
	{
		bool Create(object customerDetails);
		bool Update(object customerDetails);
		bool Delete(Guid CustomerUID, DateTime ActionUTC);
		bool AssociateCustomerAsset(AssociateCustomerAssetEvent associateCustomerAssetEvent);
		bool AssociateCustomerUser(AssociateCustomerUserEvent associateCustomerUserEvent);
		bool DissociateCustomerAsset(DissociateCustomerAssetEvent dissociateCustomerAssetEvent);
		bool DissociateCustomerUser(DissociateCustomerUserEvent dissociateCustomerUserEvent);
	}
}
