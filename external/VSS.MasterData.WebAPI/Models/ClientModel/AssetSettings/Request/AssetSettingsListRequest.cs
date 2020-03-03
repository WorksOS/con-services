using ClientModel.Interfaces;
using Newtonsoft.Json;
using System;

namespace ClientModel.AssetSettings.Request
{
	public class AssetSettingsListRequest : IServiceRequest
    {
        public string FilterName { get; set; }
        public string FilterValue { get; set; }
        public string DeviceType { get; set; }
        public string SortColumn { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        [JsonIgnore]
        public Guid? CustomerUid { get; set; }

        public Guid? SubAccountCustomerUid { get; set; }

        [JsonIgnore]
        public Guid? UserUid { get; set; }
    }
}
