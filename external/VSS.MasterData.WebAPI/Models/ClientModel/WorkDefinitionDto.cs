using System;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class WorkDefinitionDto
	{
		public string AssetUID { get; set; }
		public string AssetUIDString => new Guid(AssetUID).ToString("N");
		public string WorkDefinitionType { get; set; }
		public long WorkDefinitionTypeID { get; set; }
		public int? SwitchNumber { get; set; }
		public bool? SwitchWorkStartState { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime InsertUTC { get; set; }
		public DateTime UpdateUTC { get; set; }
	}
}
