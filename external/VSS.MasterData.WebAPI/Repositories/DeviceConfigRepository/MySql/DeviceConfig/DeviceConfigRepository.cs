using DbModel.DeviceConfig;
using Infrastructure.Common.DeviceSettings.Enums;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace DeviceConfigRepository.MySql.DeviceConfig
{
	public class DeviceConfigRepository : IDeviceConfigRepository
    {
        private readonly ITransactions _transaction;
        private readonly ILoggingService _loggingService;
        private const string logMethodFormat = "DeviceConfigRepository.{0}";

		public DeviceConfigRepository(ITransactions transaction, ILoggingService loggingService)
        {
            this._transaction = transaction;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<IEnumerable<DeviceConfigDto>> Fetch(IList<string> assetUIDs, IList<DeviceConfigDto> deviceConfigDtos)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "");
				var response = await this._transaction.GetAsync<DeviceConfigDto>(string.Format(Queries.FetchDeviceConfig,
					string.Join(",", assetUIDs.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty)), //Asset UIDs Lists
					new
					{
						DeviceTypeParameterIDs = deviceConfigDtos.Select(x => x.DeviceTypeParameterID),
						DeviceParamAttrIDs = deviceConfigDtos.Select(x => x.DeviceParameterAttributeId)
					});
                return response;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        public async Task<DeviceConfigDto> Insert(DeviceConfigDto deviceConfigDto)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "");
                this._transaction.Upsert( new DeviceConfigDto
				{
                    DeviceTypeParameterID = deviceConfigDto.DeviceTypeParameterID,
                    DeviceParameterAttributeId = deviceConfigDto.DeviceParameterAttributeId,
                    FutureAttributeValue = deviceConfigDto.FutureAttributeValue,
                    FutureAttrEventUTC = deviceConfigDto.FutureAttrEventUTC,
                    RowInsertedUTC = deviceConfigDto.RowInsertedUTC,
                    UpdateUTC = deviceConfigDto.RowUpdatedUTC,
                    AssetUIDString = deviceConfigDto.AssetUIDString,
                    DeviceConfigID = deviceConfigDto.DeviceConfigID
                });
                return deviceConfigDto;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<DeviceConfigDto>> Insert(IList<DeviceConfigDto> deviceConfigDtos)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "");
                Parallel.ForEach(deviceConfigDtos, async deviceConfigDto =>
                {
					this._transaction.Upsert(new DeviceConfigDto
					{
						DeviceTypeParameterID = deviceConfigDto.DeviceTypeParameterID,
						DeviceParameterAttributeId = deviceConfigDto.DeviceParameterAttributeId,
						FutureAttributeValue = deviceConfigDto.FutureAttributeValue,
						FutureAttrEventUTC = deviceConfigDto.FutureAttrEventUTC,
						RowInsertedUTC = deviceConfigDto.RowInsertedUTC,
						UpdateUTC = deviceConfigDto.RowUpdatedUTC,
						AssetUIDString = deviceConfigDto.AssetUIDString,
						DeviceConfigID = deviceConfigDto.DeviceConfigID
					});
				});
                return deviceConfigDtos;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        public async Task<DeviceConfigDto> Update(DeviceConfigDto deviceConfigDto)
        {
            try
            {
				this._transaction.Upsert(new DeviceConfigDto
				{
					DeviceTypeParameterID = deviceConfigDto.DeviceTypeParameterID,
					DeviceParameterAttributeId = deviceConfigDto.DeviceParameterAttributeId,
					FutureAttributeValue = deviceConfigDto.FutureAttributeValue,
					FutureAttrEventUTC = deviceConfigDto.FutureAttrEventUTC,
					RowInsertedUTC = deviceConfigDto.RowInsertedUTC,
					UpdateUTC = deviceConfigDto.RowUpdatedUTC,
					AssetUIDString = deviceConfigDto.AssetUIDString,
					DeviceConfigID = deviceConfigDto.DeviceConfigID
				});
				return deviceConfigDto;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        public async Task<bool> UpdateCurrentValue(DeviceConfigDto deviceConfigDto)
        {
            try
            {
				this._transaction.Upsert(new DeviceConfigDto
				{
					DeviceTypeParameterID = deviceConfigDto.DeviceTypeParameterID,
					DeviceParameterAttributeId = deviceConfigDto.DeviceParameterAttributeId,
					FutureAttributeValue = deviceConfigDto.FutureAttributeValue,
					FutureAttrEventUTC = deviceConfigDto.FutureAttrEventUTC,
					RowInsertedUTC = deviceConfigDto.RowInsertedUTC,
					UpdateUTC = deviceConfigDto.RowUpdatedUTC,
					AssetUIDString = deviceConfigDto.AssetUIDString,
					DeviceConfigID = deviceConfigDto.DeviceConfigID
				});
				return true;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        public async Task<bool> Upsert(DeviceConfigDto deviceConfigDto)
        {
            try
            {
				this._transaction.Upsert(new DeviceConfigDto
				{
					DeviceTypeParameterID = deviceConfigDto.DeviceTypeParameterID,
					DeviceParameterAttributeId = deviceConfigDto.DeviceParameterAttributeId,
					FutureAttributeValue = deviceConfigDto.FutureAttributeValue,
					FutureAttrEventUTC = deviceConfigDto.FutureAttrEventUTC,
					RowInsertedUTC = deviceConfigDto.RowInsertedUTC,
					UpdateUTC = deviceConfigDto.RowUpdatedUTC,
					AssetUIDString = deviceConfigDto.AssetUIDString,
					DeviceConfigID = deviceConfigDto.DeviceConfigID
				});
				return true;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<DeviceConfigDto>> Update(IList<DeviceConfigDto> deviceConfigDtos)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "");
                Parallel.ForEach(deviceConfigDtos, async deviceConfigDto =>
                {
					this._transaction.Upsert(new DeviceConfigDto
					{
						DeviceTypeParameterID = deviceConfigDto.DeviceTypeParameterID,
						DeviceParameterAttributeId = deviceConfigDto.DeviceParameterAttributeId,
						FutureAttributeValue = deviceConfigDto.FutureAttributeValue,
						FutureAttrEventUTC = deviceConfigDto.FutureAttrEventUTC,
						RowInsertedUTC = deviceConfigDto.RowInsertedUTC,
						UpdateUTC = deviceConfigDto.RowUpdatedUTC,
						AssetUIDString = deviceConfigDto.AssetUIDString,
						DeviceConfigID = deviceConfigDto.DeviceConfigID
					});
				});
                return deviceConfigDtos;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        public async Task<IEnumerable<DeviceConfigDto>> FetchDeviceConfigByParameterNames(List<string> parameterNames, bool getMaintenanceModeList = false)
        {
            try
            {
                if (parameterNames != null && parameterNames.Count > 0)
                {
                    var query = getMaintenanceModeList ? Queries.FETCH_MAINTENANCE_MODE_ON_DEVICES_RECORD + @"AND ((DC1.fk_DeviceParamAttrID = @StatusId AND DC1.AttributeValue = 'true') OR (DC1.fk_DeviceParamAttrID = @MaintenanceId AND DC1.AttributeValue != ''))" : Queries.FETCH_MAINTENANCE_MODE_ON_DEVICES_RECORD;
                    var response = await this._transaction.GetAsync<DeviceConfigDto>(query, new
                    {
                        ParamsList = parameterNames,
                        StatusId = (int)ParameterName.Status,
                        MaintenanceId = (int)ParameterName.MaintenanceModeDuration
                    });
                    return response;
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
