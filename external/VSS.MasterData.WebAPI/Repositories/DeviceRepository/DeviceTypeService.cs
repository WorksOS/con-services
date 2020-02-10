using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Data.MySql;
using VSS.MasterData.WebAPI.DbModel.Device;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;

namespace VSS.MasterData.WebAPI.Repository.Device
{
	public class DeviceTypeService : IDeviceTypeService
	{
		private readonly MySqlDatabase database;
		private readonly Dictionary<string, DbDeviceType> deviceTypeIDCache = new Dictionary<string, DbDeviceType>(StringComparer.InvariantCultureIgnoreCase);
		private readonly ITransactions transactions;

		public DeviceTypeService(ILogger logger, IConfiguration configuration)
		{
			transactions = new ExecuteTransaction(configuration, logger);
			deviceTypeIDCache = GetDeviceTypeFromSource();
		}

		public Dictionary<string, DbDeviceType> GetDeviceType()
		{
			return deviceTypeIDCache;
		}

		private Dictionary<string, DbDeviceType> GetDeviceTypeFromSource()
		{
			var deviceTypes = transactions.Get<DbDeviceType>(Queries.ReadDeviceTypeQuery, null);
			var deviceTypeIDs = new Dictionary<string, DbDeviceType>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var deviceType in deviceTypes)
			{
				deviceTypeIDs.Add(deviceType.TypeName, new DbDeviceType { DeviceTypeID = deviceType.DeviceTypeID, TypeName = deviceType.TypeName, DefaultValueJson = deviceType.DefaultValueJson });
			}
			return deviceTypeIDs;
		}
	}
}