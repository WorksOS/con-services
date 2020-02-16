using AutoMapper;
using VSS.Messaging.BaseDataConsumer.Destination.Objects;
using VSS.Messaging.Destination.Objects.Interfaces;
using VSS.Messaging.Source.Objects.Classes.MasterData.Make;

namespace VSS.Messaging.BaseDataConsumer
{
	public class CustomTypeMapperForMake : ITypeConverter<MakeEvent, IDbTable>
	{
		public IDbTable Convert(MakeEvent source, IDbTable destination, ResolutionContext context)
		{
			if (source.CreateMakeEvent != null)
			{
				destination = new DbMake()
				{
					MakeCode = source.CreateMakeEvent.MakeCode,
					MakeDesc = source.CreateMakeEvent.MakeDesc,
					MakeUID = source.CreateMakeEvent.MakeUid,
					ReceivedUTC = source.CreateMakeEvent.ReceivedUtc
				};
			}
			else if (source.UpdateMakeEvent != null)
			{
				destination = new DbMake()
				{
					MakeCode = source.UpdateMakeEvent.MakeCode,
					MakeDesc = source.UpdateMakeEvent.MakeDesc,
					MakeUID = source.UpdateMakeEvent.MakeUid,
					ReceivedUTC = source.UpdateMakeEvent.ReceivedUtc
				};
			}
			return destination;
		}
	}
}