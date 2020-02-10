using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.AssetSettings
{
    public class AssetSettingsListRequestDto
    {
        public string FilterName { get; set; }
        public string FilterValue { get; set; }
        public string DeviceType { get; set; }
        public int StatusInd { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
        public string SubAccountCustomerUid { get; set; }
        public string CustomerUid { get; set; }
        public string UserUid { get; set; }

		#region Defaults

		public int MovingThresholdsRadius { get; set; }
		public double MovingOrStoppedThreshold { get; set; }
		public int MovingThresholdsDuration { get; set; }

		#endregion 
	}
}
