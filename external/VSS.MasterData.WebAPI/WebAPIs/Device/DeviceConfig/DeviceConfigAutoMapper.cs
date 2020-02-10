using AutoMapper;
using ClientModel.DeviceConfig.Response.DeviceConfig.DeviceTypeGroupParameterAttribute;
using ClientModel.DeviceConfig.Response.DeviceConfig.Parameter;
using ClientModel.DeviceConfig.Response.DeviceConfig.ParameterGroup;
using ClientModel.DeviceConfig.Response.DeviceConfig.Ping;
using DbModel.Cache;
using DbModel.DeviceConfig;

namespace VSS.MasterData.WebAPI.Device.DeviceConfig
{
	/// <summary>
	/// 
	/// </summary>
	public class DeviceConfigAutoMapper : Profile
	{
		/// <summary>
		/// 
		/// </summary>
		public DeviceConfigAutoMapper()
		{
			CreateMap<DeviceParamGroupDto, ParameterGroupDetails>();
			CreateMap<DeviceParamDto, ParameterDetails>();
			CreateMap<DeviceTypeGroupParamAttrDto, DeviceTypeGroupParameterAttributeDetails>();
			CreateMap<DeviceTypeParameterAttributeDto, DeviceTypeGroupParameterAttributeDetails>();
			CreateMap<PingRequestStatus, DevicePingStatusResponse>();
		}
	}
}
