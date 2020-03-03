using DbModel.Cache;
using Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace DeviceTypeParameterAttributeRepository
{
	public class DeviceTypeParameterAttributeRepository : IDeviceTypeParameterAttributeRepository
	{
		private readonly ITransactions _transactions;
		private readonly ILoggingService _loggingService;
		private const string logMethodFormat = "DeviceTypeParameterAttributeRepository.{0}";

		public DeviceTypeParameterAttributeRepository(ITransactions transactions, ILoggingService loggingService)
		{
			this._transactions = transactions;
			this._loggingService = loggingService;
			this._loggingService.CreateLogger(this.GetType());
		}
		public async Task<IEnumerable<DeviceTypeParameterAttributeDto>> Fetch(DeviceTypeParameterAttributeDto request)
		{
			try
			{
				this._loggingService.Debug("Started executing query", string.Format(logMethodFormat, "Fetch"));

				this._loggingService.Debug("Device Type Parameter Groups for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "Fetch"));

				var result = await this._transactions.GetAsync<DeviceTypeParameterAttributeDto>(Queries.FetchDeviceTypesGroupsParametersAttributes);

				this._loggingService.Debug("Device Type Parameter Groups : " + JsonConvert.SerializeObject(result), string.Format(logMethodFormat, "Fetch"));

				this._loggingService.Debug("Ended executing query", string.Format(logMethodFormat, "Fetch"));

				return result;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
