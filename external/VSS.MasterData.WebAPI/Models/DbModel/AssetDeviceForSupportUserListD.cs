using System.Collections.Generic;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class AssetDeviceForSupportUserListD
	{
		public int TotalNumberOfPages { get; set; }
		public int PageNumber { get; set; }
		public List<AssetDeviceForSupportUserD> AssetDevices { get; set; }
	}
}