using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Hosted.VLCommon.Services.MDM.Models
{
	//MDM - Master Data Management (Next gen space)
	
	public class CreateCustomerEvent 
	{
		public string CustomerName { get; set; }
		
		public int CustomerType { get; set; }
		
		public string BSSID { get; set; }
		
		public string DealerNetwork { get; set; }
		
		public string NetworkDealerCode { get; set; }
		
		public string NetworkCustomerCode { get; set; }
		
		public string DealerAccountCode { get; set; }
		
		public Guid CustomerUID { get; set; }
		
		public DateTime ActionUTC { get; set; }
		
		public DateTime ReceivedUTC { get; set; }
	}
	public class UpdateCustomerEvent
	{
		public string CustomerName { get; set; }

		public string BSSID { get; set; }

		public string DealerNetwork { get; set; }

		public string NetworkDealerCode { get; set; }

		public string NetworkCustomerCode { get; set; }

		public string DealerAccountCode { get; set; }

		public Guid CustomerUID { get; set; }

		public DateTime ActionUTC { get; set; }

		public DateTime ReceivedUTC { get; set; }
	}


	public class AssociateCustomerAssetEvent
	{

		public Guid CustomerUID { get; set; }


		public Guid AssetUID { get; set; }


		public string RelationType { get; set; }


		public DateTime ActionUTC { get; set; }


		public DateTime ReceivedUTC { get; set; }
	}

	public class AssociateCustomerUserEvent
	{

		public Guid CustomerUID { get; set; }

		public Guid UserUID { get; set; }

		public DateTime ActionUTC { get; set; }

		public DateTime ReceivedUTC { get; set; }
	}

	public class DissociateCustomerAssetEvent
	{

		public Guid CustomerUID { get; set; }


		public Guid AssetUID { get; set; }


		public DateTime ActionUTC { get; set; }


		public DateTime ReceivedUTC { get; set; }
	}
	public class DissociateCustomerUserEvent
	{

		public Guid CustomerUID { get; set; }

		public Guid UserUID { get; set; }

		public DateTime ActionUTC { get; set; }

		public DateTime ReceivedUTC { get; set; }
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

}
