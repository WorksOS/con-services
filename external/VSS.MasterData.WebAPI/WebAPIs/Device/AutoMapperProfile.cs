using AutoMapper;
using VSS.MasterData.WebAPI.ClientModel.Device;
using VSS.MasterData.WebAPI.KafkaModel.Device;

namespace VSS.MasterData.WebAPI.Device
{
	public class AutoMapperProfile : Profile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AutoMapperProfile"/> class. 
		/// Map Source to Destination
		/// </summary>
		public AutoMapperProfile()
		{
			CreateMap<UpdateDevicePayload, UpdateDeviceEvent>();
			CreateMap<CreateDevicePayload, CreateDeviceEvent>();
		}
	}
}
