using AutoMapper;
using VSS.Messaging.BaseDataConsumer.Destination.Objects;
using VSS.Messaging.Destination.Objects.Interfaces;
using VSS.Messaging.Source.Objects.Classes.MasterData.SalesModel;

namespace VSS.Messaging.BaseDataConsumer
{
	public class CustomTypeMapperForSalesModel : ITypeConverter<SalesModelEvent, IDbTable>
	{
		public IDbTable Convert(SalesModelEvent source, IDbTable destination, ResolutionContext context)
		{
			if (source.CreateSalesModelEvent != null)
			{
				destination = context.Mapper.Map<CreateSalesModel, DbSalesModel>(source.CreateSalesModelEvent);
			}
			else if (source.UpdateSalesModelEvent != null)
			{
				destination = context.Mapper.Map<UpdateSalesModel, DbSalesModel>(source.UpdateSalesModelEvent);
			}
			else if (source.DeleteSalesModelEvent != null)
			{
				destination = context.Mapper.Map<DeleteSalesModel, DbSalesModel>(source.DeleteSalesModelEvent);
			}
			return destination;
		}
	}
}