//using Interfaces;
//using CommonModel.AssetSettings;
//using ClientModel.AssetSettings.Request;
//using DbModel.AssetSettings;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Utilities.Logging;
//using VSS.MasterData.WebAPI.Transactions;

namespace AssetSettings.Controller
{
//namespace AssetSettings.Controller
//{
//	[Route("v1/deviceconfigs")]
//    public class DeviceConfigController : ApiControllerBase
//    {
//        private readonly ILoggingService _loggingService;
//        private readonly IDeviceParameterAttributeLookup _deviceParameterLookup;
//        private readonly IDeviceConfigRepository _deviceConfigRepo;
//		private readonly ITransactions _databaseManager;

//		public DeviceConfigController(ILoggingService loggingService, IDeviceConfigRepository deviceConfigRepo, IDatabaseManager databaseManager, IDeviceParameterAttributeLookup deviceParameterLookup)
//        {
//            _loggingService = loggingService;
//            _deviceConfigRepo = deviceConfigRepo;
//            _deviceParameterLookup = deviceParameterLookup;
//			_databaseManager = databaseManager;
//            _loggingService.CreateLogger(GetType());
//        }

//        [HttpPut]
//        [Route("")]
//        //[ChunkedEncodingFilter(typeof(PendingDeviceConfigRequest), "request")]
//        public async Task<ActionResult<PendingDeviceConfigRequest>> UpdateDeviceConfig([FromBody] PendingDeviceConfigRequest request)
//        {
//            try
//            {
//                var deviceConfigDtos = new List<DeviceConfigDto>();
//                foreach (var pendingConfig in request.PendingDeviceConfigs)
//                {
//                    _loggingService.Info(string.Format("Request: {0}", JsonConvert.SerializeObject(request)), "UpdateDeviceConfig");
//                    DeviceParameterAttribute param = _deviceParameterLookup.GetDeviceParameterAttribute(pendingConfig.DeviceType, pendingConfig.GroupName, pendingConfig.ParameterName, pendingConfig.AttributeName);
//                    deviceConfigDtos.AddRange(ToDeviceConfigDto(pendingConfig, param));
//                }
//				using (var dbConnection = _databaseManager.GetConnection())
//				{
//					var upsertQueryBuilder = _databaseManager.GetUpsertBuilder<DeviceConfigDto>();
//					upsertQueryBuilder.AddRows(deviceConfigDtos);
//					dbConnection.Upsert(upsertQueryBuilder);
//					dbConnection.Commit();
//				}
//                return base.SendResponse(HttpStatusCode.OK);
//            }
//            catch (Exception ex)
//            {
//                _loggingService.Error("Exception", "UpdateDeviceConfig", ex);
//                return base.SendResponse(HttpStatusCode.InternalServerError);
//            }
//        }

//		private IEnumerable<DeviceConfigDto> ToDeviceConfigDto(PendingDeviceConfig deviceConfig, DeviceParameterAttribute param)
//		{
//			return deviceConfig.DeviceUIDs.Select(deviceUID => new DeviceConfigDto()
//			{
//				DeviceUID = deviceUID,
//				DeviceParamAttrID = param.DeviceParamAttrID,
//				DeviceTypeParameterID = param.DeviceTypeParameterID,
//				FutureAttrEventUTC = deviceConfig.ActionUTC,
//				FutureAttributeValue = deviceConfig.AttributeValue,
//				LastDeviceConfigUTC = DateTime.UtcNow
//			});
//		}
//	}
//}
}