using CommonModel.Enum;
using System;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace DbModel.AssetSettings
{
	public class ProductivityTargetsDBResponse
    {
        public string AssetID { get; set; }
        public Guid AssetUID { get { return Guid.Parse(AssetID);  } }
        public string AssetWeeklyConfigUID { get; set; }
        public double Sunday { get; set; }
        public double Monday { get; set; }
        public double Tuesday { get; set; }
        public double Wednesday { get; set; }
        public double Thursday { get; set; }
        public double Friday { get; set; }
        public double Saturday { get; set; }
        public string ConfigType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string AssetWeeklyConfigIdentifier { get; set; }
        public int ConfigValue { get { return (int)Enum.Parse(typeof(AssetTargetType), ConfigType);  } }
        public string fk_AssetUID { get { return AssetUID.ToStringWithoutHyphens().WrapWithUnhex(); } }
    }
}
