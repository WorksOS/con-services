using AutoMapper;
using VSS.Messaging.BaseDataConsumer.Destination.Objects;
using VSS.Messaging.Destination.Objects.Interfaces;
using VSS.Messaging.Source.Objects.Classes.MasterData.ProductFamily;

namespace VSS.Messaging.BaseDataConsumer
{
	class CustomTypeMapperForProductFamily : ITypeConverter<ProductFamilyEvent, IDbTable>
	{
		public IDbTable Convert(ProductFamilyEvent source, IDbTable destination, ResolutionContext context)
		{
			if (source.CreateProductFamilyEvent != null)
			{
				destination = new DbProductFamily()
				{
					Name = source.CreateProductFamilyEvent.ProductFamilyName,
					Description = source.CreateProductFamilyEvent.ProductFamilyDesc,
					ProductFamilyUID = source.CreateProductFamilyEvent.ProductFamilyUID,
					ReceivedUTC = source.CreateProductFamilyEvent.ReceivedUtc
				};
			}
			else if (source.UpdateProductFamilyEvent != null)
			{
				destination = new DbProductFamily()
				{
					Name = source.UpdateProductFamilyEvent.ProductFamilyName,
					Description = source.UpdateProductFamilyEvent.ProductFamilyDesc,
					ProductFamilyUID = source.UpdateProductFamilyEvent.ProductFamilyUID,
					ReceivedUTC = source.UpdateProductFamilyEvent.ReceivedUtc
				};
			}
			return destination;
		}
	}
}