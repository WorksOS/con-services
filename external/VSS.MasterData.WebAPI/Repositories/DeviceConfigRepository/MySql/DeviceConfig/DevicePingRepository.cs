using ClientModel.DeviceConfig.Request;
using DbModel.DeviceConfig;
using DeviceConfigRepository.Helpers;
using Infrastructure.Common.DeviceSettings.Enums;
using Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace DeviceConfigRepository.MySql.DeviceConfig
{
    public class DevicePingRepository : IDevicePingRepository
    {
        private readonly ITransactions _transactions;
        private readonly ILoggingService _loggingService;
        private readonly IConfiguration _configuration;

        public DevicePingRepository(ITransactions transactions, IConfiguration configuration, ILoggingService loggingService)
        {
            this._transactions = transactions;
            this._configuration = configuration;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<PingRequestStatus> Fetch(DevicePingLogRequest devicePingLogRequest)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "");
                var response = await this._transactions.GetAsync<PingRequestStatus>(string.Format(Queries.FetchPingRequestStatusQuery,
                    devicePingLogRequest.AssetUID.ToStringWithoutHyphens().WrapWithUnhex(), devicePingLogRequest.DeviceUID.ToStringWithoutHyphens().WrapWithUnhex()));
                this._loggingService.Debug("Query Execution Completed", "");
                return response.ToList().FirstOrDefault();
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        public async Task<PingRequestStatus> Insert(DevicePingLogRequest devicePingLogRequest)
        {
            try
            {
                PingRequestStatus response = null;
                DateTime currentDateTime = DateTime.UtcNow;
                string devicePingRequestExpiry = "0";
                switch (devicePingLogRequest.FamilyName.ToUpper())
                {
                    case "PL":
                        devicePingRequestExpiry = _configuration["AppSettings:PL_PingInterval"];
                        break;
                    case "MTS":
                        devicePingRequestExpiry = _configuration["AppSettings:MTS_PingInterval"];
                        break;
                    case "DATAOUT":
                        devicePingRequestExpiry = _configuration["AppSettings:DataOut_PingInterval"];
                        break;
                    default:
                        throw new Exception("Device Family Type Not Supported");
                        break;
                }

                DateTime expiryDate = currentDateTime.AddSeconds(double.Parse(devicePingRequestExpiry));
                this._loggingService.Info("Started executing query", "DevicePingRepository.Insert");

                this._transactions.Upsert<PingRequestStatus>(new PingRequestStatus
                {
                    RequestStatusID = (int)RequestStatus.Pending,
                    RequestTimeUTC = currentDateTime,
                    RequestExpiryTimeUTC = expiryDate,
                    DevicePingLogUID = devicePingLogRequest.DevicePingLogUID,
                    AssetUID = devicePingLogRequest.AssetUID,
                    DeviceUID = devicePingLogRequest.DeviceUID
                });

                this._loggingService.Info("Completed query execution", "DevicePingRepository.Insert");

                response = new PingRequestStatus();
                response.AssetUID = devicePingLogRequest.AssetUID;
                response.DeviceUID = devicePingLogRequest.DeviceUID;
                response.DevicePingLogUID = devicePingLogRequest.DevicePingLogUID;
                response.RequestState = RequestStatus.Pending.ToString();
                response.RequestExpiryTimeUTC = expiryDate;
                response.RequestTimeUTC = DateTime.Parse(currentDateTime.ToString("yyyy-MM-dd HH:mm:ss"));

                return response;

            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        public async Task<DeviceTypeFamily> GetDeviceTypeFamily(Guid deviceUID)
        {
            try
            {
                this._loggingService.Debug("Started executing query", Queries.DeviceTypeFamilyQuery);
                var response = await this._transactions.GetAsync<DeviceTypeFamily>(string.Format(Queries.DeviceTypeFamilyQuery, deviceUID.ToStringWithoutHyphens().WrapWithUnhex()));
                this._loggingService.Debug("Query Execution Completed", Queries.DeviceTypeFamilyQuery);
                return response.ToList().FirstOrDefault();
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }
    }
}
