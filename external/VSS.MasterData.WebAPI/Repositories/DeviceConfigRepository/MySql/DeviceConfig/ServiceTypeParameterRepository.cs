using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;
using Interfaces;
using DbModel;
using DbModel.DeviceConfig;
using VSS.MasterData.WebAPI.Transactions;
using DbModel.Cache;

namespace DeviceConfigRepository.MySql.DeviceConfig
{
	public class ServiceTypeParameterRepository : IServiceTypeParameterRepository
	{
		private ILoggingService _loggingService;
		private ITransactions _transactions;

		public ServiceTypeParameterRepository(ITransactions transactions, ILoggingService loggingService)
		{
			this._transactions = transactions;
			this._loggingService = loggingService;
			this._loggingService.CreateLogger(this.GetType());
		}

		#region IServiceTypeParameterRepository implementation
		public async Task<IEnumerable<ServiceTypeParameterDto>> FetchAllServiceTypeParameter()
		{
			try
			{
				this._loggingService.Debug("Started executing query", "ServiceTypeParameterRepository.FetchAllServiceTypeParameter");
				var result = await this._transactions.GetAsync<ServiceTypeParameterDto>(Queries.FETCH_ALL_SERVICETYPE_PARAMETER);
				return result;
			}
			catch (Exception ex)
			{
				this._loggingService.Error("Exception occurred while executing query", "ServiceTypeParameterRepository.FetchAllServiceTypeParameter", ex);
				throw ex;
			}
		}
		#endregion
	}
}
