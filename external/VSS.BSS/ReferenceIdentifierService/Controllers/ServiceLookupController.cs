using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;

namespace VSS.Nighthawk.ReferenceIdentifierService.Controllers
{
  public class ServiceLookupController : ApiController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IServiceLookupManager _worker;

    public ServiceLookupController(IServiceLookupManager worker)
    {
      _worker = worker;
    }

    [HttpGet]
    public HttpResponseMessage GetAssetActiveServices(HttpRequestMessage request, Guid assetUid)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}", GetType().Name, "GetAssetActiveServices", assetUid);
      var response = new LookupResponse<List<Guid?>>();

      try
      {
        response.Data = _worker.GetAssetActiveServices(assetUid);
        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error getting active services for asset", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }

    [HttpGet]
    public HttpResponseMessage GetAssetActiveServices(HttpRequestMessage request, string serialNumber, string makeCode)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}, {3}", GetType().Name, "GetAssetActiveServices", serialNumber, makeCode);
      var response = new LookupResponse<IList<ServiceLookupItem>>();

      try
      {
        response.Data = _worker.GetAssetActiveServices(serialNumber, makeCode);
        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error getting active services for asset", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }

    [HttpGet]
    public HttpResponseMessage GetDeviceActiveServices(HttpRequestMessage request, string serialNumber, string deviceTypeString)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}, {3}", GetType().Name, "GetDeviceActiveServices", serialNumber, deviceTypeString);
      var response = new LookupResponse<IList<ServiceLookupItem>>();

      try
      {
        var deviceType = (DeviceTypeEnum)Enum.Parse(typeof(DeviceTypeEnum), deviceTypeString);
        response.Data = _worker.GetDeviceActiveServices(serialNumber, deviceType);
        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error getting active services for device", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }
  }
}
