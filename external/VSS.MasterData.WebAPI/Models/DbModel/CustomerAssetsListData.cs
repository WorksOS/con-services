using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class CustomerAssetsListData
	{
		public int TotalRowsCount { get; set; }
		public int TotalNumberOfPages { get; set; }
		public int PageNumber { get; set; }
		public List<CustomerAsset> CustomerAssets { get; set; }

	}
}