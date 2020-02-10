using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Logging;
using Interfaces;
using DbModel;
using DbModel.DeviceConfig;
using VSS.MasterData.WebAPI.Transactions;

namespace DeviceConfigRepository.MySql.DeviceConfig
{
    public class DeviceTypeGroupParamAttributeRepository : IDeviceTypeGroupParamAttributeRepository
    {
        private readonly ITransactions _transactions;
        private readonly ILoggingService _loggingService;
        private const string logMethodFormat = "DeviceTypeGroupParamAttributeRepository.{0}";

        public DeviceTypeGroupParamAttributeRepository(ITransactions transactions, ILoggingService loggingService)
        {
            this._transactions = transactions;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
        }
        public async Task<IEnumerable<DeviceTypeGroupParamAttrDto>> Fetch(DeviceTypeGroupParamAttrDto request)
        {
            try
            {
                this._loggingService.Debug("Started executing query", string.Format(logMethodFormat, "Fetch"));

                this._loggingService.Info("Device Type Parameter Groups for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "Fetch"));

                var result = await this._transactions.GetAsync<DeviceTypeGroupParamAttrDto>(Queries.FetchDeviceTypesGroupsParametersAttributes);

                this._loggingService.Info("Device Type Parameter Groups : " + JsonConvert.SerializeObject(result), string.Format(logMethodFormat, "Fetch"));

                this._loggingService.Debug("Ended executing query", string.Format(logMethodFormat, "Fetch"));

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<DeviceTypeParameterAttribute> Fetch(int deviceTypeID, string parameterName, string attributeName, string deviceUID)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "DeviceTypeGroupParamAttributeRepository.Fetch");
                var response = await this._transactions.GetAsync<DeviceTypeParameterAttribute>(string.Format(Queries.SELECT_DEVICETYPEPARAMETERATTRIBUTE_FOR_A_DEVICE, deviceTypeID, parameterName, attributeName, deviceUID));
                return response.FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DeviceTypeParameterAttribute> Fetch(int deviceTypeID, string parameterName, string attributeName)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "DeviceTypeGroupParamAttributeRepository.Fetch");
                var response = await this._transactions.GetAsync<DeviceTypeParameterAttribute>(string.Format(Queries.SELECT_DEVICETYPEPARAMETERATTRIBUTE, deviceTypeID, parameterName, attributeName));
                return response.FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
