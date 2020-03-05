//TODO: Check if its not needed, This class not needed since all are in same db with different tables.

//using Newtonsoft.Json;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using VSS.MasterData.Asset.Common.Models.Common;
//using VSS.MasterData.Asset.DataAccess.MySql.Interfaces;
//using VSS.MasterData.Asset.DataAccess.MySql.Models;
//using VSS.Utilities.Logging;

//namespace AssetSettingsRepository
//{
//	public class DeviceConfigRepository : IDeviceConfigRepository
//    {
//		private readonly IConnection _connection;
//		private readonly ILoggingService _loggingService;

//		public DeviceConfigRepository(IConnection connection, ILoggingService loggingService)
//        {
//			this._connection = connection;
//			this._loggingService = loggingService;
//			this._loggingService.CreateLogger(this.GetType());
//		}

//        public async Task<List<DeviceParameterAttribute>> GetDeviceParameterAttributes()
//        {
//            return (await _connection.FetchAsync<DeviceParameterAttribute>(Queries.SELECT_DEVICE_PARAMETR_ATTRIBUTE)).ToList();
//        }

//        public async Task CreateDeviceConfig(List<DeviceConfigDto> dtos)
//        {
//			_loggingService.Debug(string.Format("DeviceConfigDTO: {0}", JsonConvert.SerializeObject(dtos)), "DeviceConfigRepository.CreateDeviceConfig");
//			await _connection.ExecuteAsync(Queries.UPSERT_DEVICE_CONFIG, dtos);
//        }
//    }
//}
