using AutoMapper;
using VSS.Messaging.BaseDataConsumer.Destination.Objects;
using VSS.Messaging.Destination.Objects.Interfaces;
using VSS.Messaging.Source.Objects.Classes.MasterData.Make;
using VSS.Messaging.Source.Objects.Classes.MasterData.ProductFamily;
using VSS.Messaging.Source.Objects.Classes.MasterData.SalesModel;

namespace VSS.Messaging.BaseDataConsumer
{
	public class AutoMapperProfile : Profile
	{
		/// <summary>
		/// Map Source to Destination
		/// </summary>
		public AutoMapperProfile()
		{

			CreateMap<SalesModelEvent, IDbTable>().ConvertUsing<CustomTypeMapperForSalesModel>();

			CreateMap<CreateSalesModel, DbSalesModel>()
				.ForMember(dest => dest.ProductFamilyUID, options => options.MapFrom(src => src.ProductFamilyUID))
				.ForMember(dest => dest.ModelCode, options => options.MapFrom(src => src.SalesModelCode))
				.ForMember(dest => dest.Description, options => options.MapFrom(src => src.SalesModelDescription))
				.ForMember(dest => dest.SalesModelUID, options => options.MapFrom(src => src.SalesModelUID))
				.ForMember(dest => dest.SerialNumberPrefix, options => options.MapFrom(src => src.SerialNumberPrefix))
				.ForMember(dest => dest.StartRange, options => options.MapFrom(src => src.StartRange))
				.ForMember(dest => dest.EndRange, options => options.MapFrom(src => src.EndRange))
				.ForMember(dest => dest.IconUID, options => options.MapFrom(src => src.IconUID))
				.ForMember(dest => dest.ReceivedUTC, options => options.MapFrom(src => src.ReceivedUtc))
				.ForMember(dest => dest.IsDelete, options => options.MapFrom(src => false))
				.ForAllOtherMembers(opt => opt.Ignore());

			CreateMap<UpdateSalesModel, DbSalesModel>()
				.ForMember(dest => dest.ProductFamilyUID, options => options.MapFrom(src => src.ProductFamilyUID))
				.ForMember(dest => dest.ModelCode, options => options.MapFrom(src => src.SalesModelCode))
				.ForMember(dest => dest.Description, options => options.MapFrom(src => src.SalesModelDescription))
				.ForMember(dest => dest.SalesModelUID, options => options.MapFrom(src => src.SalesModelUID))
				.ForMember(dest => dest.SerialNumberPrefix, options => options.MapFrom(src => src.SerialNumberPrefix))
				.ForMember(dest => dest.StartRange, options => options.MapFrom(src => src.StartRange))
				.ForMember(dest => dest.EndRange, options => options.MapFrom(src => src.EndRange))
				.ForMember(dest => dest.IconUID, options => options.MapFrom(src => src.IconUID))
				.ForMember(dest => dest.ReceivedUTC, options => options.MapFrom(src => src.ReceivedUtc))
				.ForMember(dest => dest.IsDelete, options => options.MapFrom(src => false))
				.ForAllOtherMembers(opt => opt.Ignore());

			CreateMap<DeleteSalesModel, DbSalesModel>()
				.ForMember(dest => dest.SalesModelUID, options => options.MapFrom(src => src.SalesModelUID))
				.ForMember(dest => dest.ReceivedUTC, options => options.MapFrom(src => src.ReceivedUtc))
				.ForMember(dest => dest.IsDelete, options => options.MapFrom(src => true))
				.ForAllOtherMembers(opt => opt.Ignore());

			CreateMap<MakeEvent, IDbTable>().ConvertUsing<CustomTypeMapperForMake>();

			CreateMap<ProductFamilyEvent, IDbTable>().ConvertUsing<CustomTypeMapperForProductFamily>();

		}
	}
}