using System.Collections.Generic;
using System.Linq;
using VSS.Messaging.BaseDataConsumer.Destination.Objects;

namespace VSS.Messaging.BaseDataConsumer.Destination.Database
{
	public class ProductFamilyCommand : ICommand
	{
		private IDatabase database;

		public ProductFamilyCommand(IDatabase database)
		{
			this.database = database;
		}

		public void SaveDestinationRecordsToDB(List<object> list)
		{
			List<ProductFamilyDto> saveToDBRecords = new List<ProductFamilyDto>();

			var distinctProductFamilyUidsMessages = from records in list.Cast<DbProductFamily>()
													group records by records.ProductFamilyUID into uniqueProductFamilyGroup
													select uniqueProductFamilyGroup.OrderByDescending(x => x.ReceivedUTC).First();

			foreach (var productFamilyUidMsg in distinctProductFamilyUidsMessages)
			{
				saveToDBRecords.Add(new ProductFamilyDto()
				{
					Description = productFamilyUidMsg.Description,
					Name = productFamilyUidMsg.Name,
					ProductFamilyUID = productFamilyUidMsg.ProductFamilyUID
				});
			}

			database.ExecuteProcedure(saveToDBRecords.CopyToDataTable(), null, "uspPub_ProductFamily_Save");
		}
	}
}