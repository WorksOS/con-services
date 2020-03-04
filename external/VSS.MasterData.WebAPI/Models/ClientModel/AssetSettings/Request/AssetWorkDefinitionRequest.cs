using ClientModel.Interfaces;
using System;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Request
{
	public class AssetSettingValidationRequestBase : IServiceRequest
	{
		public string DeviceType { get; set; }
		public List<string> AssetUIDs { get; set; }
		public string GroupName { get; set; }
		public Guid? CustomerUid { get; set; }
		public Guid? UserUid { get; set; }
	}
}
