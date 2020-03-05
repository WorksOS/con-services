using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class AssetOwner
	{
		#region Public Properties
		/// <summary>
		///  "Universal Customer"
		/// </summary>
		public string CustomerName { get; set; }

		/// <summary>
		/// "Account Name"
		/// </summary>
		public string AccountName { get; set; }

		/// <summary>
		/// "DCN"
		/// </summary>
		public string DealerAccountCode { get; set; }

		/// <summary>
		/// "Registered DealerUID"
		/// </summary>
		
		public Guid? DealerUID { get; set; }

		/// <summary>
		/// "Registered Dealer"
		/// </summary>
		
		public string DealerName { get; set; }

		/// <summary>
		/// "Dealer Code"
		/// </summary>
		public string NetworkDealerCode { get; set; }

		/// <summary>
		/// "UCID"
		/// </summary>
		public string NetworkCustomerCode { get; set; }

		/// <summary>
		/// CustomerUID"
		/// </summary>
		public Guid? CustomerUID { get; set; }

		/// <summary>
		/// AccountUID
		/// </summary>
		public Guid? AccountUID { get; set; }

		 

		#endregion Public Properties


	}
}
