using System.Collections.Generic;
using System.Linq;
using VSS.Messaging.BaseDataConsumer.Destination.Objects;

namespace VSS.Messaging.BaseDataConsumer.Destination.Database
{
	public class SalesModelCommand : ICommand
	{
		private IDatabase database;

		public SalesModelCommand(IDatabase database)
		{
			this.database = database;
		}

		public void SaveDestinationRecordsToDB(List<object> list)
		{
			List<SalesModelDto> saveToDBRecords = new List<SalesModelDto>();
			List<SalesModelDto> deleteFromDBRecords = new List<SalesModelDto>();

			var distinctSalesModelUidsMessages = from records in list.Cast<DbSalesModel>()
												 group records by records.SalesModelUID into uniquesalesmodelgroup
												 select uniquesalesmodelgroup.OrderByDescending(x => x.ReceivedUTC).First();

			foreach (var salesModelUidMsg in distinctSalesModelUidsMessages)
			{
				if (!salesModelUidMsg.IsDelete && salesModelUidMsg.ProductFamilyUID != null)
				{
					saveToDBRecords.Add(new SalesModelDto()
					{
						SalesModelUID = salesModelUidMsg.SalesModelUID,
						ProductFamilyUID = salesModelUidMsg.ProductFamilyUID,
						IconUID = salesModelUidMsg.IconUID,
						Description = salesModelUidMsg.Description,
						EndRange = salesModelUidMsg.EndRange,
						StartRange = salesModelUidMsg.StartRange,
						SerialNumberPrefix = salesModelUidMsg.SerialNumberPrefix,
						ModelCode = salesModelUidMsg.ModelCode
					});
				}
				else if (salesModelUidMsg.IsDelete)
				{
					deleteFromDBRecords.Add(new SalesModelDto()
					{
						SalesModelUID = salesModelUidMsg.SalesModelUID
					});
				}
			}

			if (saveToDBRecords.Count != 0 && deleteFromDBRecords.Count != 0)
				database.ExecuteProcedure(saveToDBRecords.CopyToDataTable(), deleteFromDBRecords.CopyToDataTable(), "uspPub_SalesModel_Save");
			else if (saveToDBRecords.Count != 0)
				database.ExecuteProcedure(saveToDBRecords.CopyToDataTable(), null, "uspPub_SalesModel_Save");
			else if (deleteFromDBRecords.Count != 0)
				database.ExecuteProcedure(null, deleteFromDBRecords.CopyToDataTable(), "uspPub_SalesModel_Save");
		}
	}
}