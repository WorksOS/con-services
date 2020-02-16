using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM
{
	public class CustomerService : ServiceBase, ICustomerService
	{
		private readonly ILog _log;
        private static readonly string CustomerApiBaseUri = ConfigurationManager.AppSettings["CustomerService.WebAPIURI"];

		public CustomerService()
		{
			_log = base.Logger;
		}
		public bool Create(object customerDetails)
		{
            var stringified = JsonConvert.SerializeObject(customerDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			try
			{
                _log.IfDebugFormat("Dispatching new customer to NextGen. Payload :{0}", stringified);
                var success = DispatchRequest(CustomerApiBaseUri, HttpMethod.Post, stringified);
                return success;
			}
			catch (Exception ex)
			{
				_log.IfWarnFormat("Error occurred while creating customer in VSP stack. Error message :{0}", ex.Message);
				return false;
			}

		}

        public bool Update(object customerDetails)
		{
            var stringified = JsonConvert.SerializeObject(customerDetails, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
			try
			{
                _log.IfDebugFormat("Updating customer to NextGen. Payload :{0}", stringified);
                var success = DispatchRequest(CustomerApiBaseUri, HttpMethod.Put, stringified);
                return success;
			}
			catch (Exception ex)
			{
				_log.IfWarnFormat("Error occurred while updating customer in VSP stack. Error message :{0}", ex.Message);
				return false;
			}
		}

		
		public bool Delete(Guid CustomerUID, DateTime ActionUTC)
		{
			try
			{
				string url = string.Format("{0}?CustomerUID={1}&ActionUTC={2}", CustomerApiBaseUri, CustomerUID,
					ActionUTC.ToString("s", CultureInfo.InvariantCulture));
                var success = DispatchRequest(url, HttpMethod.Delete);
                return success;
			}
			catch (Exception ex)
			{
				_log.IfWarnFormat("Error occurred while deleting customer in VSP stack. Error message :{0}", ex.Message);
				return false;
			}
		}
		
		public bool AssociateCustomerAsset(AssociateCustomerAssetEvent associateCustomerAssetEvent)
		{
			var stringified = JsonConvert.SerializeObject(associateCustomerAssetEvent, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			try
			{
				_log.IfDebugFormat("Associating customer-asset in NextGen. Payload :{0}", stringified);
				string uri = CustomerApiBaseUri + "/associatecustomerasset";
                var success = DispatchRequest(uri, HttpMethod.Post, stringified);
                return success;
			}
			catch (Exception ex)
			{
				_log.IfWarnFormat("Error occurred while associating customer-asset in VSP stack. Error message :{0}", ex.Message);
				return false;
			}
		}

		public bool AssociateCustomerUser(AssociateCustomerUserEvent associateCustomerUserEvent)
		{
			var stringified = JsonConvert.SerializeObject(associateCustomerUserEvent, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			try
			{
				_log.IfDebugFormat("Associating customer-user in NextGen. Payload :{0}", stringified);
				string uri = CustomerApiBaseUri + "/associatecustomeruser";
                var success = DispatchRequest(uri, HttpMethod.Post, stringified);
                return success;
			}
			catch (Exception ex)
			{
				_log.IfWarnFormat("Error occurred while associating customer-user in VSP stack. Error message :{0}", ex.Message);
				return false;
			}
		}

		public bool DissociateCustomerAsset(DissociateCustomerAssetEvent dissociateCustomerAssetEvent)
		{
			var stringified = JsonConvert.SerializeObject(dissociateCustomerAssetEvent, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			try
			{
				_log.IfDebugFormat("Dissociating customer-asset in NextGen. Payload :{0}", stringified);
				string uri = CustomerApiBaseUri + "/dissociatecustomerasset";
                var success = DispatchRequest(uri, HttpMethod.Post, stringified);
                return success;
			}
			catch (Exception ex)
			{
				_log.IfWarnFormat("Error occurred while dissociating customer-asset in VSP stack. Error message :{0}", ex.Message);
				return false;
			}
		}

		public bool DissociateCustomerUser(DissociateCustomerUserEvent dissociateCustomerUserEvent)
		{
			var stringified = JsonConvert.SerializeObject(dissociateCustomerUserEvent, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			try
			{
				_log.IfDebugFormat("Dissociating customer-user NextGen. Payload :{0}", stringified);
				string uri = CustomerApiBaseUri + "/dissociatecustomeruser";
                var success = DispatchRequest(uri, HttpMethod.Post, stringified);
                return success;
			}
			catch (Exception ex)
			{
				_log.IfWarnFormat("Error occurred while dissociating customer-user in VSP stack. Error message :{0}", ex.Message);
				return false;
			}
		}
	}
}
