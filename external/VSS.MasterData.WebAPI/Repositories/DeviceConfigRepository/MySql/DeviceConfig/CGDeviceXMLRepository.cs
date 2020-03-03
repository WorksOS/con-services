//TODO: Remove, if Not needed
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Utilities.Logging;
//using Interfaces;
//using DbModel;
//using DbModel.DeviceConfig;

//namespace DeviceConfigRepository.MySql.DeviceConfig
//{
//    public class CGDeviceXMLRepository : ICGDeviceXMLRepository
//    {
//        private ILoggingService _loggingService;
//        private IConnection _connection;

//        public CGDeviceXMLRepository(IConnection connection, ILoggingService loggingService)
//        {
//            _connection = connection;
//            _loggingService = loggingService;
//            this._loggingService.CreateLogger(this.GetType());
//        }

//        public Task<IEnumerable<CGNGMap>> GetAllCGNGMap()
//        {
//            _loggingService.Info("Started Fetching AssetDevice Details", "CGDeviceXMLRepository.GetAllCGNGMap");
//            string query = "SELECT CG_Element, CG_Attribute, NG_Parameter, NG_Attribute FROM CG_NG_Map;";
//            _loggingService.Info(query, "CGDeviceXMLRepository.GetAllCGNGMap");
//            return _connection.FetchAsync<CGNGMap>(query);
//        }
//    }
//}
